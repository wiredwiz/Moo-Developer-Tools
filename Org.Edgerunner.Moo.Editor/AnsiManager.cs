#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="AnsiManager.cs">
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

using FastColoredTextBoxNS.Types;

namespace Org.Edgerunner.Moo.Editor;

public class AnsiManager
{
   public AnsiManager(Color defaultTextColor, Color defaultTextBackgroundColor)
   {
      DefaultTextColor = defaultTextColor;
      ForeColor = defaultTextColor;
      DefaultTextBackgroundColor = defaultTextBackgroundColor;
      BackgroundColor = defaultTextBackgroundColor;
      DefaultFontTextStyle = FontStyle.Regular;
      FontStyle = FontStyle.Regular;
      CurrentStyle = GetStyle(defaultTextColor, defaultTextBackgroundColor, FontStyle.Regular);
      IsReset = true;
      Blinking = false;
      ReverseVideo = false;
   }

   static AnsiManager()
   {
      StyleCache = new Dictionary<string, TextStyle>();
   }

   public Color DefaultTextColor { get; set; }
   public Color DefaultTextBackgroundColor { get; set; }
   public FontStyle DefaultFontTextStyle { get; set; }

   public bool Blinking { get; private set; }

   public TextStyle CurrentStyle { get; private set; }

   protected Color ForeColor { get; set; }

   protected Color BackgroundColor { get; set; }

   protected FontStyle FontStyle { get; set; }

   protected bool Bright { get; set; }

   protected bool ReverseVideo { get; set; }

   protected bool IsReset { get; set; }

   protected static Dictionary<string, TextStyle> StyleCache { get; set; }

   public TextStyle GetStyle(Color foregroundColor, Color backgroundColor, FontStyle fontStyle)
   {
      if (ReverseVideo)
         (backgroundColor, foregroundColor) = (foregroundColor, backgroundColor);
      string key = $"{foregroundColor}-{backgroundColor}-{fontStyle}";
      if (StyleCache.TryGetValue(key, out var style))
         return style;

      var foreBrush = new SolidBrush(foregroundColor);
      var backBrush = new SolidBrush(backgroundColor);
      return StyleCache[key] = new TextStyle(foreBrush, backBrush, fontStyle);
   }

   public TextStyle ProcessCodes(List<int> codes)
   {
      if (codes.Count == 1 && codes[0] is 2 or 4 or 5 or 7)
      {
         if (codes[0] == 2)
         {
            Bright = false;
            CurrentStyle = GetStyle(ForeColor, BackgroundColor, FontStyle);
         }
         else if (codes[0] == 4)
         {
            FontStyle |= FontStyle.Underline;
            CurrentStyle = GetStyle(ForeColor, BackgroundColor, FontStyle);
         }
         else if (codes[0] == 5)
            Blinking = true;
         else if (codes[0] == 7)
         {
            ReverseVideo = true;
            FontStyle |= FontStyle.Bold;
            CurrentStyle = GetStyle(ForeColor, BackgroundColor, FontStyle);
         }
      }
      else if (codes.Count > 2 && (codes[0] == 38 || codes[0] == 48))
      {
         if (codes[1] == 5)
            return ProcessXtermColors(codes);

         if (codes[1] == 2)
            return ProcessTrueColorColors(codes);
      }
      else if (codes.Count != 0)
         return ProcessBasicColors(codes);

      return CurrentStyle;
   }

