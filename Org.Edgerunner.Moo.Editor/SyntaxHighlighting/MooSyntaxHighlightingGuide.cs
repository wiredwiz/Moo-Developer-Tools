using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.ANTLR4.Tools.Common.Syntax;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.MooSharp.Language.Grammar;

namespace Org.Edgerunner.Moo.Editor.SyntaxHighlighting;

public class MooSyntaxHighlightingGuide : ISyntaxHighlightingGuide
{
   private readonly Settings _settings;

   /// <summary>
   /// Initializes a new instance of the <see cref="MooSyntaxHighlightingGuide"/> class.
   /// </summary>
   /// <param name="source">
   /// The settings source to read colors and styles from. When <see langword="null"/>, the
   /// singleton <see cref="Settings.Instance"/> is used (default behavior).
   /// </param>
   public MooSyntaxHighlightingGuide(Settings source = null)
   {
      _settings = source ?? Settings.Instance;
      AllGrammarNames = new List<string> { "Moo", "MooSharp", "ToastStuntMoo", "StuntMoo", "EdgeMoo" };
   }

   public Color GetErrorIndicatorColor()
   {
      return _settings.ErrorIndicatorColor;
   }

   public Color GetTokenForegroundColor(DetailedToken token, DetailedToken previousToken, DetailedToken nextToken)
   {
      var tokenTypeName = token.TypeNameUpperCase;
      switch (tokenTypeName)
      {
         case "WHILE":
         case "ENDWHILE":
         case "FOR":
         case "ENDFOR":
         case "BREAK":
         case "CONTINUE":
         case "RETURN":
         case "FORK":
         case "ENDFORK":
         case "TRY":
         case "EXCEPT":
         case "FINALLY":
         case "ENDTRY":
         case "IF":
         case "ELSE":
         case "ELSEIF":
         case "ENDIF":
         case "ANY":
            return _settings.KeywordColor;
         case "OBJECT":
            return _settings.ObjectColor;
         case "CORE_REFERENCE":
            return _settings.CoreReferenceColor;
         case "STRING":
            return _settings.StringColor;
         case "NUMBER":
         case "FLOAT":
         case "ERROR":
         case "BOOLEAN":
            return _settings.LiteralColor;
         case "IDENTIFIER":
            // Highlight verbs
            if (previousToken?.DisplayText == ":")
               return _settings.VerbColor;

            // Highlight functions
            if (nextToken?.DisplayText == "(")
               return _settings.BuiltinFunctionColor;

            // Highlight properties
            if (previousToken?.DisplayText == ".")
               return _settings.PropertyColor;

            // highlight builtin variables
            switch (token.DisplayText)
            {
               case "player":
               case "caller":
               case "this":
               case "verb":
               case "args":
               case "argstr":
               case "dobj":
               case "dobjstr":
               case "prepstr":
               case "iobj":
               case "iobjstr":
               case "ERR":
               case "STR":
               case "BOOL":
               case "INT":
               case "NUM":
               case "FLOAT":
               case "LIST":
               case "OBJ":
               case "MAP":
               case "WAIF":
               case "ANON":
                  return _settings.BuiltinVariableColor;
               default:
                  return _settings.DefaultWordColor;
            }
         case "'->'":
         case "'$'":
         case "'`'":
         case "'''":
         case "'=>'":
         case "'?'":
         case "'|'":
         case "'@'":
         case "';'":
         case "':'":
         case "'.'":
         case "'..'":
         case "','":
            return _settings.SymbolColor;
         case "'='":
         case "'+'":
         case "'-'":
         case "'*'":
         case "'/'":
         case "'^'":
         case "'%'":
         case "'<'":
         case "'>'":
         case "'=='":
         case "'!='":
         case "'!'":
         case "'~'":
         case "'<='":
         case "'>='":
         case "'&.'":
         case "'|.'":
         case "'^.'":
         case "'>>'":
         case "'<<'":
         case "'+='":
         case "'-='":
         case "'*='":
         case "'/='":
         case "'&='":
         case "'|='":
         case "'%='":
         case "'^='":
         case "'++'":
         case "'--'":
         case "'||'":
         case "'&&'":
         case "IN":
            return _settings.OperatorColor;
         case "'('":
         case "')'":
            return _settings.ParenthesisColor;
         case "'['":
         case "']'":
            return _settings.BracketColor;
         case "'{'":
         case "'}'":
            return _settings.CurlyBraceColor;
         case "SINGLE_LINE_COMMENT":
         case "DELIMITED_COMMENT":
            return _settings.CommentColor;
         default:
            return _settings.DefaultWordColor;
      }
   }

