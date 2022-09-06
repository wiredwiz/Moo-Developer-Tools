#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="PlainTextToHtmlConverter.cs">
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

using System.Text;

namespace Org.Edgerunner.Moo.MooText;

public static class PlainTextToHtmlConverter
{
   public static string ToHtml(string text)
   {
      return ToHtml(text.ToCharArray());
   }

   public static string ToHtml(char[] text)
   {
      var length = text.Length;
      var sb = new StringBuilder(length);
      sb.Append(text);
      sb.Replace("\r\n", "\n");
      sb.Replace("&", "&amp;");
      sb.Replace("<", "&lt;");
      sb.Replace(">", "&gt;");
      sb.Replace("\"", "&quot;");
      sb.Replace("'", "&#39;");
      sb.Replace(" ", "&nbsp;");
      length = sb.Length;
      text = new char[sb.Length];
      sb.CopyTo(0, text, 0, length);
      sb.Clear();
      sb.Append("<p>");
      var openParagraph = true;
      for (int i = 0; i < length; i++)
      {
         if (text[i] == '\r')
         {
            // ignore
         }
         else if (text[i] == '\n')
         {
            if (i + 1 < length && text[i + 1] == '\n')
            {
               if (openParagraph)
               {
                  sb.Append("</p>");
                  openParagraph = false;
               }
               else
               {
                  sb.Append("<br/>");
               }
               i++;
            }
            else
               sb.Append("<br/>");
         }
         else
         {
            if (!openParagraph)
            {
               openParagraph = true;
               sb.Append("<p>");
            }
            sb.Append(text[i]);
         }
      }

      if (openParagraph)
         sb.Append("</p>");

      return sb.ToString();
   }
}