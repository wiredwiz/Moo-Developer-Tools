using System;
using System.Reflection;
using System.Windows.Automation;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class ConsoleWindowEmulator
   {
      // Reflection helper to call the internal RaiseAutomationEvent method on AccessibleObject.
      // UIA_LiveRegionChangedEventId = 19996
      private static readonly MethodInfo _raiseAutomationEventMethod =
         typeof(AccessibleObject).GetMethod("RaiseAutomationEvent",
            BindingFlags.NonPublic | BindingFlags.Instance);

      // Line count at the last TextChanged scan. FCTB always keeps a trailing empty line, so
      // new complete lines occupy indices [_prevLinesCount-1 .. LinesCount-2] after a change.
      private int _prevLinesCount;

      // True while a suppression window is open after text arrives. Using a timer rather than
      // a one-shot bool because FCTB may auto-scroll the view WITHOUT moving the cursor, so
      // SelectionChanged may never fire to clear a bool flag — leaving navigation blocked forever.
      private bool _suppressingNav;
      private System.Windows.Forms.Timer _suppressNavTimer;

      protected override void OnHandleCreated(EventArgs e)
      {
         base.OnHandleCreated(e);
         TextChanged += (_, _) => FireLiveRegionChangedEvent();

         _prevLinesCount = LinesCount;

         // Suppression timer: opened by TextChanged, auto-closes after 150ms.
         // When it closes, sync _lastNavLine so the first arrow key after auto-scroll
         // is correctly detected as a position change.
         _suppressNavTimer = new System.Windows.Forms.Timer { Interval = 150 };
         _suppressNavTimer.Tick += (_, _) =>
         {
            _suppressNavTimer.Stop();
            _suppressingNav = false;
            UpdateNavPosition(); // cursor may have scrolled; sync so next UP/DOWN is recognised
         };

         TextChanged += (_, _) =>
         {
            int count = LinesCount;
            if (count <= _prevLinesCount) { _prevLinesCount = count; return; }

            // Open/extend the suppression window so auto-scroll SelectionChangeds are ignored.
            _suppressingNav = true;
            _suppressNavTimer.Stop();
            _suppressNavTimer.Start();

            int startLine = Math.Max(0, _prevLinesCount - 1);
            int endLine   = count - 2;

            for (int i = startLine; i <= endLine; i++)
            {
               string line = Lines[i];
               if (!string.IsNullOrWhiteSpace(line))
                  FireAccessibilityNotification(line, all: true);
            }
            _prevLinesCount = count;
         };

         SelectionChanged += (_, _) =>
         {
            if (_suppressingNav) return;
            FireNavigationNotification();
         };
      }

      protected override AccessibleObject CreateAccessibilityInstance()
      {
         EnsureUiaEventHooks(); // text/selection UIA events from FastColoredTextBox base
         return new ConsoleAccessibleObject(this);
      }

      /// <summary>
      /// Fires the UIA LiveRegionChanged event to notify screen readers of new console output.
      /// </summary>
      internal void FireLiveRegionChangedEvent()
      {
         var ao = AccessibilityObject;
         if (_raiseAutomationEventMethod == null || ao == null) return;
         try
         {
            var param = _raiseAutomationEventMethod.GetParameters()[0];
            var enumVal = Enum.ToObject(param.ParameterType, 19996); // UIA_LiveRegionChangedEventId
            _raiseAutomationEventMethod.Invoke(ao, new object[] { enumVal });
         }
         catch (Exception) { /* best effort */ }
      }
   }

   /// <summary>
   /// Accessible object for ConsoleWindowEmulator — adds live-region support so screen
   /// readers automatically announce incoming MUD server text.
   /// </summary>
   public class ConsoleAccessibleObject : FctbAccessibleObject
   {
      public ConsoleAccessibleObject(ConsoleWindowEmulator console) : base(console) { }

      /// <summary>Polite live setting: screen reader reads new content during natural pauses.</summary>
      public override System.Windows.Forms.Automation.AutomationLiveSetting LiveSetting
      {
         get => System.Windows.Forms.Automation.AutomationLiveSetting.Polite;
         set { }
      }
   }
}