   public Color GetTokenBackgroundColor(DetailedToken token, DetailedToken previousToken, DetailedToken nextToken)
   {
      var tokenTypeName = token.TypeNameUpperCase;
      switch (tokenTypeName)
      {
         case "WHILE":
         case "ENDWHILE":
         case "FOR":
         case "ENDFOR":
         case "BREAK":
         case "CONTINUE":
         case "RETURN":
         case "FORK":
         case "ENDFORK":
         case "TRY":
         case "EXCEPT":
         case "FINALLY":
         case "ENDTRY":
         case "IF":
         case "ELSE":
         case "ELSEIF":
         case "ENDIF":
         case "ANY":
            return _settings.KeywordBackgroundColor;
         case "OBJECT":
            return _settings.ObjectBackgroundColor;
         case "CORE_REFERENCE":
            return _settings.CoreReferenceBackgroundColor;
         case "STRING":
            return _settings.StringBackgroundColor;
         case "NUMBER":
         case "FLOAT":
         case "ERROR":
         case "BOOLEAN":
            return _settings.LiteralBackgroundColor;
         case "IDENTIFIER":
            // Highlight verbs
            if (previousToken?.DisplayText == ":")
               return _settings.VerbBackgroundColor;

            // Highlight functions
            if (nextToken?.DisplayText == "(")
               return _settings.BuiltinFunctionBackgroundColor;

            // Highlight properties
            if (previousToken?.DisplayText == ".")
               return _settings.PropertyBackgroundColor;

            // highlight builtin variables
            switch (token.DisplayText)
            {
               case "player":
               case "caller":
               case "this":
               case "verb":
               case "args":
               case "argstr":
               case "dobj":
               case "dobjstr":
               case "prepstr":
               case "iobj":
               case "iobjstr":
               case "ERR":
               case "STR":
               case "BOOL":
               case "INT":
               case "NUM":
               case "FLOAT":
               case "LIST":
               case "OBJ":
               case "MAP":
               case "WAIF":
               case "ANON":
                  return _settings.BuiltinVariableBackgroundColor;
               default:
                  return _settings.DefaultWordBackgroundColor;
            }
         case "'->'":
         case "'$'":
         case "'`'":
         case "'''":
         case "'=>'":
         case "'?'":
         case "'|'":
         case "'@'":
         case "';'":
         case "':'":
         case "'.'":
         case "'..'":
         case "','":
            return _settings.SymbolBackgroundColor;
         case "'='":
         case "'+'":
         case "'-'":
         case "'*'":
         case "'/'":
         case "'^'":
         case "'%'":
         case "'<'":
         case "'>'":
         case "'=='":
         case "'!='":
         case "'!'":
         case "'~'":
         case "'<='":
         case "'>='":
         case "'&.'":
         case "'|.'":
         case "'^.'":
         case "'>>'":
         case "'<<'":
         case "'+='":
         case "'-='":
         case "'*='":
         case "'/='":
         case "'&='":
         case "'|='":
         case "'%='":
         case "'^='":
         case "'++'":
         case "'--'":
         case "'||'":
         case "'&&'":
         case "IN":
            return _settings.OperatorBackgroundColor;
         case "'('":
         case "')'":
            return _settings.ParenthesisBackgroundColor;
         case "'['":
         case "']'":
            return _settings.BracketBackgroundColor;
         case "'{'":
         case "'}'":
            return _settings.CurlyBraceBackgroundColor;
         case "SINGLE_LINE_COMMENT":
         case "DELIMITED_COMMENT":
            return _settings.CommentBackgroundColor;
         default:
            return _settings.DefaultWordBackgroundColor;
      }
   }

