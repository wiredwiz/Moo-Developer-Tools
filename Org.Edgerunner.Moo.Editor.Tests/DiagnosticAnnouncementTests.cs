using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Controls;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class DiagnosticAnnouncementTests
{
    [Fact]
    public void BuildAnnouncementString_NoErrorsNoWarnings_ReturnsNoErrors()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(0, 0)
            .Should().Be("No errors");
    }

    [Fact]
    public void BuildAnnouncementString_OneError_ReturnsSingular()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(1, 0)
            .Should().Be("1 syntax error");
    }

    [Fact]
    public void BuildAnnouncementString_MultipleErrors_ReturnsPlural()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(3, 0)
            .Should().Be("3 syntax errors");
    }

    [Fact]
    public void BuildAnnouncementString_OneWarning_ReturnsSingular()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(0, 1)
            .Should().Be("1 warning");
    }

    [Fact]
    public void BuildAnnouncementString_MultipleWarnings_ReturnsPlural()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(0, 2)
            .Should().Be("2 warnings");
    }

    [Fact]
    public void BuildAnnouncementString_ErrorsAndWarnings_ReturnsBoth()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(3, 2)
            .Should().Be("3 syntax errors and 2 warnings");
    }

    [Fact]
    public void BuildAnnouncementString_OneErrorOneWarning_ReturnsSingulars()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(1, 1)
            .Should().Be("1 syntax error and 1 warning");
    }
}
