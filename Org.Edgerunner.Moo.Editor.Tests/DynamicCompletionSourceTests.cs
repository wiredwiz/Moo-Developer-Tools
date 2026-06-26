using FastColoredTextBoxNS.Types;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class DynamicCompletionSourceTests
{
   private static MemberCompletionController CreateInertController()
   {
      // No provider: the controller always yields empty member lists.
      return new MemberCompletionController(() => null, () => null, action => action(), () => { });
   }

   [Fact]
   public void Enumeration_yields_static_items_when_no_member_context()
   {
      var statics = new List<AutocompleteItem> { new("if"), new("while") };
      using var controller = CreateInertController();
      var source = new DynamicCompletionSource(statics, controller, () => "x = 5");

      source.Should().Equal(statics);
   }

   [Fact]
   public void Enumeration_yields_member_items_before_static_items()
   {
      var statics = new List<AutocompleteItem> { new("if") };
      var fakeProvider = new ImmediatePropertyProvider("name");
      using var controller = new MemberCompletionController(
         () => fakeProvider, () => new MooObjectId(5), action => action(), () => { });
      var source = new DynamicCompletionSource(statics, controller, () => "this.");

      source.ToList();                                       // first pass starts the fetch
      SpinWait.SpinUntil(() => source.Count() == 2, TimeSpan.FromSeconds(5)).Should().BeTrue();

      var items = source.ToList();
      items[0].Should().BeOfType<MemberCompletionItem>();
      items[0].Text.Should().Be("name");
      items[1].Text.Should().Be("if");
   }

   [Fact]
   public void Line_prefix_is_evaluated_freshly_on_every_enumeration()
   {
      var prefixes = new Queue<string>(new[] { "x = 5", "x = 5" });
      using var controller = CreateInertController();
      var source = new DynamicCompletionSource(new List<AutocompleteItem>(), controller, prefixes.Dequeue);

      source.ToList();
      source.ToList();

      prefixes.Should().BeEmpty("each enumeration must request the current line prefix");
   }

   [Fact]
   public void Enumeration_resolves_a_chain_when_a_chain_context_is_supplied()
   {
      // udd-efk: when the source is given the parse tree + tokens + caret, a chained expression
      // ($Mcp.package:) resolves through to the target object's members.
      var statics = new List<AutocompleteItem> { new("if") };
      var provider = new ChainPropertyProvider();
      provider.PropertyValues[(0, "Mcp")] = new MooPropertyValue(1, "#123");
      provider.PropertyValues[(123, "package")] = new MooPropertyValue(1, "#456");
      provider.VerbsByObject[456] = new List<MooVerbSummary> { new(new[] { "send" }, new MooObjectId(456)) };
      using var controller = new MemberCompletionController(
         () => provider, () => null, action => action(), () => { });

      const string buffer = "$Mcp.package:";
      var (tree, tokens) = ChainParse(buffer);
      var source = new DynamicCompletionSource(
         statics, controller, () => buffer,
         () => new MemberCompletionContextSnapshot(tree, tokens, 1, buffer.Length + 1));

      source.ToList(); // start the resolve
      SpinWait.SpinUntil(() => source.Any(i => i.Text == "send"), TimeSpan.FromSeconds(5))
              .Should().BeTrue("the chain $Mcp.package: should resolve to #456 and offer its verbs");
   }

   private static (Antlr4.Runtime.ParserRuleContext Tree, IList<Org.Edgerunner.ANTLR4.Tools.Common.Grammar.DetailedToken> Tokens) ChainParse(string buffer)
   {
      var inputStream = new Antlr4.Runtime.AntlrInputStream(buffer);
      var lexer = Org.Edgerunner.Moo.Editor.Moo.GetLexer(GrammarDialect.Edgerunner, inputStream);
      lexer.TokenFactory = Org.Edgerunner.ANTLR4.Tools.Common.Grammar.DetailedTokenFactory.Instance;
      var stream = new Antlr4.Runtime.CommonTokenStream(lexer);
      stream.Fill();
      var tokens = stream.GetTokens().Cast<Org.Edgerunner.ANTLR4.Tools.Common.Grammar.DetailedToken>().ToList();
      var parser = Org.Edgerunner.Moo.Editor.Moo.GetParser(GrammarDialect.Edgerunner, stream);
      parser.RemoveErrorListeners();
      var tree = (Antlr4.Runtime.ParserRuleContext)parser.GetType().GetMethod("code")!.Invoke(parser, null)!;
      return (tree, tokens);
   }

   private sealed class ChainPropertyProvider : IMooWorldQueryProvider
   {
      public Dictionary<(int, string), MooPropertyValue?> PropertyValues { get; } = new();
      public Dictionary<int, List<MooVerbSummary>> VerbsByObject { get; } = new();

      public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
      {
         PropertyValues.TryGetValue((objectId.Number, propName), out var v);
         return Task.FromResult(v);
      }

      public Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken)
      {
         VerbsByObject.TryGetValue(objectId.Number, out var verbs);
         return Task.FromResult<IReadOnlyList<MooVerbSummary>>(verbs ?? new List<MooVerbSummary>());
      }

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
      public Task<string?> GetConstantValueAsync(string name, CancellationToken cancellationToken) => Task.FromResult<string?>(null);
      public Task<string?> GetConstantToStrAsync(string name, CancellationToken cancellationToken) => Task.FromResult<string?>(null);
   }

   /// <summary>A provider whose property query completes synchronously.</summary>
   private sealed class ImmediatePropertyProvider : IMooWorldQueryProvider
   {
      private readonly string _propertyName;

      public ImmediatePropertyProvider(string propertyName) => _propertyName = propertyName;

      public Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
         Task.FromResult<IReadOnlyList<MooPropertySummary>>(new[] { new MooPropertySummary(_propertyName, objectId) });

      public Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooObjectId?> GetCurrentPlayerAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetCoreObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<string?> GetConstantValueAsync(string name, CancellationToken cancellationToken) => Task.FromResult<string?>(null);
      public Task<string?> GetConstantToStrAsync(string name, CancellationToken cancellationToken) => Task.FromResult<string?>(null);
   }
}
