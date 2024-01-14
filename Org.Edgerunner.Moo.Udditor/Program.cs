using System.Configuration;
using NLog;
using Org.Edgerunner.Moo.Editor.Configuration;

namespace Org.Edgerunner.Moo.Udditor;

internal static class Program
{
   // ReSharper disable once InconsistentNaming
   private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

   /// <summary>
   ///  The main entry point for the application.
   /// </summary>
   [STAThread]
   static void Main()
   {
      // To customize application configuration such as set high DPI settings or default font,
      // see https://aka.ms/applicationconfiguration.

      try
      {
         ApplicationConfiguration.Initialize();
         Settings.Instance.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Moo.Editor.config"));
      }
      catch (ConfigurationErrorsException ex)
      {
         Logger.Error(ex, "Application configuration file failed to load");
         MessageBox.Show(ex.Message, "Configuration Load Error");
      }
      try
      {
         Application.Run(new Main.Editor());
      }
      // ReSharper disable once CatchAllClause
      catch (Exception ex)
      {
         Logger.Fatal(ex, "Application crashed with fatal error");
      }
   }
}
