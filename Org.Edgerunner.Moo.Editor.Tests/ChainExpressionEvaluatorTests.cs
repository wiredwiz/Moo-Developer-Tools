using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class ChainExpressionEvaluatorTests
{
   // A property-object resolver backed by a (objectId, propName) -> #N map; counts calls.
   private sealed class FakeWorld
   {
      public Dictionary<(int Obj, string Prop), MooObjectId?> Map { get; } = new();
      public int Calls;
      public Exception? Throw;

      public Task<MooObjectId?> ResolveAsync(MooObjectId obj, string prop, CancellationToken ct)
      {
         Calls++;
         if (Throw is not null)
            throw Throw;
         Map.TryGetValue((obj.Number, prop), out var result);
         return Task.FromResult(result);
      }
   }

   private static ChainDescriptor Chain(ChainBase chainBase, params string[] steps) =>
      new(chainBase, steps, MemberContextKind.Property, string.Empty);

   // Convenience: a flow value that reassigns a base to a ground chain.
   private static FlowValue Reassigned(ChainBase chainBase, params string[] steps) =>
      new(FlowValueKind.Reassigned, new ChainDescriptor(chainBase, steps, MemberContextKind.Property, string.Empty));

   private static ChainExpressionEvaluator CreateEvaluator(
      FakeWorld world,
      MooObjectId? currentPlayer = null,
      Func<string, FlowValue>? flowResolver = null,
      Action<string, Exception?>? diagnostic = null)
   {
      return new ChainExpressionEvaluator(
         world.ResolveAsync,
         _ => Task.FromResult(currentPlayer),
         flowResolver ?? (name => new FlowValue(
            // Keywords default; bare locals are unknown.
            name is "this" or "player" or "caller" ? FlowValueKind.UseDefault : FlowValueKind.Unknown, null)),
         diagnostic);
   }

   [Fact]
   public async Task Resolves_object_literal_base()
   {
      var world = new FakeWorld();
      var evaluator = CreateEvaluator(world);

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.ObjectLiteral, "42")), null, CancellationToken.None);

      result.Should().Be(new MooObjectId(42));
   }

   [Fact]
   public async Task Resolves_this_base_to_context_object()
   {
      var world = new FakeWorld();
      var evaluator = CreateEvaluator(world);

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.This, string.Empty)), new MooObjectId(5), CancellationToken.None);

      result.Should().Be(new MooObjectId(5));
   }

   [Fact]
   public async Task This_base_without_context_object_yields_null()
   {
      var world = new FakeWorld();
      var evaluator = CreateEvaluator(world);

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.This, string.Empty)), null, CancellationToken.None);

      result.Should().BeNull();
   }

   [Theory]
   [InlineData(ChainBaseKind.Player)]
   [InlineData(ChainBaseKind.Caller)]
   public async Task Resolves_player_and_caller_via_current_player(ChainBaseKind kind)
   {
      var world = new FakeWorld();
      var evaluator = CreateEvaluator(world, currentPlayer: new MooObjectId(62));

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(kind, string.Empty)), null, CancellationToken.None);

      result.Should().Be(new MooObjectId(62));
   }

   [Fact]
   public async Task Resolves_core_name_base_via_object_zero_property()
   {
      var world = new FakeWorld();
      world.Map[(0, "foo")] = new MooObjectId(62);
      var evaluator = CreateEvaluator(world);

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.CoreName, "foo")), null, CancellationToken.None);

      result.Should().Be(new MooObjectId(62));
   }

   [Fact]
   public async Task Resolves_two_step_core_name_chain()
   {
      var world = new FakeWorld();
      world.Map[(0, "foo")] = new MooObjectId(62);   // $foo -> #62
      world.Map[(62, "bar")] = new MooObjectId(70);  // #62.bar -> #70
      var evaluator = CreateEvaluator(world);

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.CoreName, "foo"), "bar"), null, CancellationToken.None);

      result.Should().Be(new MooObjectId(70));
   }

   [Fact]
   public async Task Resolves_deep_object_literal_chain()
   {
      var world = new FakeWorld();
      world.Map[(1, "a")] = new MooObjectId(2);
      world.Map[(2, "b")] = new MooObjectId(3);
      world.Map[(3, "c")] = new MooObjectId(4);
      var evaluator = CreateEvaluator(world);

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.ObjectLiteral, "1"), "a", "b", "c"), null, CancellationToken.None);

      result.Should().Be(new MooObjectId(4));
   }

   [Fact]
   public async Task Non_object_intermediate_step_yields_null()
   {
      var world = new FakeWorld();
      world.Map[(0, "foo")] = new MooObjectId(62);
      // #62.bar is not an object (absent from the map -> null).
      var evaluator = CreateEvaluator(world);

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.CoreName, "foo"), "bar"), null, CancellationToken.None);

      result.Should().BeNull();
   }

   [Fact]
   public async Task Missing_core_name_yields_null()
   {
      var world = new FakeWorld();
      var evaluator = CreateEvaluator(world);

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.CoreName, "nope")), null, CancellationToken.None);

      result.Should().BeNull();
   }

   [Fact]
   public async Task Resolves_variable_base_to_its_rhs_object()
   {
      var world = new FakeWorld();
      world.Map[(0, "foo")] = new MooObjectId(62);  // $foo -> #62
      // x = $foo  =>  the flow resolver hands back the already-grounded $foo chain
      var evaluator = CreateEvaluator(
         world,
         flowResolver: name => name == "x"
            ? Reassigned(new ChainBase(ChainBaseKind.CoreName, "foo"))
            : new FlowValue(FlowValueKind.Unknown, null));

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.Variable, "x")), null, CancellationToken.None);

      result.Should().Be(new MooObjectId(62));
   }

   [Fact]
   public async Task Resolves_variable_base_then_walks_a_property_step()
   {
      var world = new FakeWorld();
      world.Map[(0, "foo")] = new MooObjectId(62);   // $foo -> #62
      world.Map[(62, "bar")] = new MooObjectId(70);  // #62.bar -> #70
      // x = $foo; then x.bar should resolve to #70 (ground.Steps ++ chain.Steps).
      var evaluator = CreateEvaluator(
         world,
         flowResolver: name => name == "x"
            ? Reassigned(new ChainBase(ChainBaseKind.CoreName, "foo"))
            : new FlowValue(FlowValueKind.Unknown, null));

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.Variable, "x"), "bar"), null, CancellationToken.None);

      result.Should().Be(new MooObjectId(70));
   }

   [Fact]
   public async Task Reassigned_base_merges_ground_steps_before_chain_steps()
   {
      var world = new FakeWorld();
      world.Map[(0, "foo")] = new MooObjectId(62);   // $foo -> #62
      world.Map[(62, "package")] = new MooObjectId(70);  // #62.package -> #70
      world.Map[(70, "bar")] = new MooObjectId(80);  // #70.bar -> #80
      // pack = $foo.package; then pack.bar -> ground steps [package] ++ chain steps [bar].
      var evaluator = CreateEvaluator(
         world,
         flowResolver: name => name == "pack"
            ? Reassigned(new ChainBase(ChainBaseKind.CoreName, "foo"), "package")
            : new FlowValue(FlowValueKind.Unknown, null));

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.Variable, "pack"), "bar"), null, CancellationToken.None);

      result.Should().Be(new MooObjectId(80));
   }

   [Fact]
   public async Task Unknown_variable_yields_null()
   {
      var world = new FakeWorld();
      var evaluator = CreateEvaluator(world, flowResolver: _ => new FlowValue(FlowValueKind.Unknown, null));

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.Variable, "x")), null, CancellationToken.None);

      result.Should().BeNull();
   }

   [Fact]
   public async Task Reassigned_player_resolves_the_new_object_not_the_default()
   {
      var world = new FakeWorld();
      // The default player is #62, but player was reassigned to #10.
      var evaluator = CreateEvaluator(
         world,
         currentPlayer: new MooObjectId(62),
         flowResolver: name => name == "player"
            ? Reassigned(new ChainBase(ChainBaseKind.ObjectLiteral, "10"))
            : new FlowValue(FlowValueKind.Unknown, null));

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.Player, string.Empty)), null, CancellationToken.None);

      result.Should().Be(new MooObjectId(10));
   }

   [Fact]
   public async Task Use_default_keyword_falls_back_to_default_player()
   {
      var world = new FakeWorld();
      var evaluator = CreateEvaluator(
         world,
         currentPlayer: new MooObjectId(62),
         flowResolver: _ => new FlowValue(FlowValueKind.UseDefault, null));

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.Player, string.Empty)), null, CancellationToken.None);

      result.Should().Be(new MooObjectId(62));
   }

   [Fact]
   public async Task Depth_budget_exceeded_yields_null()
   {
      var world = new FakeWorld();
      // A long property chain #1.a.a.a... exceeding the 8-step budget. Map every #N.a -> #(N+1).
      for (var n = 1; n <= 30; n++)
         world.Map[(n, "a")] = new MooObjectId(n + 1);
      var evaluator = CreateEvaluator(world);

      var steps = new string[20];
      Array.Fill(steps, "a");
      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.ObjectLiteral, "1"), steps), null, CancellationToken.None);

      result.Should().BeNull("a 20-step chain exceeds the 8-step resolution budget");
   }

   [Fact]
   public async Task Provider_throw_is_swallowed_and_routed_to_diagnostic()
   {
      var world = new FakeWorld { Throw = new TimeoutException("world query timed out") };
      world.Map[(0, "foo")] = new MooObjectId(62);
      string? message = null;
      Exception? exception = null;
      var evaluator = CreateEvaluator(world, diagnostic: (m, e) => { message = m; exception = e; });

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.CoreName, "foo")), null, CancellationToken.None);

      result.Should().BeNull();
      message.Should().NotBeNullOrEmpty();
      exception.Should().BeOfType<TimeoutException>();
   }

   [Fact]
   public async Task Expected_non_resolution_does_not_invoke_diagnostic()
   {
      var world = new FakeWorld(); // $foo absent -> expected non-resolution, no exception
      var diagnosticInvoked = false;
      var evaluator = CreateEvaluator(world, diagnostic: (_, _) => diagnosticInvoked = true);

      var result = await evaluator.EvaluateAsync(
         Chain(new ChainBase(ChainBaseKind.CoreName, "foo")), null, CancellationToken.None);

      result.Should().BeNull();
      diagnosticInvoked.Should().BeFalse("a chain that simply does not resolve must not be logged as a warning");
   }
}
