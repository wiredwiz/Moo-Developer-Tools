using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.ANTLR4.Tools.Common.Syntax;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.MooSharp.Language.Grammar;

namespace Org.Edgerunner.Moo.Editor.SyntaxHighlighting;

public class MooSyntaxHighlightingGuide : ISyntaxHighlightingGuide
{
   public MooSyntaxHighlightingGuide()
   {
      AllGrammarNames = new List<string> { "Moo", "MooSharp", "ToastStuntMoo", "StuntMoo", "EdgeMoo" };
   }

   public Color GetErrorIndicatorColor()
   {
      return Settings.Instance.ErrorIndicatorColor;
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
            return Settings.Instance.KeywordColor;
         case "OBJECT":
            return Settings.Instance.ObjectColor;
         case "CORE_REFERENCE":
            return Settings.Instance.CoreReferenceColor;
         case "STRING":
            return Settings.Instance.StringColor;
         case "NUMBER":
         case "FLOAT":
         case "ERROR":
         case "BOOLEAN":
            return Settings.Instance.LiteralColor;
         case "IDENTIFIER":
            // Highlight verbs
            if (previousToken?.DisplayText == ":")
               return Settings.Instance.VerbColor;

            // Highlight functions
            if (nextToken?.DisplayText == "(")
               return Settings.Instance.BuiltinFunctionColor;

            // Highlight properties
            if (previousToken?.DisplayText == ".")
               return Settings.Instance.PropertyColor;

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
                  return Settings.Instance.BuiltinVariableColor;
               default:
                  return Settings.Instance.DefaultWordColor;
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
            return Settings.Instance.SymbolColor;
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
            return Settings.Instance.OperatorColor;
         case "'('":
         case "')'":
            return Settings.Instance.ParenthesisColor;
         case "'['":
         case "']'":
            return Settings.Instance.BracketColor;
         case "'{'":
         case "'}'":
            return Settings.Instance.CurlyBraceColor;
         case "SINGLE_LINE_COMMENT":
         case "DELIMITED_COMMENT":
            return Settings.Instance.CommentColor;
         default:
            return Settings.Instance.DefaultWordColor;
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
            return Settings.Instance.KeywordBackgroundColor;
         case "OBJECT":
            return Settings.Instance.ObjectBackgroundColor;
         case "CORE_REFERENCE":
            return Settings.Instance.CoreReferenceBackgroundColor;
         case "STRING":
            return Settings.Instance.StringBackgroundColor;
         case "NUMBER":
         case "FLOAT":
         case "ERROR":
         case "BOOLEAN":
            return Settings.Instance.LiteralBackgroundColor;
         case "IDENTIFIER":
            // Highlight verbs
            if (previousToken?.DisplayText == ":")
               return Settings.Instance.VerbBackgroundColor;

            // Highlight functions
            if (nextToken?.DisplayText == "(")
               return Settings.Instance.BuiltinFunctionBackgroundColor;

            // Highlight properties
            if (previousToken?.DisplayText == ".")
               return Settings.Instance.PropertyBackgroundColor;

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
                  return Settings.Instance.BuiltinVariableBackgroundColor;
               default:
                  return Settings.Instance.DefaultWordBackgroundColor;
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
            return Settings.Instance.SymbolBackgroundColor;
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
            return Settings.Instance.OperatorBackgroundColor;
         case "'('":
         case "')'":
            return Settings.Instance.ParenthesisBackgroundColor;
         case "'['":
         case "']'":
            return Settings.Instance.BracketBackgroundColor;
         case "'{'":
         case "'}'":
            return Settings.Instance.CurlyBraceBackgroundColor;
         case "SINGLE_LINE_COMMENT":
         case "DELIMITED_COMMENT":
            return Settings.Instance.CommentBackgroundColor;
         default:
            return Settings.Instance.DefaultWordBackgroundColor;
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
            return Settings.Instance.KeywordFontStyle;
         case "OBJECT":
            return Settings.Instance.ObjectFontStyle;
         case "CORE_REFERENCE":
            return Settings.Instance.CoreReferenceFontStyle;
         case "STRING":
            return Settings.Instance.StringFontStyle;
         case "NUMBER":
         case "FLOAT":
         case "ERROR":
         case "BOOLEAN":
            return Settings.Instance.LiteralFontStyle;
         case "IDENTIFIER":
            // Highlight verbs
            if (previousToken?.DisplayText == ":")
               return Settings.Instance.VerbFontStyle;

            // Highlight functions
            if (nextToken?.DisplayText == "(")
               return Settings.Instance.BuiltinFunctionFontStyle;

            // Highlight properties
            if (previousToken?.DisplayText == ".")
               return Settings.Instance.PropertyFontStyle;

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
                  return Settings.Instance.BuiltinVariableFontStyle;
               default:
                  return Settings.Instance.DefaultWordFontStyle;
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
            return Settings.Instance.SymbolFontStyle;
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
            return Settings.Instance.OperatorFontStyle;
         case "'('":
         case "')'":
            return Settings.Instance.ParenthesisFontStyle;
         case "'['":
         case "']'":
            return Settings.Instance.BracketFontStyle;
         case "'{'":
         case "'}'":
            return Settings.Instance.CurlyBraceFontStyle;
         case "SINGLE_LINE_COMMENT":
         case "DELIMITED_COMMENT":
            return Settings.Instance.CommentFontStyle;
         default:
            return Settings.Instance.DefaultWordFontStyle;
      }
   }

   public string GrammarName => "Moo";
   public IList<string> AllGrammarNames { get; }
}