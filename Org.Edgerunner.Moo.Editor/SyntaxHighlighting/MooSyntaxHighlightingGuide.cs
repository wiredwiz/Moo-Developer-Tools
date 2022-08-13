using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.ANTLR4.Tools.Common.Syntax;

namespace Org.Edgerunner.Moo.Editor.SyntaxHighlighting;

public class MooSyntaxHighlightingGuide : ISyntaxHighlightingGuide
{
   public MooSyntaxHighlightingGuide()
      {
         AllGrammarNames = new List<string> {"Moo", "MooSharp", "ToastStuntMoo", "StuntMoo", "EdgeMoo"};
      }

      public Color GetErrorIndicatorColor()
      {
         return Color.Red;
      }

      public Color GetTokenForegroundColor(DetailedToken token)
      {
         var tokenTypeName = token.TypeNameUpperCase;
         switch (tokenTypeName.ToUpperInvariant())
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
               return Color.Blue;
            case "OBJECT":
            case "CORE_REFERENCE":
               return Color.DeepPink;
            case "STRING":
               return Color.Red;
            case "NUMBER":
            case "FLOAT":
            case "ERROR":
            case "BOOLEAN":
               return Color.BlueViolet;
            case "'$'":
            case "'='":
            case "'+'":
            case "'-'":
            case "'*'":
            case "'/'":
            case "'^'":
            case "'%'":
            case "'<'":
            case "'>'":
            case "'`'":
            case "'''":
            case "'=>'":
            case "'?'":
            case "'|'":
            case "'@'":
            case "';'":
            case "':'":
            case "'.'":
            case "','":
            case "'('":
            case "')'":
            case "'=='":
            case "'!='":
            case "'!'":
            case "'~'":
            case "'<='":
            case "'>='":
            case "'&.'":
            case "'|.'":
            case "'^.'":
            case "'->'":
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
               return  Color.DarkCyan;
            case "'['":
            case "']'":
            case "'{'":
            case "'}'":
               return Color.Tomato;
            case "SINGLE_LINE_COMMENT":
            case "DELIMITED_COMMENT":
               return Color.Green;
            default:
               return Color.Black;
         }
      }

      public Color GetTokenBackgroundColor(DetailedToken token)
      {
         return Color.Transparent;
      }

      public FontStyle GetTokenFontStyle(DetailedToken token)
      {
         return FontStyle.Regular;
      }

      public string GrammarName => "Moo";
      public IList<string> AllGrammarNames { get; }
}