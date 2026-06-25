using FastColoredTextBoxNS;
using FluentAssertions;
using Xunit;

namespace FastColoredTextBox.Tests;

public class AutocompleteMatchRankerTests {
	[Fact]
	public void SelectBestMatchIndex_PrefersAlphabeticallyFirstOverShortest() {
		var index = AutocompleteMatchRanker.SelectBestMatchIndex(new[] { "name", "nz" }, "n");

		index.Should().Be(0);
	}

	[Fact]
	public void SelectBestMatchIndex_IsAlphabeticalNotSourceOrder() {
		var index = AutocompleteMatchRanker.SelectBestMatchIndex(new[] { "nz", "name" }, "n");

		index.Should().Be(1);
	}

	[Fact]
	public void SelectBestMatchIndex_ExactMatchWinsOverPrefix_For() {
		var index = AutocompleteMatchRanker.SelectBestMatchIndex(new[] { "for", "fork" }, "for");

		index.Should().Be(0);
	}

	[Fact]
	public void SelectBestMatchIndex_ExactMatchWinsOverPrefix_Else() {
		var index = AutocompleteMatchRanker.SelectBestMatchIndex(new[] { "elseif", "else" }, "else");

		index.Should().Be(1);
	}

	[Fact]
	public void SelectBestMatchIndex_ExactMatchWinsEvenWhenAlphabeticallyLast() {
		var index = AutocompleteMatchRanker.SelectBestMatchIndex(new[] { "za", "z" }, "z");

		index.Should().Be(1);
	}

	[Fact]
	public void SelectBestMatchIndex_EmptyList_ReturnsMinusOne() {
		var index = AutocompleteMatchRanker.SelectBestMatchIndex(new string[0], "n");

		index.Should().Be(-1);
	}

	[Fact]
	public void SelectBestMatchIndex_TieOnEqualText_KeepsEarlierIndex() {
		var index = AutocompleteMatchRanker.SelectBestMatchIndex(new[] { "dup", "dup" }, "d");

		index.Should().Be(0);
	}

	[Fact]
	public void SelectBestMatchIndex_CaseInsensitiveExactMatchWins() {
		var index = AutocompleteMatchRanker.SelectBestMatchIndex(new[] { "Foreach", "FOR" }, "for");

		index.Should().Be(1);
	}
}
