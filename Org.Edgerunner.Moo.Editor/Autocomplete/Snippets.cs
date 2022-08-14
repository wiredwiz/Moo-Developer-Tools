using System.Collections;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

public static class Snippets
{
   public static IList<string> LoadSnippets(string filePath)
   {
      if (!File.Exists(filePath))
         return Array.Empty<string>();

      var lines = File.ReadLines(filePath);
      var snippets = new List<string>();
      foreach (var snippet in lines)
      {
         snippets.Add(snippet
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t"));
      }

      return snippets;
   }
}