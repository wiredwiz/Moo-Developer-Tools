using Org.Edgerunner.Moo.Editor.Configuration;

namespace Org.Edgerunner.Moo.Udditor;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Settings.Instance.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Moo.Editor.config"));
        Application.Run(new Editor());
    }
}
