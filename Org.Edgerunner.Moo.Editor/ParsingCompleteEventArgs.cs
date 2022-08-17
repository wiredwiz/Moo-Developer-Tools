using Antlr4.Runtime;
using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;

namespace Org.Edgerunner.Moo.Editor;

public class ParsingCompleteEventArgs : EventArgs
{
   /// <summary>
   /// Initializes a new instance of the <see cref="ParsingCompleteEventArgs" /> class.
   /// </summary>
   /// <param name="document">The related document.</param>
   /// <param name="errorMessages">The error messages.</param>
   /// <param name="tokens">The tokens.</param>
   /// <param name="result">The parser rule context result.</param>
   public ParsingCompleteEventArgs(Document document, List<ParseMessage> errorMessages, List<DetailedToken> tokens, ParserRuleContext result)
   {
      Document = document;
      ErrorMessages = errorMessages;
      Tokens = tokens;
      Result = result;
   }

   /// <summary>
   /// Gets the related document.
   /// </summary>
   /// <value>
   /// The document.
   /// </value>
   public Document Document { get; set; }

   /// <summary>
   /// Gets or sets the error messages.
   /// </summary>
   /// <value>
   /// The error messages.
   /// </value>
   public List<ParseMessage> ErrorMessages { get; set; }

   /// <summary>
   /// Gets or sets the lexer tokens.
   /// </summary>
   /// <value>
   /// The lexer tokens.
   /// </value>
   public List<DetailedToken> Tokens { get; set; }

   /// <summary>
   /// Gets or sets the parser result.
   /// </summary>
   /// <value>
   /// The parser result.
   /// </value>
   public ParserRuleContext Result { get; set; }
}