   public FontStyle GetTokenFontStyle(DetailedToken token, DetailedToken previousToken, DetailedToken nextToken)
   {
      var tokenTypeName = token.TypeNameUpperCase;
      switch (tokenTypeName)
      {
         case "WHILE":
         case "ENDWHILE":
         case "FOR":
         case "ENDFOR":
         case "BREAK":
         case "CONTINUE":
         case "RETURN":
         case "FORK":
         case "ENDFORK":
         case "TRY":
         case "EXCEPT":
         case "FINALLY":
         case "ENDTRY":
         case "IF":
         case "ELSE":
         case "ELSEIF":
         case "ENDIF":
         case "ANY":
            return _settings.KeywordFontStyle;
         case "OBJECT":
            return _settings.ObjectFontStyle;
         case "CORE_REFERENCE":
            return _settings.CoreReferenceFontStyle;
         case "STRING":
            return _settings.StringFontStyle;
         case "NUMBER":
         case "FLOAT":
         case "ERROR":
         case "BOOLEAN":
            return _settings.LiteralFontStyle;
         case "IDENTIFIER":
            // Highlight verbs
            if (previousToken?.DisplayText == ":")
               return _settings.VerbFontStyle;

            // Highlight functions
            if (nextToken?.DisplayText == "(")
               return _settings.BuiltinFunctionFontStyle;

            // Highlight properties
            if (previousToken?.DisplayText == ".")
               return _settings.PropertyFontStyle;

            // highlight builtin variables
            switch (token.DisplayText)
            {
               case "player":
               case "caller":
               case "this":
               case "verb":
               case "args":
               case "argstr":
               case "dobj":
               case "dobjstr":
               case "prepstr":
               case "iobj":
               case "iobjstr":
               case "ERR":
               case "STR":
               case "BOOL":
               case "INT":
               case "NUM":
               case "FLOAT":
               case "LIST":
               case "OBJ":
               case "MAP":
               case "WAIF":
               case "ANON":
                  return _settings.BuiltinVariableFontStyle;
               default:
                  return _settings.DefaultWordFontStyle;
            }
         case "'->'":
         case "'$'":
         case "'`'":
         case "'''":
         case "'=>'":
         case "'?'":
         case "'|'":
         case "'@'":
         case "';'":
         case "':'":
         case "'.'":
         case "'..'":
         case "','":
            return _settings.SymbolFontStyle;
         case "'='":
         case "'+'":
         case "'-'":
         case "'*'":
         case "'/'":
         case "'^'":
         case "'%'":
         case "'<'":
         case "'>'":
         case "'=='":
         case "'!='":
         case "'!'":
         case "'~'":
         case "'<='":
         case "'>='":
         case "'&.'":
         case "'|.'":
         case "'^.'":
         case "'>>'":
         case "'<<'":
         case "'+='":
         case "'-='":
         case "'*='":
         case "'/='":
         case "'&='":
         case "'|='":
         case "'%='":
         case "'^='":
         case "'++'":
         case "'--'":
         case "'||'":
         case "'&&'":
         case "IN":
            return _settings.OperatorFontStyle;
         case "'('":
         case "')'":
            return _settings.ParenthesisFontStyle;
         case "'['":
         case "']'":
            return _settings.BracketFontStyle;
         case "'{'":
         case "'}'":
            return _settings.CurlyBraceFontStyle;
         case "SINGLE_LINE_COMMENT":
         case "DELIMITED_COMMENT":
            return _settings.CommentFontStyle;
         default:
            return _settings.DefaultWordFontStyle;
      }
   }

   public string GrammarName => "Moo";
   public IList<string> AllGrammarNames { get; }
}