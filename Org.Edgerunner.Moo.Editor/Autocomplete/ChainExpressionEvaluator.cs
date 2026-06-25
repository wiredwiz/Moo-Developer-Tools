#region BSD 3-Clause License
// <copyright file="ChainExpressionEvaluator.cs" company="Edgerunner.org">
// Copyright 2020
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2022,
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Org.Edgerunner.Mud.Common.Querying;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Resolves a <see cref="ChainDescriptor"/> to a final <see cref="MooObjectId"/> by resolving its base
/// and walking each property step through the world, recursing into local-variable bases.
/// </summary>
/// <remarks>
/// World-state property-to-object resolution (<c>(objectId, propName) -> #N</c>) is supplied by the
/// caller so that it can be TTL-cached per connection; the evaluator simply invokes it. A top-level
/// keyword (<c>this</c>/<c>player</c>/<c>caller</c>) or variable base is resolved <em>once</em> by the
/// supplied flow-value delegate (bound by the caller to the live parse tree and caret); the delegate
/// returns a fully reduced GROUND chain (no variable base, offsets already applied), which the evaluator
/// resolves with keyword bases meaning <em>default</em>. All variable/offset logic therefore lives in the
/// pure resolver, not here. A depth budget bounds the total number of property steps; every unresolved
/// step degrades to <c>null</c> (no source). Expected non-resolution is not logged; only unexpected
/// exceptions are routed to the optional diagnostic delegate.
/// </remarks>
public sealed class ChainExpressionEvaluator
{
   /// <summary>The maximum number of resolution steps (variable hops plus property steps) per request.</summary>
   public const int MaxResolutionSteps = 8;

   private readonly Func<MooObjectId, string, CancellationToken, Task<MooObjectId?>> _resolvePropertyObject;

   private readonly Func<CancellationToken, Task<MooObjectId?>> _getCurrentPlayer;

   private readonly Func<string, FlowValue> _resolveFlowValue;

   private readonly Action<string, Exception?>? _diagnostic;

   /// <summary>
   /// Initializes a new instance of the <see cref="ChainExpressionEvaluator"/> class.
   /// </summary>
   /// <param name="resolvePropertyObject">
   /// Resolves <c>(objectId, propName)</c> to the object the property holds, or <c>null</c> when the
   /// property is missing or not an object. The caller wraps this with its world-state TTL cache.
   /// </param>
   /// <param name="getCurrentPlayer">Resolves the connected player object (for <c>player</c>/<c>caller</c>).</param>
   /// <param name="resolveFlowValue">
   /// Resolves a top-level <c>this</c>/<c>player</c>/<c>caller</c>/variable base to its flow value (bound to
   /// the live parse tree and caret): a fully reduced GROUND chain when reassigned, <c>UseDefault</c> to fall
   /// back to the keyword default, or <c>Unknown</c> for no source.
   /// </param>
   /// <param name="diagnostic">Optional sink for unexpected (logged) failures; expected unknowns pass silently.</param>
   /// <exception cref="ArgumentNullException">Thrown when any required delegate is <c>null</c>.</exception>
   public ChainExpressionEvaluator(
      Func<MooObjectId, string, CancellationToken, Task<MooObjectId?>> resolvePropertyObject,
      Func<CancellationToken, Task<MooObjectId?>> getCurrentPlayer,
      Func<string, FlowValue> resolveFlowValue,
      Action<string, Exception?>? diagnostic = null)
   {
      _resolvePropertyObject = resolvePropertyObject ?? throw new ArgumentNullException(nameof(resolvePropertyObject));
      _getCurrentPlayer = getCurrentPlayer ?? throw new ArgumentNullException(nameof(getCurrentPlayer));
      _resolveFlowValue = resolveFlowValue ?? throw new ArgumentNullException(nameof(resolveFlowValue));
      _diagnostic = diagnostic;
   }

