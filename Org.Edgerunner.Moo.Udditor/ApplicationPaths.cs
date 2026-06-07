namespace Org.Edgerunner.Moo.Udditor;

/// <summary>
/// Provides resolved file system paths for per-user application data files.
/// </summary>
internal static class ApplicationPaths
{
   /// <summary>
   /// Gets the per-user application data folder for Moo Udditor (%APPDATA%\Moo Udditor).
   /// </summary>
   public static string AppDataFolder =>
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Moo Udditor");

   /// <summary>
   /// Resolves a data file for reading: returns the copy in the application data folder if it
   /// exists, otherwise the copy in the application base directory (preserves older/portable installs).
   /// </summary>
   /// <param name="fileName">The bare file name, e.g. "Worlds.xml".</param>
   public static string ResolveDataFile(string fileName)
   {
      var appDataPath = Path.Combine(AppDataFolder, fileName);
      return File.Exists(appDataPath)
         ? appDataPath
         : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
   }

   /// <summary>
   /// Returns the application data folder path for a file to be written, creating the folder if needed.
   /// </summary>
   /// <param name="fileName">The bare file name, e.g. "Worlds.xml".</param>
   public static string GetWritableDataFile(string fileName)
   {
      Directory.CreateDirectory(AppDataFolder);
      return Path.Combine(AppDataFolder, fileName);
   }
}
