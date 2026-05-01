using System;
using System.Reflection;
using System.Windows.Automation;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class ConsoleWindowEmulator
   {
      private bool _consoleAccessibilityInitialized;

      // Reflection helper to call the internal RaiseAutomationEvent method on AccessibleObject.
      // UIA_LiveRegionChangedEventId = 19996
      private static readonly MethodInfo _raiseAutomationEventMethod =
         typeof(AccessibleObject).GetMethod("RaiseAutomationEvent",
            BindingFlags.NonPublic | BindingFlags.Instance);

      protected override AccessibleObject CreateAccessibilityInstance()
      {
         EnsureUiaEventHooks(); // text/selection UIA events from FastColoredTextBox base
         if (!_consoleAccessibilityInitialized)
         {
            TextChanged += (_, _) => FireLiveRegionChangedEvent();
            _consoleAccessibilityInitialized = true;
         }
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
