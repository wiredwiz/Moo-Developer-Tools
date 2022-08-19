#region BSD 3-Clause License
// <copyright file="LambdaMooIndentationGuide.cs" company="Edgerunner.org">
// Copyright 2020
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2022,
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.MooSharp.Language.Grammar;

namespace Org.Edgerunner.Moo.Editor.Language.Navigation;

public class LambdaMooIndentationGuide : MooParserBaseListener, IMooIndentationGuide
{
   public LambdaMooIndentationGuide(int indentSpacing)
   {
      if (indentSpacing < 1)
         throw new ArgumentException("IndentSpacing must be greater than 0");

      IndentSpacing = indentSpacing;
      IndentLevels = new Dictionary<int, int>(500);
      ColumnOverride = new Dictionary<int, int>(500);
   }

   public  int IndentSpacing { get; set; }

   public  Dictionary<int, int> IndentLevels { get; set; }

   private Dictionary<int, int> ColumnOverride { get; set; }

   private int MaxLineNo { get; set; }

   public void AdjustIndent(int? line, int spaces)
   {
      if (!line.HasValue)
         return;

      if (IndentLevels.ContainsKey(line.Value))
         IndentLevels[line.Value] += spaces;
      else
         IndentLevels.Add(line.Value, spaces);

      MaxLineNo = Math.Max(MaxLineNo, line.Value);
   }

   public int GetIndentShift(int line)
   {
      return IndentLevels.TryGetValue(line, out var shift) ? shift : 0;
   }

   public override void EnterCode(MooParser.CodeContext context)
   {
      MaxLineNo = 0;
      IndentLevels.Clear();
      ColumnOverride.Clear();
      base.EnterCode(context);
   }

   public override void ExitCode(MooParser.CodeContext context)
   {
      int level = 0;
      for (int i = 0; i < MaxLineNo + 3; i++)
      {
         var found = IndentLevels.TryGetValue(i, out var current);
         if (found && current != 0)
            level += current;
         if (ColumnOverride.TryGetValue(i, out var column) && level != column)
         {
            level = column - level;
            IndentLevels[i] = level;
         }
      }
      base.ExitCode(context);
   }

   public override void EnterIfStatement(MooParser.IfStatementContext context)
   {
      AdjustIndent(context.Start.Line + 1, IndentSpacing);
      base.EnterIfStatement(context);
   }

   public override void EnterElseifStatement(MooParser.ElseifStatementContext context)
   {
      AdjustIndent(context.start.Line, -IndentSpacing);
      AdjustIndent(context.start.Line + 1, IndentSpacing);
      base.EnterElseifStatement(context);
   }

   public override void EnterElseStatement(MooParser.ElseStatementContext context)
   {
      AdjustIndent(context.start.Line, -IndentSpacing);
      AdjustIndent(context.start.Line + 1, IndentSpacing);
      base.EnterElseStatement(context);
   }

   public override void EnterEndifStatement(MooParser.EndifStatementContext context)
   {
      if (context.start is DetailedToken { TypeNameUpperCase: "ENDIF" })
         AdjustIndent(context.Start.Line, -IndentSpacing);
      base.EnterEndifStatement(context);
   }

   public override void EnterForStatement(MooParser.ForStatementContext context)
   {
      AdjustIndent(context.Start.Line + 1, IndentSpacing);
      base.EnterForStatement(context);
   }

   public override void ExitEndforStatement(MooParser.EndforStatementContext context)
   {
      AdjustIndent(context.Start.Line, -IndentSpacing);
      base.ExitEndforStatement(context);
   }

   public override void EnterWhileStatement(MooParser.WhileStatementContext context)
   {
      AdjustIndent(context.Start.Line + 1, IndentSpacing);
      base.EnterWhileStatement(context);
   }

   public override void EnterEndwhileStatement(MooParser.EndwhileStatementContext context)
   {
      AdjustIndent(context.Start.Line, -IndentSpacing);
      base.EnterEndwhileStatement(context);
   }

   public override void EnterForkStatement(MooParser.ForkStatementContext context)
   {
      AdjustIndent(context.Start.Line + 1, IndentSpacing);
      base.EnterForkStatement(context);
   }

   public override void EnterEndforkStatement(MooParser.EndforkStatementContext context)
   {
      AdjustIndent(context.Start.Line, -IndentSpacing);
      base.EnterEndforkStatement(context);
   }

   public override void EnterTryExceptStatement(MooParser.TryExceptStatementContext context)
   {
      AdjustIndent(context.Start.Line + 1, IndentSpacing);
      base.EnterTryExceptStatement(context);
   }

   public override void EnterExceptStatement(MooParser.ExceptStatementContext context)
   {
      AdjustIndent(context.Start.Line, -IndentSpacing);
      AdjustIndent(context.Start.Line + 1, IndentSpacing);
      base.EnterExceptStatement(context);
   }

   public override void EnterTryFinallyStatement(MooParser.TryFinallyStatementContext context)
   {
      AdjustIndent(context.Start.Line + 1, IndentSpacing);
      AdjustIndent(context.Stop?.Line, -IndentSpacing);
      base.EnterTryFinallyStatement(context);
   }

   public override void EnterFinallyStatement(MooParser.FinallyStatementContext context)
   {
      AdjustIndent(context.Start.Line, -IndentSpacing);
      AdjustIndent(context.Start.Line + 1, IndentSpacing);
      base.EnterFinallyStatement(context);
   }

   public override void EnterEndtryStatement(MooParser.EndtryStatementContext context)
   {
      AdjustIndent(context.Start.Line, -IndentSpacing);
      base.EnterEndtryStatement(context);
   }

   public override void EnterParenthesisExpression(MooParser.ParenthesisExpressionContext context)
   {
      if (context.start.Line != context.stop.Line)
         if (context.stop != null)
            for (int i = context.start.Line + 1; i <= context.stop.Line; i++)
               IndentLevels[i] += 1;
      base.EnterParenthesisExpression(context);
   }
}