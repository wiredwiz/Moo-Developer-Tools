using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS.Types;

namespace FastColoredTextBoxNS
{
   public partial class FastColoredTextBox
   {
      private bool _accessibilityInitialized;

      // UIA event IDs for text notifications (from UIAutomationClient constants).
      // UIA_Text_TextChangedEventId          = 20014
      // UIA_Text_TextSelectionChangedEventId = 20018
      private static readonly MethodInfo _raiseAutomationEventMethod =
         typeof(AccessibleObject).GetMethod("RaiseAutomationEvent",
            BindingFlags.NonPublic | BindingFlags.Instance);

      private static void RaiseUiaEvent(AccessibleObject ao, int uiaEventId)
      {
         // Convert the integer event ID to the internal UIA enum value via reflection.
         // The internal method signature is: RaiseAutomationEvent(Interop.UiaCore.UIA)
         if (_raiseAutomationEventMethod == null || ao == null) return;
         try
         {
            var param = _raiseAutomationEventMethod.GetParameters()[0];
            var enumVal = Enum.ToObject(param.ParameterType, uiaEventId);
            _raiseAutomationEventMethod.Invoke(ao, new object[] { enumVal });
         }
         catch (Exception) { /* best effort */ }
      }

      protected override AccessibleObject CreateAccessibilityInstance()
      {
         if (!_accessibilityInitialized)
         {
            TextChanged += (_, _) => RaiseUiaEvent(AccessibilityObject, 20014);
            SelectionChanged += (_, _) => RaiseUiaEvent(AccessibilityObject, 20018);
            _accessibilityInitialized = true;
         }
         return new FctbAccessibleObject(this);
      }
   }

   /// <summary>
   /// Accessibility object for FastColoredTextBox implementing the UIA Text Pattern.
   /// </summary>
   public class FctbAccessibleObject : Control.ControlAccessibleObject, ITextProvider, System.Windows.Forms.Automation.IAutomationLiveRegion
   {
      protected readonly FastColoredTextBox Tb;

      public FctbAccessibleObject(FastColoredTextBox tb) : base(tb)
      {
         Tb = tb;
      }

      // ── ITextProvider ──────────────────────────────────────────────────

      public virtual ITextRangeProvider DocumentRange
      {
         get
         {
            int lastLine = Tb.LinesCount > 0 ? Tb.LinesCount - 1 : 0;
            int lastChar = Tb.LinesCount > 0 ? Tb.Lines[lastLine].Length : 0;
            return new FctbTextRangeProvider(Tb, new Place(0, 0), new Place(lastChar, lastLine));
         }
      }

      public SupportedTextSelection SupportedTextSelection => SupportedTextSelection.Single;

      public ITextRangeProvider[] GetSelection()
      {
         var sel = Tb.Selection;
         return new ITextRangeProvider[]
         {
            new FctbTextRangeProvider(Tb, sel.Start, sel.End)
         };
      }

      public ITextRangeProvider[] GetVisibleRanges()
      {
         var vis = Tb.VisibleRange;
         return new ITextRangeProvider[]
         {
            new FctbTextRangeProvider(Tb, vis.Start, vis.End)
         };
      }

      public ITextRangeProvider RangeFromChild(IRawElementProviderSimple childElement) => null;

      public ITextRangeProvider RangeFromPoint(System.Windows.Point screenLocation)
      {
         var clientPt = Tb.PointToClient(
            new System.Drawing.Point((int)screenLocation.X, (int)screenLocation.Y));
         var place = Tb.PointToPlace(clientPt);
         return new FctbTextRangeProvider(Tb, place, place);
      }

      /// <summary>Gets or sets the UIA live region setting. Virtual so subclasses can override.</summary>
      public virtual System.Windows.Forms.Automation.AutomationLiveSetting LiveSetting
      {
         get => System.Windows.Forms.Automation.AutomationLiveSetting.Off;
         set { /* read-only by design; setter required by IAutomationLiveRegion */ }
      }
   }

   /// <summary>
   /// Implements ITextRangeProvider for a span of text within FastColoredTextBox.
   /// </summary>
   public class FctbTextRangeProvider : ITextRangeProvider
   {
      protected readonly FastColoredTextBox Tb;
      protected Place _start;
      protected Place _end;

      public FctbTextRangeProvider(FastColoredTextBox tb, Place start, Place end)
      {
         Tb = tb;
         _start = start;
         _end = end;
      }

      /// <summary>Returns (normalizedStart, normalizedEnd) — start always before end.</summary>
      private (Place s, Place e) Normalized()
      {
         bool startFirst = _start.iLine < _end.iLine ||
                           (_start.iLine == _end.iLine && _start.iChar <= _end.iChar);
         return startFirst ? (_start, _end) : (_end, _start);
      }

      // ── Simple members ─────────────────────────────────────────────────

      public ITextRangeProvider Clone() =>
         new FctbTextRangeProvider(Tb, _start, _end);

      public bool Compare(ITextRangeProvider range)
      {
         if (range is not FctbTextRangeProvider other) return false;
         return _start.iLine == other._start.iLine &&
                _start.iChar == other._start.iChar &&
                _end.iLine == other._end.iLine &&
                _end.iChar == other._end.iChar;
      }

