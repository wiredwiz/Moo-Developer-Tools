using FluentAssertions;
using Org.Edgerunner.Mud.Communication.Sdwc;
using Xunit;

namespace Org.Edgerunner.Mud.Communication.Tests.Sdwc;

public class SdwcServerCapabilitiesTests
{
   [Fact]
   public void FullTokenList_SetsAllTypedAccessors()
   {
      var caps = new SdwcServerCapabilities(new[] { "verbs", "props", "VERB-OVERLAY", "PROP-OVERLAY", "SUPPORT" });

      caps.SupportsVerbs.Should().BeTrue();
      caps.SupportsProps.Should().BeTrue();
      caps.SupportsVerbOverlay.Should().BeTrue();
      caps.SupportsPropOverlay.Should().BeTrue();
      caps.HasAnyQueryableAbility.Should().BeTrue();
   }

   [Fact]
   public void Subset_SetsOnlyAdvertisedAccessors()
   {
      var caps = new SdwcServerCapabilities(new[] { "verbs", "SUPPORT" });

      caps.SupportsVerbs.Should().BeTrue();
      caps.SupportsProps.Should().BeFalse();
      caps.SupportsVerbOverlay.Should().BeFalse();
      caps.SupportsPropOverlay.Should().BeFalse();
      caps.HasAnyQueryableAbility.Should().BeTrue();
   }

   [Fact]
   public void EmptyTokenSet_HasNoQueryableAbility()
   {
      var caps = new SdwcServerCapabilities(System.Array.Empty<string>());

      caps.SupportsVerbs.Should().BeFalse();
      caps.SupportsProps.Should().BeFalse();
      caps.SupportsVerbOverlay.Should().BeFalse();
      caps.SupportsPropOverlay.Should().BeFalse();
      caps.HasAnyQueryableAbility.Should().BeFalse();
      caps.RawTokens.Should().BeEmpty();
   }

   [Fact]
   public void OnlySupportToken_HasNoQueryableAbility()
   {
      var caps = new SdwcServerCapabilities(new[] { "SUPPORT" });

      caps.HasAnyQueryableAbility.Should().BeFalse();
      caps.RawTokens.Should().Contain("SUPPORT");
   }

   [Fact]
   public void UnknownTokens_ArePreservedInRawTokens()
   {
      var caps = new SdwcServerCapabilities(new[] { "verbs", "FUTURE-THING", "another" });

      caps.RawTokens.Should().Contain("FUTURE-THING");
      caps.RawTokens.Should().Contain("another");
      caps.SupportsVerbs.Should().BeTrue();
   }

   [Fact]
   public void TypedAccessors_AreCaseInsensitive()
   {
      var caps = new SdwcServerCapabilities(new[] { "VERBS", "Props", "verb-overlay", "prop-OVERLAY" });

      caps.SupportsVerbs.Should().BeTrue();
      caps.SupportsProps.Should().BeTrue();
      caps.SupportsVerbOverlay.Should().BeTrue();
      caps.SupportsPropOverlay.Should().BeTrue();
   }

   [Fact]
   public void RawTokens_UseCaseInsensitiveComparer()
   {
      var caps = new SdwcServerCapabilities(new[] { "Verbs" });

      caps.RawTokens.Contains("verbs").Should().BeTrue();
      caps.RawTokens.Contains("VERBS").Should().BeTrue();
   }
}
