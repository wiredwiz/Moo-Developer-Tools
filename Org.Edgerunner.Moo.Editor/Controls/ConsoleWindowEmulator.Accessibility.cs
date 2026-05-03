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

      protected override void OnHandleCreated(EventArgs e)
      {
         base.OnHandleCreated(e);
         TextChanged += (_, _) => FireLiveRegionChangedEvent();

         // Seed to current count so pre-existing buffer content is not re-announced.
         _prevLinesCount = LinesCount;

         // Announce every new non-empty complete line in arrival order.
         // NotificationProcessing.All (all:true) queues each line so Narrator reads
         // the full stream in sequence rather than only the last line.
         TextChanged += (_, _) =>
         {
            int count = LinesCount;
            if (count <= _prevLinesCount) { _prevLinesCount = count; return; }

            // New complete lines: from the old trailing-empty slot to the new trailing-empty.
            // Old trailing empty was at index _prevLinesCount-1; it now holds real text.
            // New trailing empty is at index count-1; we skip it.
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
