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
   }
}
