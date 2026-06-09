using FastColoredTextBoxNS;
using FluentAssertions;
using Xunit;

namespace FastColoredTextBox.Tests;

public class AutocompletePopupMetricsTests {
	const float BaseFontPt = 9f;

	[Fact]
	public void Compute_At100Percent_ReturnsBaselineMetrics() {
		var metrics = AutocompletePopupMetrics.Compute(BaseFontPt, 100);

		metrics.FontSizeInPoints.Should().Be(9f);
		metrics.IconSize.Should().Be(16);
		metrics.Gutter.Should().Be(18);
		metrics.MaxHeight.Should().Be(180);
	}

	[Fact]
	public void Compute_At200Percent_DoublesEveryMetric() {
		var metrics = AutocompletePopupMetrics.Compute(BaseFontPt, 200);

		metrics.FontSizeInPoints.Should().Be(18f);
		metrics.IconSize.Should().Be(32);
		metrics.Gutter.Should().Be(34);
		metrics.MaxHeight.Should().Be(360);
	}

	[Fact]
	public void Compute_At50Percent_ClampsFontToFloor() {
		var metrics = AutocompletePopupMetrics.Compute(BaseFontPt, 50);

		// 9pt * 0.5 = 4.5pt, below the ~6pt floor.
		metrics.FontSizeInPoints.Should().Be(6f);
	}

	[Fact]
	public void Compute_At50Percent_ClampsIconToFloor() {
		var metrics = AutocompletePopupMetrics.Compute(BaseFontPt, 50);

		// 16px * 0.5 = 8px, exactly at the ~8px floor.
		metrics.IconSize.Should().BeGreaterThanOrEqualTo(8);
	}

	[Fact]
	public void Compute_GutterAlwaysTrailsIconByTwo() {
		var metrics = AutocompletePopupMetrics.Compute(BaseFontPt, 175);

		metrics.Gutter.Should().Be(metrics.IconSize + 2);
	}
}
