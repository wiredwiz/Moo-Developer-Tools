using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
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

      private const int WM_GETOBJECT = 0x003D;
      private const int OBJID_CLIENT = unchecked((int)0xFFFFFFFC);

      [DllImport("UIAutomationCore.dll", SetLastError = false)]
      private static extern IntPtr UiaReturnRawElementProvider(
         IntPtr hwnd, IntPtr wParam, IntPtr lParam, IRawElementProviderSimple el);

      // UIA event IDs for text notifications (from UIAutomationClient constants).
      // UIA_Text_TextChangedEventId          = 20014
      // UIA_Text_TextSelectionChangedEventId = 20018
      private static readonly MethodInfo _raiseAutomationEventMethod =
         typeof(AccessibleObject).GetMethod("RaiseAutomationEvent",
            BindingFlags.NonPublic | BindingFlags.Instance);

      private static void RaiseUiaEvent(AccessibleObject ao, int uiaEventId)
      {
         if (_raiseAutomationEventMethod == null || ao == null) return;
         try
         {
            var param = _raiseAutomationEventMethod.GetParameters()[0];
            var enumVal = Enum.ToObject(param.ParameterType, uiaEventId);
            _raiseAutomationEventMethod.Invoke(ao, new object[] { enumVal });
         }
         catch (Exception) { /* best effort */ }
      }

      // Called by CreateAccessibilityInstance overrides in this class and subclasses.
      // Ensures text/selection UIA events are hooked exactly once per control lifetime.
      protected void EnsureUiaEventHooks()
      {
         if (_accessibilityInitialized) return;
         TextChanged     += (_, _) => RaiseUiaEvent(AccessibilityObject, 20014);
         SelectionChanged += (_, _) => RaiseUiaEvent(AccessibilityObject, 20018);
         _accessibilityInitialized = true;
      }

      protected override AccessibleObject CreateAccessibilityInstance()
      {
         EnsureUiaEventHooks();
         return new FctbAccessibleObject(this);
      }

   }

   /// <summary>
   /// Accessibility object for FastColoredTextBox implementing the UIA Text Pattern.
   /// </summary>
   public class FctbAccessibleObject : Control.ControlAccessibleObject,
      ITextProvider,
      System.Windows.Forms.Automation.IAutomationLiveRegion,
      IRawElementProviderSimple
   {
      protected readonly FastColoredTextBox Tb;

      // UIA pattern/property IDs used in IRawElementProviderSimple implementation
      private const int UIA_TextPatternId                  = 10014;
      private const int UIA_TextPattern2Id                 = 10024;
      private const int UIA_ControlTypePropertyId          = 30003;
      private const int UIA_LiveSettingPropertyId          = 30135;
      private const int UIA_IsTextPatternAvailablePropertyId = 30119;
      private const int UIA_DocumentControlTypeId          = 50006;

      [DllImport("UIAutomationCore.dll", SetLastError = false)]
      private static extern int UiaHostProviderFromHwnd(IntPtr hwnd, out IRawElementProviderSimple provider);

      public FctbAccessibleObject(FastColoredTextBox tb) : base(tb)
      {
         Tb = tb;
      }

      // ── AccessibleObject overrides ────────────────────────────────────

      public override AccessibleRole Role => AccessibleRole.Document;

      // ── IRawElementProviderSimple — expose ITextProvider to UIA ──────
      // Re-implemented explicitly because ControlAccessibleObject.GetPatternProvider
      // (internal) does not discover ITextProvider in .NET 6 for custom UserControls.

      IRawElementProviderSimple IRawElementProviderSimple.HostRawElementProvider
      {
         get
         {
            if (!Tb.IsHandleCreated) return null;
            UiaHostProviderFromHwnd(Tb.Handle, out var host);
            return host;
         }
      }

      ProviderOptions IRawElementProviderSimple.ProviderOptions =>
         ProviderOptions.ServerSideProvider | ProviderOptions.UseComThreading;

      object IRawElementProviderSimple.GetPatternProvider(int patternId)
      {
         if (patternId == UIA_TextPatternId || patternId == UIA_TextPattern2Id)
            return this;
         return null;
      }

      object IRawElementProviderSimple.GetPropertyValue(int propertyId)
      {
         switch (propertyId)
         {
            case UIA_ControlTypePropertyId:             return UIA_DocumentControlTypeId;
            case UIA_LiveSettingPropertyId:             return (int)LiveSetting;
            case UIA_IsTextPatternAvailablePropertyId:  return true;
            default:                                    return null; // Host provider fills the rest
         }
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

      public virtual ITextRangeProvider[] GetSelection()
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

      private const int UIA_FontNameAttributeId        = 40005;
      private const int UIA_FontSizeAttributeId        = 40006;
      private const int UIA_ForegroundColorAttributeId = 40008;
      private const int UIA_BackgroundColorAttributeId = 40009;
      protected const int UIA_AnnotationTypesAttributeId         = 40031;
      protected const int UIA_FullDescriptionAttributeId         = 40035;
      protected const int AnnotationType_GrammarError            = 60002;
      protected const int AnnotationType_AdvancedProofingIssue   = 60017;

      /// <summary>Returns (normalizedStart, normalizedEnd) — start always before end.</summary>
      private (Place s, Place e) Normalized()
      {
         bool startFirst = _start.iLine < _end.iLine ||
                           (_start.iLine == _end.iLine && _start.iChar <= _end.iChar);
         return startFirst ? (_start, _end) : (_end, _start);
      }

      // ── Simple members ─────────────────────────────────────────────────

      public virtual ITextRangeProvider Clone() =>
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

      protected virtual void ExpandToEnclosingUnitImpl(TextUnit unit)
      {
         switch (unit)
         {
            case TextUnit.Character:
               _end = _start;
               if (_start.iChar < Tb.Lines[_start.iLine].Length)
                  _end = new Place(_start.iChar + 1, _start.iLine);
               break;

            case TextUnit.Word:
               var startRange = new TextSelectionRange(Tb, _start.iChar, _start.iLine, _start.iChar, _start.iLine);
               startRange.GoWordLeft(false);
               _start = startRange.Start;
               var endRange = new TextSelectionRange(Tb, _start.iChar, _start.iLine, _start.iChar, _start.iLine);
               endRange.GoWordRight(false);
               _end = endRange.End;
               break;

            case TextUnit.Line:
            case TextUnit.Paragraph:
               _start = new Place(0, _start.iLine);
               _end   = new Place(Tb.Lines[_start.iLine].Length, _start.iLine);
               break;

            case TextUnit.Page:
            case TextUnit.Document:
               _start = new Place(0, 0);
               int lastLine = Tb.LinesCount > 0 ? Tb.LinesCount - 1 : 0;
               _end   = new Place(Tb.LinesCount > 0 ? Tb.Lines[lastLine].Length : 0, lastLine);
               break;
         }
      }

      protected virtual int MoveImpl(TextUnit unit, int count)
      {
         int moved = MoveEndpointByUnitImpl(TextPatternRangeEndpoint.Start, unit, count);
         _end = _start;
         return moved;
      }

      protected virtual int MoveEndpointByUnitImpl(
         TextPatternRangeEndpoint endpoint, TextUnit unit, int count)
      {
         int moved = 0;
         int direction = count < 0 ? -1 : 1;
         int steps = Math.Abs(count);

         for (int i = 0; i < steps; i++)
         {
            bool advanced;
            if (endpoint == TextPatternRangeEndpoint.Start)
               advanced = AdvanceByUnit(ref _start, unit, direction);
            else
               advanced = AdvanceByUnit(ref _end, unit, direction);
            if (!advanced) break;
            moved += direction;
         }
         return moved;
      }

      private bool AdvanceByUnit(ref Place place, TextUnit unit, int direction)
      {
         switch (unit)
         {
            case TextUnit.Character:
               if (direction > 0)
               {
                  if (place.iChar < Tb.Lines[place.iLine].Length)
                     place = new Place(place.iChar + 1, place.iLine);
                  else if (place.iLine < Tb.LinesCount - 1)
                     place = new Place(0, place.iLine + 1);
                  else return false;
               }
               else
               {
                  if (place.iChar > 0)
                     place = new Place(place.iChar - 1, place.iLine);
                  else if (place.iLine > 0)
                     place = new Place(Tb.Lines[place.iLine - 1].Length, place.iLine - 1);
                  else return false;
               }
               return true;

            case TextUnit.Word:
               var r = new TextSelectionRange(Tb, place.iChar, place.iLine, place.iChar, place.iLine);
               if (direction > 0) r.GoWordRight(false);
               else r.GoWordLeft(false);
               var newPlace = direction > 0 ? r.End : r.Start;
               if (newPlace.iLine == place.iLine && newPlace.iChar == place.iChar) return false;
               place = newPlace;
               return true;

            case TextUnit.Line:
            case TextUnit.Paragraph:
               int newLine = place.iLine + direction;
               if (newLine < 0 || newLine >= Tb.LinesCount) return false;
               place = new Place(0, newLine);
               return true;

            case TextUnit.Page:
            case TextUnit.Document:
               if (direction > 0)
               {
                  int last = Tb.LinesCount > 0 ? Tb.LinesCount - 1 : 0;
                  place = new Place(Tb.LinesCount > 0 ? Tb.Lines[last].Length : 0, last);
               }
               else
                  place = new Place(0, 0);
               return true;

            default:
               return false;
         }
      }

      protected virtual void MoveEndpointByRangeImpl(
         TextPatternRangeEndpoint endpoint,
         ITextRangeProvider targetRange,
         TextPatternRangeEndpoint targetEndpoint)
      {
         if (targetRange is not FctbTextRangeProvider other) return;
         var source = targetEndpoint == TextPatternRangeEndpoint.Start ? other._start : other._end;
         if (endpoint == TextPatternRangeEndpoint.Start) _start = source;
         else _end = source;
      }

      protected virtual object GetAttributeValueImpl(int attribute)
      {
         switch (attribute)
         {
            case UIA_FontNameAttributeId:
               return Tb.Font.Name;

            case UIA_FontSizeAttributeId:
               return (double)Tb.Font.SizeInPoints;

            case UIA_ForegroundColorAttributeId:
               return (int)(((uint)Tb.ForeColor.R << 16) |
                            ((uint)Tb.ForeColor.G << 8)  |
                             (uint)Tb.ForeColor.B);

            case UIA_BackgroundColorAttributeId:
               return (int)(((uint)Tb.BackColor.R << 16) |
                            ((uint)Tb.BackColor.G << 8)  |
                             (uint)Tb.BackColor.B);

            case UIA_AnnotationTypesAttributeId:
               return AutomationElementIdentifiers.NotSupported;

            case UIA_FullDescriptionAttributeId:
               return AutomationElementIdentifiers.NotSupported;

            default:
               return AutomationElementIdentifiers.NotSupported;
         }
      }

      protected virtual ITextRangeProvider FindTextImpl(string text, bool backward, bool ignoreCase)
      {
         if (string.IsNullOrEmpty(text)) return null;
         var (s, e) = Normalized();
         var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

         if (!backward)
         {
            for (int line = s.iLine; line <= e.iLine && line < Tb.LinesCount; line++)
            {
               string lineText = Tb.Lines[line];
               int startChar = line == s.iLine ? s.iChar : 0;
               int searchLen = lineText.Length - startChar;
               if (searchLen <= 0) continue;
               int idx = lineText.IndexOf(text, startChar, searchLen, comparison);
               if (idx >= 0 && (line < e.iLine || idx + text.Length <= e.iChar))
                  return new FctbTextRangeProvider(Tb,
                     new Place(idx, line),
                     new Place(idx + text.Length, line));
            }
         }
         else
         {
            for (int line = e.iLine; line >= s.iLine; line--)
            {
               string lineText = Tb.Lines[line];
               int endChar = line == e.iLine ? Math.Min(e.iChar, lineText.Length) : lineText.Length;
               int startChar = line == s.iLine ? s.iChar : 0;
               if (endChar < text.Length) continue;
               int idx = lineText.LastIndexOf(text, endChar - 1, endChar - startChar, comparison);
               if (idx >= 0 && idx >= startChar)
                  return new FctbTextRangeProvider(Tb,
                     new Place(idx, line),
                     new Place(idx + text.Length, line));
            }
         }
         return null;
      }

      protected virtual ITextRangeProvider FindAttributeImpl(int attribute, object value, bool backward) => null;
   }
}
