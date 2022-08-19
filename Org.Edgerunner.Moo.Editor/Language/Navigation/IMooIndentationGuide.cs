using Antlr4.Runtime.Tree;

namespace Org.Edgerunner.Moo.Editor.Language.Navigation;

public interface IMooIndentationGuide : IParseTreeListener
{
   public  int IndentSpacing { get; set; }

   public void AdjustIndent(int? line, int spaces);

   public int GetIndentShift(int line);
}