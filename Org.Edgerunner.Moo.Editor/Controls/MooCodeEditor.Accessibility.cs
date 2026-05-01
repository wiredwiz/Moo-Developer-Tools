using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;

namespace Org.Edgerunner.Moo.Editor.Controls;

public partial class MooCodeEditor
{
   private bool _editorAccessibilityInitialized;

   protected override AccessibleObject CreateAccessibilityInstance()
   {
      if (!_editorAccessibilityInitialized)
         _editorAccessibilityInitialized = true;
      return new MooCodeEditorAccessibleObject(this);
   }

   internal void UpdateDiagnostics(List<ParseMessage> messages)
   {
      if (AccessibilityObject is MooCodeEditorAccessibleObject acc)
         acc.UpdateDiagnostics(messages);
   }
}

/// <summary>
/// Accessible object for MooCodeEditor — adds parser error/warning annotations and
/// a debounced diagnostic count announcement.
/// </summary>
public class MooCodeEditorAccessibleObject : FctbAccessibleObject
{
   private readonly MooCodeEditor _editor;
   internal IReadOnlyList<ParseMessage> _errors   = Array.Empty<ParseMessage>();
   internal IReadOnlyList<ParseMessage> _warnings = Array.Empty<ParseMessage>();

   public MooCodeEditorAccessibleObject(MooCodeEditor editor) : base(editor)
   {
      _editor = editor;
   }

   public override System.Windows.Forms.Automation.AutomationLiveSetting LiveSetting
   {
      get => System.Windows.Forms.Automation.AutomationLiveSetting.Polite;
      set { }
   }

   internal void UpdateDiagnostics(List<ParseMessage> messages)
   {
      _errors   = messages.Where(m => m.Severity == ParseMessageSeverity.Error).ToList();
      _warnings = messages.Where(m => m.Severity == ParseMessageSeverity.Warning).ToList();
   }

   public override ITextRangeProvider[] GetSelection()
   {
      var sel = Tb.Selection;
      return new ITextRangeProvider[]
      {
         new MooCodeEditorRangeProvider(Tb, sel.Start, sel.End, _errors, _warnings)
      };
   }

   public override ITextRangeProvider DocumentRange
   {
      get
      {
         int lastLine = Tb.LinesCount > 0 ? Tb.LinesCount - 1 : 0;
         int lastChar = Tb.LinesCount > 0 ? Tb.Lines[lastLine].Length : 0;
         return new MooCodeEditorRangeProvider(Tb,
            new FastColoredTextBoxNS.Types.Place(0, 0),
            new FastColoredTextBoxNS.Types.Place(lastChar, lastLine),
            _errors, _warnings);
      }
   }

   public static string BuildAnnouncementString(int errorCount, int warningCount)
   {
      if (errorCount == 0 && warningCount == 0)
         return "No errors";
      if (errorCount > 0 && warningCount == 0)
         return $"{errorCount} syntax {(errorCount == 1 ? "error" : "errors")}";
      if (errorCount == 0 && warningCount > 0)
         return $"{warningCount} {(warningCount == 1 ? "warning" : "warnings")}";
      return $"{errorCount} syntax {(errorCount == 1 ? "error" : "errors")} " +
             $"and {warningCount} {(warningCount == 1 ? "warning" : "warnings")}";
   }
}

/// <summary>
/// Range provider that overlays error/warning annotation attributes from the
/// MooCodeEditor diagnostic list.
/// </summary>
internal class MooCodeEditorRangeProvider : FctbTextRangeProvider
{
   private readonly IReadOnlyList<ParseMessage> _errors;
   private readonly IReadOnlyList<ParseMessage> _warnings;

   public MooCodeEditorRangeProvider(
      FastColoredTextBox tb, FastColoredTextBoxNS.Types.Place start, FastColoredTextBoxNS.Types.Place end,
      IReadOnlyList<ParseMessage> errors,
      IReadOnlyList<ParseMessage> warnings)
      : base(tb, start, end)
   {
      _errors   = errors;
      _warnings = warnings;
   }

   protected override object GetAttributeValueImpl(int attribute)
   {
      if (attribute == UIA_AnnotationTypesAttributeId)
      {
         foreach (var err in _errors)
            if (OverlapsRange(err)) return AnnotationType_GrammarError;
         foreach (var warn in _warnings)
            if (OverlapsRange(warn)) return AnnotationType_AdvancedProofingIssue;
      }

      if (attribute == UIA_FullDescriptionAttributeId)
      {
         foreach (var err in _errors)
            if (OverlapsRange(err)) return err.Message;
         foreach (var warn in _warnings)
            if (OverlapsRange(warn)) return warn.Message;
      }

      return base.GetAttributeValueImpl(attribute);
   }

   public override ITextRangeProvider Clone() =>
      new MooCodeEditorRangeProvider(Tb, _start, _end, _errors, _warnings);

   private bool OverlapsRange(ParseMessage msg)
   {
      var (rs, re) = Normalized_();
      if (msg.Guide == null)
      {
         int msgLine = msg.LineNumber - 1;
         return msgLine >= rs.iLine && msgLine <= re.iLine;
      }
      // ISyntaxErrorGuide uses Line (start, 1-indexed) and EndingLine (end, 1-indexed)
      int msgStartLine = msg.Guide.Line - 1;
      int msgEndLine   = msg.Guide.EndingLine - 1;
      return msgStartLine <= re.iLine && msgEndLine >= rs.iLine;
   }

   private (FastColoredTextBoxNS.Types.Place s, FastColoredTextBoxNS.Types.Place e) Normalized_()
   {
      bool startFirst = _start.iLine < _end.iLine ||
                        (_start.iLine == _end.iLine && _start.iChar <= _end.iChar);
      return startFirst ? (_start, _end) : (_end, _start);
   }
}
