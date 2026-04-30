namespace Org.Edgerunner.Mud.MCP;

public enum McpParseState { InProgress, Complete, Error }

public class McpMessageParser
{
   private enum InternalState { Normal, InMultiline }

   private InternalState _state = InternalState.Normal;
   private string _name = string.Empty;
   private string _key = string.Empty;
   private Dictionary<string, string> _simpleFields = new();
   private Dictionary<string, string> _dataTagToKeyword = new();
   private Dictionary<string, List<string>> _multilineBuffers = new();

   public Message? Result { get; private set; }

   public McpParseState FeedLine(string line)
   {
      Result = null;
      return _state == InternalState.Normal
         ? ProcessNormalLine(line)
         : ProcessMultilineLine(line);
   }

   private McpParseState ProcessNormalLine(string line)
   {
      if (string.IsNullOrWhiteSpace(line))
         return McpParseState.Error;

      if (line.StartsWith("* ") || line == "*" || line.StartsWith(": ") || line == ":")
         return McpParseState.Error;

      try
      {
         var words = McpUtils.SplitMessageIntoWords(line);
         if (words.Count == 0)
            return McpParseState.Error;

         _name = words[0];
         words.RemoveAt(0);

         _key = string.Empty;
         if (words.Count > 0 && !words[0].EndsWith(':'))
         {
            _key = words[0];
            words.RemoveAt(0);
         }

         _simpleFields = new Dictionary<string, string>();
         _dataTagToKeyword = new Dictionary<string, string>();
         _multilineBuffers = new Dictionary<string, List<string>>();

         bool hasMultiline = false;
         string currentKey = string.Empty;

         foreach (var word in words)
         {
            if (word.EndsWith(':'))
            {
               currentKey = word;
            }
            else if (!string.IsNullOrEmpty(currentKey))
            {
               if (currentKey.EndsWith("*:"))
               {
                  var fieldName = currentKey[..^2];
                  _dataTagToKeyword[word] = fieldName;
                  _multilineBuffers[word] = new List<string>();
                  hasMultiline = true;
               }
               else
               {
                  _simpleFields[currentKey.ToLowerInvariant()] = word;
               }
               currentKey = string.Empty;
            }
         }

         if (hasMultiline)
         {
            _state = InternalState.InMultiline;
            return McpParseState.InProgress;
         }

         Result = new Message(_name, _key, _simpleFields);
         return McpParseState.Complete;
      }
      catch
      {
         return McpParseState.Error;
      }
   }

   private McpParseState ProcessMultilineLine(string line)
   {
      // Placeholder — multiline support added in a later task
      return McpParseState.Error;
   }

   public void Reset()
   {
      _state = InternalState.Normal;
      _name = string.Empty;
      _key = string.Empty;
      _simpleFields = new Dictionary<string, string>();
      _dataTagToKeyword = new Dictionary<string, string>();
      _multilineBuffers = new Dictionary<string, List<string>>();
      Result = null;
   }
}
