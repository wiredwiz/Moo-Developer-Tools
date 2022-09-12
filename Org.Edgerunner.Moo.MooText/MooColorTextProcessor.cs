#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MooColorTextProcessor.cs">
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

using System.Drawing;
using System.Text;
using System.Xml.Linq;

namespace Org.Edgerunner.Moo.MooText;

/// <summary>
/// Class representing a Moo text color code processor.
/// </summary>
/// <seealso cref="Org.Edgerunner.Moo.MooText.MooTextPipeline" />
public class MooColorTextProcessor : MooTextPipeline
{
   /// <summary>
   /// Initializes a new instance of the <see cref="MooColorTextProcessor" /> class.
   /// </summary>
   public MooColorTextProcessor()
   {
   }

   /// <summary>
   /// Gets or sets the number of open tags.
   /// </summary>
   /// <value>
   /// The number of open tags.
   /// </value>
   protected virtual int OpenTags { get; set; }

   /// <summary>
   /// Gets or sets a value indicating whether the processor is in [bright mode].
   /// </summary>
   /// <value>
   ///   <c>true</c> if in [bright mode]; otherwise, <c>false</c>.
   /// </value>
   protected virtual bool BrightMode { get; set; }

   /// <summary>
   /// Gets or sets the current foreground color.
   /// </summary>
   /// <value>
   /// The current foreground color.
   /// </value>
   protected virtual Color? CurrentFgColor { get; set; }

   /// <summary>
   /// Gets or sets the current background color.
   /// </summary>
   /// <value>
   /// The current background color.
   /// </value>
   protected virtual Color? CurrentBgColor { get; set; }

   /// <summary>
   /// Processes the specified text.
   /// </summary>
   /// <param name="text">The text.</param>
   /// <param name="position">The current position.</param>
   /// <param name="output">The output <see cref="StringBuilder" />.</param>
   /// <returns>
   ///   <c>true</c> if this pipeline instance or another pipe farther down processed the current position; <c>false</c> otherwise.
   /// </returns>
   internal override bool Process(ref char[] text, ref int position, ref StringBuilder output)
   {
      if (text[position] == '[')
         if (AttemptProcessingOfColorName(ref text, ref position, ref output))
            return true;

      return base.Process(ref text, ref position, ref output);
   }

   protected virtual bool AttemptProcessingOfColorName(ref char[] text, ref int position, ref StringBuilder output)
   {
      var current = position++;
      var valid = true;
      var colorName = new StringBuilder(15);
      var finished = false;
      while (position < text.Length)
      {
         char c = text[position++];

         if ((c > 47 && c < 59) || (c > 64 && c < 91) || c == 93 || (c > 96 && c < 123))
         {
            if (c == ']')
            {
               if (text[position] != '[')
                  finished = true;

               break;
            }

            colorName.Append(c);
         }
         else
         {
            valid = false;
            break;
         }
      }

      // Attempt to translate the color name
      if (valid)
      {
         try
         {
            var tag = colorName.ToString().ToLowerInvariant();
            if (tag == "normal")
            {
               BrightMode = false;
               for (int i = OpenTags; i > 0; i--)
                  output.Append("</font>");
               OpenTags = 0;
            }
            else if (tag is "bright" or "bold")
            {
               BrightMode = true;
            }
            else
            {
               var color = Mud.Common.ColorConverter.ConvertStandardAnsiMooColorName(tag, BrightMode);
               if (tag.StartsWith("b:") || tag.StartsWith("bg"))
                  CurrentBgColor = color;
               else
                  CurrentFgColor = color;

               if (finished)
               {
                  output.Append("<font");

                  // populate foreground
                  if (CurrentFgColor.HasValue)
                  {
                     output.Append(" color=\"");
                     output.Append(ColorTranslator.ToHtml(CurrentFgColor!.Value));
                     output.Append('"');
                  }

                  // populate background
                  if (CurrentBgColor.HasValue)
                  {
                     output.Append(" bgcolor=\"");
                     output.Append(ColorTranslator.ToHtml(CurrentBgColor!.Value));
                     output.Append('"');
                  }

                  output.Append('>');
                  OpenTags++;

                  // Reset our color buffers since we have now written them out
                  CurrentBgColor = null;
                  CurrentFgColor = null;
               }
               valid = true;
            }
         }
         catch (ArgumentOutOfRangeException)
         {
            valid = false;
         }
      }

      if (!valid)
      {
         position = current;
         return false;
      }

      return true;
   }

   /// <inheritdoc />
   internal override void PostProcessing(ref StringBuilder output)
   {
      for (int i = OpenTags; i > 0; i--)
         output.Append("</font>");

      base.PostProcessing(ref output);
   }

   /// <inheritdoc />
   internal override void Reset()
   {
      BrightMode = false;
      OpenTags = 0;
      CurrentBgColor = null;
      CurrentFgColor = null;
      base.Reset();
   }
}