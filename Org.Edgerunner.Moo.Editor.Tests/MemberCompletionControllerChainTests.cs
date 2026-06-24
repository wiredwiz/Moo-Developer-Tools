using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using FastColoredTextBoxNS.Types;
using FluentAssertions;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class MemberCompletionControllerChainTests
{
   private sealed class FakeQueryProvider : IMooWorldQueryProvider
   {
      public List<MooVerbSummary> Verbs { get; } = new();
      public List<MooPropertySummary> Properties { get; } = new();
      public Dictionary<(int Obj, string Prop), MooPropertyValue?> PropertyValues { get; } = new();
      public int PropValueCalls;
      public MooObjectId? LastVerbQueryObjectId;

      public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
      {
         Interlocked.Increment(ref PropValueCalls);
         PropertyValues.TryGetValue((objectId.Number, propName), out var value);
         return Task.FromResult(value);
      }

      public Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken)
      {
         LastVerbQueryObjectId = objectId;
         return Task.FromResult<IReadOnlyList<MooVerbSummary>>(Verbs);
      }

      public Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
         Task.FromResult<IReadOnlyList<MooPropertySummary>>(Properties);

      public Task<MooObjectId?> GetCurrentPlayerAsync(CancellationToken cancellationToken) => Task.FromResult<MooObjectId?>(null);
      public Task<IReadOnlyList<MooObjectSummary>> GetCoreObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
   }

   private static (ParserRuleContext Tree, IList<DetailedToken> Tokens) ParseBuffer(string buffer)
   {
      var inputStream = new AntlrInputStream(buffer);
      var lexer = Org.Edgerunner.Moo.Editor.Moo.GetLexer(GrammarDialect.Edgerunner, inputStream);
      lexer.TokenFactory = DetailedTokenFactory.Instance;
      var stream = new CommonTokenStream(lexer);
      stream.Fill();
      var tokens = stream.GetTokens().Cast<DetailedToken>().ToList();
      var parser = Org.Edgerunner.Moo.Editor.Moo.GetParser(GrammarDialect.Edgerunner, stream);
      parser.RemoveErrorListeners();
      var tree = (ParserRuleContext)parser.GetType().GetMethod("code")!.Invoke(parser, null)!;
      return (tree, tokens);
   }

   private static MemberCompletionController CreateController(
      FakeQueryProvider provider, MooObjectId? contextObject = null, Action<string, Exception?>? diagnostic = null)
   {
      return new MemberCompletionController(
         () => provider,
         () => contextObject,
         action => action(),
         () => { },
         diagnostic: diagnostic);
   }

   // Drives a chain request: parse buffer, caret at end, poll until members land.
   private static IReadOnlyList<AutocompleteItem> RequestChain(MemberCompletionController controller, string buffer)
   {
      var (tree, tokens) = ParseBuffer(buffer);
      var lines = buffer.Split('\n');
      var caretLine = lines.Length;
      var caretColumn = lines[^1].Length + 1;
      var linePrefix = lines[^1];

      controller.GetMemberItems(linePrefix, tree, tokens, caretLine, caretColumn); // start
      SpinWait.SpinUntil(
         () => controller.GetMemberItems(linePrefix, tree, tokens, caretLine, caretColumn).Count > 0,
         TimeSpan.FromSeconds(5));
      return controller.GetMemberItems(linePrefix, tree, tokens, caretLine, caretColumn);
   }

   [Fact]
   public void Chain_core_property_resolves_through_two_steps_and_fetches_verbs()
   {
      var provider = new FakeQueryProvider();
      provider.PropertyValues[(0, "Mcp")] = new MooPropertyValue(1, "#123");
      provider.PropertyValues[(123, "package")] = new MooPropertyValue(1, "#456");
      provider.Verbs.Add(new MooVerbSummary(new[] { "send" }, new MooObjectId(456)));
      using var controller = CreateController(provider);

      var items = RequestChain(controller, "$Mcp.package:");

      items.Select(i => i.Text).Should().Contain("send");
      provider.LastVerbQueryObjectId.Should().Be(new MooObjectId(456));
   }

   [Fact]
   public void Cached_two_step_chain_resolves_with_full_context_but_not_from_line_prefix_alone()
   {
      // Regression for udd-efk: the page's re-open guard must thread the SAME full context
      // (tree/tokens/caret) into GetMemberItems that the popup enumeration uses. The line-prefix-only
      // overload only resolves single-atom chains, so for a multi-step chain ($Mcp.package:) it
      // extracts nothing and reports Count == 0 even after the chain's verbs are fetched and cached —
      // which is why the popup never re-opened. This test documents that asymmetry.
      var provider = new FakeQueryProvider();
      provider.PropertyValues[(0, "Mcp")] = new MooPropertyValue(1, "#123");
      provider.PropertyValues[(123, "package")] = new MooPropertyValue(1, "#456");
      provider.Verbs.Add(new MooVerbSummary(new[] { "send" }, new MooObjectId(456)));
      using var controller = CreateController(provider);

      // Warm the cache exactly as RequestChain does: drive the chain via the FULL-context overload
      // and await the async resolve+fetch.
      const string buffer = "$Mcp.package:";
      var (tree, tokens) = ParseBuffer(buffer);
      var lines = buffer.Split('\n');
      var caretLine = lines.Length;
      var caretColumn = lines[^1].Length + 1;
      var linePrefix = lines[^1];
      RequestChain(controller, buffer); // resolves + caches the verb list for #456

      // Full context: the resolved+cached chain must yield items (Count > 0) — what the enumeration sees.
      controller.GetMemberItems(linePrefix, tree, tokens, caretLine, caretColumn).Count.Should().BeGreaterThan(0,
         "with the full parse context the cached multi-step chain resolves and returns its verbs");

      // Line-prefix-only: the SAME cached chain line prefix yields nothing, because that overload only
      // resolves single-atom chains. The page's re-open guard therefore must NOT use this overload.
      controller.GetMemberItems(linePrefix).Should().BeEmpty(
         "the line-prefix-only overload cannot resolve a multi-step chain, so the re-open guard must pass the full context");
   }

   [Fact]
   public void World_state_cache_avoids_a_second_property_value_query()
   {
      var provider = new FakeQueryProvider();
      provider.PropertyValues[(0, "Mcp")] = new MooPropertyValue(1, "#123");
      provider.Verbs.Add(new MooVerbSummary(new[] { "send" }, new MooObjectId(123)));
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(123)));
      using var controller = CreateController(provider);

      RequestChain(controller, "$Mcp:");         // resolves $Mcp -> #123 (one prop-value query)
      var before = provider.PropValueCalls;
      RequestChain(controller, "$Mcp.");          // property context on the same resolved object

      provider.PropValueCalls.Should().Be(before,
         "the cached (#0, Mcp) -> #123 resolution must not re-query the world");
   }

   [Fact]
   public void Variable_chain_is_re_derived_after_an_edit_changes_the_assignment()
   {
      var provider = new FakeQueryProvider();
      provider.PropertyValues[(0, "foo")] = new MooPropertyValue(1, "#10");
      provider.PropertyValues[(0, "bar")] = new MooPropertyValue(1, "#20");
      provider.Verbs.Add(new MooVerbSummary(new[] { "f" }, new MooObjectId(10)));
      provider.Verbs.Add(new MooVerbSummary(new[] { "f" }, new MooObjectId(20)));
      using var controller = CreateController(provider);

      // First edit: x = $foo; x:
      RequestChain(controller, "x = $foo;\nx:");
      provider.LastVerbQueryObjectId.Should().Be(new MooObjectId(10));

      // Second edit: x = $bar; x:  — variable value must be re-derived (never cached).
      RequestChain(controller, "x = $bar;\nx:");
      provider.LastVerbQueryObjectId.Should().Be(new MooObjectId(20),
         "local-variable values are re-walked from the live tree, never cached");
   }

   [Fact]
   public void Two_controllers_isolate_the_same_variable_name()
   {
      var provider = new FakeQueryProvider();
      provider.PropertyValues[(0, "foo")] = new MooPropertyValue(1, "#10");
      provider.PropertyValues[(0, "bar")] = new MooPropertyValue(1, "#20");
      provider.Verbs.Add(new MooVerbSummary(new[] { "f" }, new MooObjectId(10)));
      provider.Verbs.Add(new MooVerbSummary(new[] { "f" }, new MooObjectId(20)));
      using var controllerA = CreateController(provider);
      using var controllerB = CreateController(provider);

      RequestChain(controllerA, "x = $foo;\nx:");
      var aTarget = provider.LastVerbQueryObjectId;
      RequestChain(controllerB, "x = $bar;\nx:");
      var bTarget = provider.LastVerbQueryObjectId;

      aTarget.Should().Be(new MooObjectId(10));
      bTarget.Should().Be(new MooObjectId(20), "each page resolves the same var name from its own tree");
   }

   [Fact]
   public void Throwing_provider_during_a_chain_step_invokes_the_diagnostic()
   {
      var provider = new FakeQueryProvider();
      // $Mcp resolves, but the package step throws via a provider that faults on the second query.
      var faulting = new FaultingProvider();
      faulting.PropertyValues[(0, "Mcp")] = new MooPropertyValue(1, "#123");
      faulting.ThrowOnObject = 123;
      string? message = null;
      Exception? exception = null;
      using var controller = new MemberCompletionController(
         () => faulting, () => null, action => action(), () => { },
         diagnostic: (m, e) => { if (e != null) { message = m; exception = e; } });

      RequestChainExpectingEmpty(controller, "$Mcp.package:");

      exception.Should().BeOfType<InvalidOperationException>("a provider throw during a step must be logged");
      message.Should().NotBeNullOrEmpty();
   }

   [Fact]
   public void Expected_non_resolution_chain_does_not_invoke_warning_diagnostic()
   {
      var provider = new FakeQueryProvider();
      provider.PropertyValues[(0, "Mcp")] = new MooPropertyValue(1, "#123");
      // #123.package is absent -> expected non-resolution.
      var warned = false;
      using var controller = new MemberCompletionController(
         () => provider, () => null, action => action(), () => { },
         diagnostic: (_, ex) => { if (ex != null) warned = true; });

      RequestChainExpectingEmpty(controller, "$Mcp.package:");

      warned.Should().BeFalse("a chain that simply does not resolve must not be logged as a warning");
   }

   private static void RequestChainExpectingEmpty(MemberCompletionController controller, string buffer)
   {
      var (tree, tokens) = ParseBuffer(buffer);
      var lines = buffer.Split('\n');
      controller.GetMemberItems(lines[^1], tree, tokens, lines.Length, lines[^1].Length + 1);
      Thread.Sleep(200);
      controller.GetMemberItems(lines[^1], tree, tokens, lines.Length, lines[^1].Length + 1).Should().BeEmpty();
   }

   private sealed class FaultingProvider : IMooWorldQueryProvider
   {
      public Dictionary<(int Obj, string Prop), MooPropertyValue?> PropertyValues { get; } = new();
      public int ThrowOnObject = int.MinValue;

      public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
      {
         if (objectId.Number == ThrowOnObject)
            throw new InvalidOperationException("boom");
         PropertyValues.TryGetValue((objectId.Number, propName), out var value);
         return Task.FromResult(value);
      }

      public Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
         Task.FromResult<IReadOnlyList<MooVerbSummary>>(Array.Empty<MooVerbSummary>());
      public Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
         Task.FromResult<IReadOnlyList<MooPropertySummary>>(Array.Empty<MooPropertySummary>());
      public Task<MooObjectId?> GetCurrentPlayerAsync(CancellationToken cancellationToken) => Task.FromResult<MooObjectId?>(null);
      public Task<IReadOnlyList<MooObjectSummary>> GetCoreObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
   }
}
