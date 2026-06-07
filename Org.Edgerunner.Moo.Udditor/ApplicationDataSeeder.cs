using NLog;

namespace Org.Edgerunner.Moo.Udditor;

/// <summary>
/// Seeds the per-user application data folder with default data files extracted from embedded
/// resources on first run. Existing files are never overwritten.
/// </summary>
internal static class ApplicationDataSeeder
{
   // ReSharper disable once InconsistentNaming
   private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

   /// <summary>
   /// The set of default files to seed. Each name equals the embedded resource's LogicalName
   /// and the target file name in the application data folder.
   /// </summary>
   private static readonly string[] Manifest =
   {
      "Moo.Editor.config",
      "Moo.Editor.Darkmode.Example.config",
      "Snippets.txt"
   };

   /// <summary>
   /// Ensures the per-user application data folder exists and is seeded with the default data files.
   /// Best-effort: any failure is logged and is non-fatal.
   /// </summary>
   public static void EnsureSeeded()
   {
      try
      {
         Directory.CreateDirectory(ApplicationPaths.AppDataFolder);
      }
      catch (Exception ex)
      {
         Logger.Warn(ex, "Unable to create application data folder '{0}'; skipping default file seeding", ApplicationPaths.AppDataFolder);
         return;
      }

      foreach (var fileName in Manifest)
      {
         try
         {
            SeedFile(fileName);
         }
         catch (Exception ex)
         {
            Logger.Warn(ex, "Failed to seed default file '{0}'", fileName);
         }
      }
   }

   /// <summary>
   /// Writes the embedded default for <paramref name="fileName"/> into the application data folder
   /// if it does not already exist.
   /// </summary>
   /// <param name="fileName">The bare file name, which also serves as the embedded resource LogicalName.</param>
   private static void SeedFile(string fileName)
   {
      var target = Path.Combine(ApplicationPaths.AppDataFolder, fileName);
      if (File.Exists(target))
         return;

      using var stream = typeof(ApplicationDataSeeder).Assembly.GetManifestResourceStream(fileName);
      if (stream is null)
      {
         Logger.Warn("Embedded default '{0}' not found; cannot seed", fileName);
         return;
      }

      var tempPath = target + ".tmp";
      try
      {
         using (var file = File.Create(tempPath))
         {
            stream.CopyTo(file);
         }

         File.Move(tempPath, target);
      }
      catch
      {
         try
         {
            File.Delete(tempPath);
         }
         catch
         {
            // best effort cleanup; ignore
         }

         throw;
      }

      Logger.Info("Seeded default file '{0}' to '{1}'", fileName, target);
   }
}