   protected TextStyle ProcessBasicColors(List<int> codes)
   {
      if (codes.Contains(1))
         // preemptive bright check since the bright may come after
         // the color is defined and we need to know in advance
         Bright = true;

      if (codes.Contains(4))
         FontStyle = FontStyle.Underline;

      foreach (var code in codes)
      {
         if (code == 0)
         {
            DoAnsiReset();
         }
         else
         {
            switch (code)
            {
               case 30:
                  ForeColor = Bright ? Color.DarkGray : Color.Black;
                  break;
               case 31:
                  ForeColor = Bright ? Color.Red : Color.Maroon;
                  break;
               case 32:
                  ForeColor = Bright ? Color.LawnGreen : Color.Green;
                  break;
               case 33:
                  ForeColor = Bright ? Color.Yellow : Color.Gold;
                  break;
               case 34:
                  ForeColor = Bright ? Color.Blue : Color.DarkBlue;
                  break;
               case 35:
                  ForeColor = Bright ? Color.Magenta : Color.DarkMagenta;
                  break;
               case 36:
                  ForeColor = Bright ? Color.Cyan : Color.DarkCyan;
                  break;
               case 37:
                  ForeColor = Bright ? Color.White : Color.WhiteSmoke;
                  break;
               case 40:
                  BackgroundColor = Color.Black;
                  break;
               case 41:
                  BackgroundColor = Color.Maroon;
                  break;
               case 42:
                  BackgroundColor = Color.Green;
                  break;
               case 43:
                  BackgroundColor = Color.Gold;
                  break;
               case 44:
                  BackgroundColor = Color.DarkBlue;
                  break;
               case 45:
                  BackgroundColor = Color.DarkMagenta;
                  break;
               case 46:
                  BackgroundColor = Color.DarkCyan;
                  break;
               case 47:
                  BackgroundColor = Color.LightGray;
                  break;
            }

            IsReset = false;
         }
      }

      return CurrentStyle = GetStyle(ForeColor, BackgroundColor, FontStyle);
   }

   private void DoAnsiReset()
   {
      // Reset colors
      Bright = false;
      IsReset = true;
      Blinking = false;
      ReverseVideo = false;
      ForeColor = DefaultTextColor;
      BackgroundColor = DefaultTextBackgroundColor;
      FontStyle = DefaultFontTextStyle;
   }

   protected TextStyle ProcessXtermColors(List<int> codes)
   {
      Color color;
      int colorNum = codes[2];
      IsReset = false;
      if (colorNum == 0)
         color = Color.Black;
      else if (colorNum == 1)
         color = Color.Maroon;
      else if (colorNum == 2)
         color = Color.Green;
      else if (colorNum == 3)
         color = Color.Olive;
      else if (colorNum == 4)
         color = Color.FromArgb(0, 0, 128);
      else if (colorNum == 5)
         color = Color.Purple;
      else if (colorNum == 6)
         color = Color.Teal;
      else if (colorNum == 7)
         color = Color.Silver;
      else if (colorNum == 8)
         color = Color.Gray;
      else if (colorNum == 9)
         color = Color.Red;
      else if (colorNum == 10)
         color = Color.Lime;
      else if (colorNum == 11)
         color = Color.Yellow;
      else if (colorNum == 12)
         color = Color.Blue;
      else if (colorNum == 13)
         color = Color.Fuchsia;
      else if (colorNum == 14)
         color = Color.Aqua;
      else if (colorNum == 15)
         color = Color.White;
      else if (colorNum is >= 16 and <= 231)
      {
         int floorColorNum = colorNum - 16;
         var red = floorColorNum / 36 * 40;
         int green = (floorColorNum / 6) % 6 * 40;
         var blue = floorColorNum % 6 * 40;
         color = Color.FromArgb(
                                red == 0 ? 0 : 55 + red,
                                green == 0 ? 0 : 55 + green,
                                blue == 0 ? 0 : 55 + blue);
      }
      else if (colorNum is >= 232 and <= 255)
      {
         int rgb = (colorNum - 232) * 10 + 8;
         color = Color.FromArgb(rgb, rgb, rgb);
      }
      else
         return CurrentStyle = GetStyle(ForeColor, BackgroundColor, FontStyle);

      if (codes[0] == 38)
         ForeColor = color;
      else if (codes[0] == 48)
         BackgroundColor = color;

      return CurrentStyle = GetStyle(ForeColor, BackgroundColor, FontStyle);
   }

   protected TextStyle ProcessTrueColorColors(List<int> codes)
   {
      IsReset = false;
      if (codes[0] == 38)
         ForeColor = Color.FromArgb(codes[2], codes[3], codes[4]);
      else if (codes[0] == 48)
         BackgroundColor = Color.FromArgb(codes[2], codes[3], codes[4]);

      return CurrentStyle = GetStyle(ForeColor, BackgroundColor, FontStyle);
   }
}