   /// <summary>
   /// Resolves the supplied chain to its final object id.
   /// </summary>
   /// <param name="chain">The chain to resolve.</param>
   /// <param name="contextObjectId">The object the edited verb lives on (the meaning of <c>this</c>), when known.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>The resolved object id, or <c>null</c> when the chain does not resolve.</returns>
   public async Task<MooObjectId?> EvaluateAsync(
      ChainDescriptor chain, MooObjectId? contextObjectId, CancellationToken cancellationToken)
   {
      try
      {
         var budget = new ResolutionBudget(MaxResolutionSteps);

         switch (chain.Base.Kind)
         {
            case ChainBaseKind.This:
            case ChainBaseKind.Player:
            case ChainBaseKind.Caller:
            case ChainBaseKind.Variable:
            {
               // Consult the flow resolver ONCE for the top-level keyword/variable base.
               var flow = _resolveFlowValue(chain.Base.Text.Length > 0 ? chain.Base.Text : KeywordName(chain.Base.Kind));
               switch (flow.Kind)
               {
                  case FlowValueKind.Reassigned when flow.Chain is not null:
                     // Resolve the reduced ground base + (ground.Steps ++ chain.Steps).
                     var mergedSteps = new List<string>(flow.Chain.Steps);
                     mergedSteps.AddRange(chain.Steps);
                     return await ResolveGroundAsync(flow.Chain.Base, mergedSteps, contextObjectId, budget, cancellationToken)
                               .ConfigureAwait(false);

                  case FlowValueKind.UseDefault:
                     // Fall back to the keyword default (this/player/caller); a Variable base never returns this.
                     return await ResolveGroundAsync(chain.Base, chain.Steps, contextObjectId, budget, cancellationToken)
                               .ConfigureAwait(false);

                  default:
                     // Unknown -> no source.
                     return null;
               }
            }

            default:
               // ObjectLiteral / CoreName: already ground, resolve directly.
               return await ResolveGroundAsync(chain.Base, chain.Steps, contextObjectId, budget, cancellationToken)
                         .ConfigureAwait(false);
         }
      }
      catch (OperationCanceledException)
      {
         return null;
      }
      catch (Exception ex)
      {
         _diagnostic?.Invoke($"Chain evaluation failed for '{Describe(chain)}'.", ex);
         return null;
      }
   }

   // Resolves a GROUND chain (base in { ObjectLiteral, CoreName, This, Player, Caller }, never Variable),
   // with keyword bases meaning DEFAULT (no flow re-entry), then walks the property steps.
   private async Task<MooObjectId?> ResolveGroundAsync(
      ChainBase chainBase,
      IReadOnlyList<string> steps,
      MooObjectId? contextObjectId,
      ResolutionBudget budget,
      CancellationToken cancellationToken)
   {
      MooObjectId? current;
      switch (chainBase.Kind)
      {
         case ChainBaseKind.ObjectLiteral:
            // A literal #N base resolves directly (negatives included, mirroring the legacy resolver);
            // the world step-walk is what requires N >= 0 for resolved property values.
            current = int.TryParse(chainBase.Text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number)
               ? new MooObjectId(number)
               : null;
            break;

         case ChainBaseKind.This:
            current = contextObjectId;
            break;

         case ChainBaseKind.Player:
         case ChainBaseKind.Caller:
            current = await _getCurrentPlayer(cancellationToken).ConfigureAwait(false);
            break;

         case ChainBaseKind.CoreName:
            if (!budget.TryConsume())
               return null;
            // $name is the property `name` on #0, required to be an object.
            current = await _resolvePropertyObject(new MooObjectId(0), chainBase.Text, cancellationToken)
                         .ConfigureAwait(false);
            break;

         default:
            // A Variable base must never reach here (the resolver guarantees full reduction).
            return null;
      }

      if (current is null)
         return null;

      foreach (var step in steps)
      {
         if (!budget.TryConsume())
            return null;

         // A missing or non-object property is an expected non-resolution (no source), not a warning.
         current = await _resolvePropertyObject(current.Value, step, cancellationToken).ConfigureAwait(false);
         if (current is null)
            return null;
      }

      return current;
   }

   private static string KeywordName(ChainBaseKind kind) => kind switch
   {
      ChainBaseKind.This => "this",
      ChainBaseKind.Player => "player",
      ChainBaseKind.Caller => "caller",
      _ => string.Empty,
   };

   private static string Describe(ChainDescriptor chain)
   {
      var basePart = chain.Base.Kind switch
      {
         ChainBaseKind.ObjectLiteral => $"#{chain.Base.Text}",
         ChainBaseKind.CoreName => $"${chain.Base.Text}",
         ChainBaseKind.This => "this",
         ChainBaseKind.Player => "player",
         ChainBaseKind.Caller => "caller",
         _ => chain.Base.Text,
      };
      var steps = chain.Steps.Count > 0 ? "." + string.Join(".", chain.Steps) : string.Empty;
      var separator = chain.MemberKind == MemberContextKind.Verb ? ":" : ".";
      return $"{basePart}{steps}{separator}{chain.PartialFragment}";
   }

   // A shared, mutable step budget for one resolution pass.
   private sealed class ResolutionBudget
   {
      private int _remaining;

      public ResolutionBudget(int budget) => _remaining = budget;

      public bool TryConsume()
      {
         if (_remaining <= 0)
            return false;
         _remaining--;
         return true;
      }
   }
}
