#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="LambdaMooValidator.cs">
// Copyright (c)  2022
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

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;
using Org.Edgerunner.MooSharp.Language.Grammar;

namespace Org.Edgerunner.Moo.Editor.Language.Parsing;

public class LambdaMooValidator : MooParserBaseListener
{
   public List<ParseMessage> Errors { get; set; }

   public Document Document { get; set; }

   public LambdaMooValidator()
   {
      Errors = new List<ParseMessage>();
   }

   public override void EnterAssignmentExpression(MooParser.AssignmentExpressionContext context)
   {
      base.EnterAssignmentExpression(context);
   }

   public override void ExitAssignmentExpression(MooParser.AssignmentExpressionContext context)
   {
      bool ok = true;
      ParserRuleContext lhs = context.lhs;
      ISyntaxErrorGuide errGuide = null;

      if (lhs != null)
         do
         {
            if (lhs is not (MooParser.PropertyExpressionContext or
                MooParser.IdentifierExpressionContext or
                MooParser.CorePropertyExpressionContext or
                MooParser.IndexedExpressionContext or
                MooParser.RangeIndexedExpressionContext))
            {
               ok = false;
               var start = lhs.start;
               var stop = lhs.stop ?? start;
               if (start != null)
                  errGuide = new ErrorGuide(start.Line, start.Column, stop.Line, stop.Column + 1);
            }
         } while ((lhs = lhs.GetChild<ParserRuleContext>(0)) != null);

      if (!ok)
         Errors.Add(new ParseMessage(Document,
                                     context.lhs.Start.Line,
                                     context.lhs.Start.Column + 1,
                                     "Parser", "Invalid expression on left hand side of assignment operation",
                                     errGuide));
      base.ExitAssignmentExpression(context);
   }
}