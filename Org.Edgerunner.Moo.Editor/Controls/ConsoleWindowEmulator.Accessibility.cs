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

      // Index of the last line we have already announced. Initialized in OnHandleCreated
      // to the current bottom so pre-existing content is not announced on connect.
      private int _lastAnnouncedLine = -1;

      protected override void OnHandleCreated(EventArgs e)
      {
         base.OnHandleCreated(e);
         TextChanged += (_, _) => FireLiveRegionChangedEvent();

         // Seed the announced position at the current bottom so we only announce NEW text.
         _lastAnnouncedLine = LinesCount - 1;

         // Announce every new non-empty line in arrival order using NotificationProcessing.All
         // so Narrator reads the full stream, not just the last line.
         TextChanged += (_, _) =>
         {
            int count = LinesCount;
            for (int i = _lastAnnouncedLine + 1; i < count; i++)
            {
               string line = i < count ? Lines[i] : string.Empty;
               if (!string.IsNullOrWhiteSpace(line))
                  FireAccessibilityNotification(line, all: true);
            }
            _lastAnnouncedLine = count - 1;
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