      public int CompareEndpoints(
         TextPatternRangeEndpoint endpoint,
         ITextRangeProvider targetRange,
         TextPatternRangeEndpoint targetEndpoint)
      {
         var mine = endpoint == TextPatternRangeEndpoint.Start ? _start : _end;
         var other = targetRange is FctbTextRangeProvider r
            ? (targetEndpoint == TextPatternRangeEndpoint.Start ? r._start : r._end)
            : new Place(0, 0);

         if (mine.iLine != other.iLine) return mine.iLine.CompareTo(other.iLine);
         return mine.iChar.CompareTo(other.iChar);
      }

      public IRawElementProviderSimple GetEnclosingElement() =>
         Tb.AccessibilityObject as IRawElementProviderSimple;

      public IRawElementProviderSimple[] GetChildren() =>
         Array.Empty<IRawElementProviderSimple>();

      public string GetText(int maxLength)
      {
         var (s, e) = Normalized();
         var sb = new System.Text.StringBuilder();
         for (int line = s.iLine; line <= e.iLine; line++)
         {
            if (maxLength >= 0 && sb.Length >= maxLength) break;
            int fromChar = line == s.iLine ? s.iChar : 0;
            int toChar   = line == e.iLine ? e.iChar  : Tb.Lines[line].Length;
            toChar = Math.Min(toChar, Tb.Lines[line].Length);
            if (fromChar >= toChar && line < e.iLine) { sb.Append('\n'); continue; }
            if (fromChar >= toChar) break;
            if (line > s.iLine) sb.Append('\n');
            if (fromChar < toChar)
               sb.Append(Tb.Lines[line].Substring(fromChar, toChar - fromChar));
         }
         var result = sb.ToString();
         if (maxLength >= 0 && result.Length > maxLength)
            result = result[..maxLength];
         return result;
      }

      public double[] GetBoundingRectangles()
      {
         if (Tb.IsDisposed || !Tb.IsHandleCreated) return Array.Empty<double>();
         var (s, e) = Normalized();
         var rects = new List<double>();
         for (int line = s.iLine; line <= e.iLine && line < Tb.LinesCount; line++)
         {
            int fromChar = line == s.iLine ? s.iChar : 0;
            int toChar   = line == e.iLine ? e.iChar  : Tb.Lines[line].Length;
            toChar = Math.Min(toChar, Tb.Lines[line].Length);
            if (fromChar >= toChar) continue;
            var p1 = Tb.PointToScreen(Tb.PlaceToPoint(new Place(fromChar, line)));
            var p2 = Tb.PointToScreen(Tb.PlaceToPoint(new Place(toChar, line)));
            rects.Add(p1.X);
            rects.Add(p1.Y);
            rects.Add(Math.Abs(p2.X - p1.X));
            rects.Add(Tb.CharHeight);
         }
         return rects.ToArray();
      }

      public void AddToSelection() =>
         throw new InvalidOperationException("Single selection only.");

      public void RemoveFromSelection() =>
         throw new InvalidOperationException("Single selection only.");

      public void Select()
      {
         if (Tb.InvokeRequired)
            Tb.Invoke(() => Select());
         else
         {
            Tb.Selection.Start = _start;
            Tb.Selection.End = _end;
            Tb.Invalidate();
         }
      }

      public void ScrollIntoView(bool alignToTop)
      {
         var range = Tb.GetRange(_start, _end);
         if (Tb.InvokeRequired) Tb.Invoke(() => Tb.DoRangeVisible(range, alignToTop));
         else Tb.DoRangeVisible(range, alignToTop);
      }

      // ── Navigation, attributes, search — stubs replaced in Tasks 5 and 6 ──

      public void ExpandToEnclosingUnit(TextUnit unit) => ExpandToEnclosingUnitImpl(unit);
      public int Move(TextUnit unit, int count) => MoveImpl(unit, count);
      public int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count)
         => MoveEndpointByUnitImpl(endpoint, unit, count);
      public void MoveEndpointByRange(TextPatternRangeEndpoint endpoint,
         ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
         => MoveEndpointByRangeImpl(endpoint, targetRange, targetEndpoint);
      public object GetAttributeValue(int attribute) => GetAttributeValueImpl(attribute);
      public ITextRangeProvider FindText(string text, bool backward, bool ignoreCase)
         => FindTextImpl(text, backward, ignoreCase);
      public ITextRangeProvider FindAttribute(int attribute, object value, bool backward)
         => FindAttributeImpl(attribute, value, backward);

      // Stubs — replaced in Tasks 5 and 6
      protected virtual void ExpandToEnclosingUnitImpl(TextUnit unit) { }
      protected virtual int MoveImpl(TextUnit unit, int count) => 0;
      protected virtual int MoveEndpointByUnitImpl(TextPatternRangeEndpoint endpoint, TextUnit unit, int count) => 0;
      protected virtual void MoveEndpointByRangeImpl(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint) { }
      protected virtual object GetAttributeValueImpl(int attribute) => AutomationElementIdentifiers.NotSupported;
      protected virtual ITextRangeProvider FindTextImpl(string text, bool backward, bool ignoreCase) => null;
      protected virtual ITextRangeProvider FindAttributeImpl(int attribute, object value, bool backward) => null;
   }
}
