#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="ColorConverter.cs">
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

namespace Org.Edgerunner.Mud.Common;

/// <summary>
/// Class responsible for converting various color codes into system colors.
/// </summary>
/// <seealso cref="Color"/>
public static class ColorConverter
{
   /// <summary>
   /// Converts the ANSI color code into a drawing color.
   /// </summary>
   /// <param name="code">The ansi color code.</param>
   /// <param name="bright">if set to <c>true</c> returns the bright version of the color code.</param>
   /// <returns>A <see cref="Color"/> represented by the code.</returns>
   /// <exception cref="ArgumentOutOfRangeException">Color code is unrecognized as a basic ansi color code.</exception>
   public static Color ConvertBasicAnsiColorCode(int code, bool bright)
   {
      Color color;
      switch (code)
      {
         case 30:
            color = bright ? Color.DarkGray : Color.Black;
            break;
         case 31:
            color = bright ? Color.Red : Color.Maroon;
            break;
         case 32:
            color = bright ? Color.LawnGreen : Color.Green;
            break;
         case 33:
            color = bright ? Color.Yellow : Color.Gold;
            break;
         case 34:
            color = bright ? Color.Blue : Color.DarkBlue;
            break;
         case 35:
            color = bright ? Color.Magenta : Color.DarkMagenta;
            break;
         case 36:
            color = bright ? Color.Cyan : Color.DarkCyan;
            break;
         case 37:
            color = bright ? Color.White : Color.WhiteSmoke;
            break;
         case 40:
            color = Color.Black;
            break;
         case 41:
            color = Color.Maroon;
            break;
         case 42:
            color = Color.Green;
            break;
         case 43:
            color = Color.Gold;
            break;
         case 44:
            color = Color.DarkBlue;
            break;
         case 45:
            color = Color.DarkMagenta;
            break;
         case 46:
            color = Color.DarkCyan;
            break;
         case 47:
            color = Color.LightGray;
            break;
         default:
            throw new ArgumentOutOfRangeException($"Color code {code} is unrecognized as a basic ansi color code.");
      }

      return color;
   }

   /// <summary>
   /// Converts the xterm color code into a drawing color.
   /// </summary>
   /// <param name="colorNum">The Xterm color code.</param>
   /// <returns>A <see cref="Color"/> represented by the code.</returns>
   /// <exception cref="ArgumentOutOfRangeException">Color code is unrecognized as an Xterm color code.</exception>
   public static Color ConvertXtermColorCode(int colorNum)
   {
      switch (colorNum)
      {
         case 0:
            return Color.Black;
         case 1:
            return Color.Maroon;
         case 2:
            return Color.Green;
         case 3:
            return Color.Olive;
         case 4:
            return Color.FromArgb(0, 0, 128);
         case 5:
            return Color.Purple;
         case 6:
            return Color.Teal;
         case 7:
            return Color.Silver;
         case 8:
            return Color.Gray;
         case 9:
            return Color.Red;
         case 10:
            return Color.Lime;
         case 11:
            return Color.Yellow;
         case 12:
            return Color.Blue;
         case 13:
            return Color.Fuchsia;
         case 14:
            return Color.Aqua;
         case 15:
            return Color.White;
         case >= 16 and <= 231:
            {
               int floorColorNum = colorNum - 16;
               var red = floorColorNum / 36 * 40;
               int green = (floorColorNum / 6) % 6 * 40;
               var blue = floorColorNum % 6 * 40;
               return Color.FromArgb(
                                      red == 0 ? 0 : 55 + red,
                                      green == 0 ? 0 : 55 + green,
                                      blue == 0 ? 0 : 55 + blue);
            }
         case >= 232 and <= 255:
            {
               int rgb = (colorNum - 232) * 10 + 8;
               return Color.FromArgb(rgb, rgb, rgb);
            }
         default:
            throw new ArgumentOutOfRangeException(nameof(colorNum), "Value must be from 0 to 255.");
      }
   }

   /// <summary>
   /// Converts the true color code into a drawing color.
   /// </summary>
   /// <param name="red">The red.</param>
   /// <param name="green">The green.</param>
   /// <param name="blue">The blue.</param>
   /// <returns>A <see cref="Color"/> represented by the code.</returns>
   /// <exception cref="System.ArgumentOutOfRangeException">
   /// red or green or blue are either less than 0 or greater than 255.
   /// </exception>
   public static Color ConvertTrueColorCode(int red, int green, int blue)
   {
      if (red < 0 || red > 255) throw new ArgumentOutOfRangeException(nameof(red), "Value must be from 0 to 255.");
      if (green < 0 || green > 255) throw new ArgumentOutOfRangeException(nameof(green), "Value must be from 0 to 255.");
      if (blue < 0 || blue > 255) throw new ArgumentOutOfRangeException(nameof(blue), "Value must be from 0 to 255.");

      return Color.FromArgb(red, green, blue);
   }

   /// <summary>
   /// Converts the moo color name to a drawing color instance.
   /// </summary>
   /// <param name="colorName">Name of the color.</param>
   /// <param name="bright">if set to <c>true</c> [bright].</param>
   /// <returns>A <see cref="Color"/> represented by the code.</returns>
   /// <exception cref="System.ArgumentOutOfRangeException">colorName - Value is not a recognized Moo ansi color name.</exception>
   public static Color ConvertStandardAnsiMooColorName(string colorName, bool bright)
   {
      if (string.IsNullOrEmpty(colorName))
         throw new ArgumentOutOfRangeException(nameof(colorName),
                                               $"Value {colorName} is not a recognized Moo ansi color name.");

      var c = colorName.ToLowerInvariant();
      if (c == "black")
         return ConvertBasicAnsiColorCode(30, bright);
      if (c == "red")
         return ConvertBasicAnsiColorCode(31, bright);
      if (c == "green")
         return ConvertBasicAnsiColorCode(32, bright);
      if (c == "yellow")
         return ConvertBasicAnsiColorCode(33, bright);
      if (c == "blue")
         return ConvertBasicAnsiColorCode(34, bright);
      if (c == "magenta")
         return ConvertBasicAnsiColorCode(35, bright);
      if (c == "cyan")
         return ConvertBasicAnsiColorCode(36, bright);
      if (c == "white")
         return ConvertBasicAnsiColorCode(37, bright);
      // ReSharper disable StringLiteralTypo
      if (c is "b:black" or "bgblack")
         return ConvertBasicAnsiColorCode(40, bright);
      if (c is "b:red" or "bgred")
         return ConvertBasicAnsiColorCode(41, bright);
      if (c is "b:green" or "bggeen")
         return ConvertBasicAnsiColorCode(42, bright);
      if (c is "b:yellow" or "bgyellow")
         return ConvertBasicAnsiColorCode(43, bright);
      if (c is "b:blue" or "bgblue")
         return ConvertBasicAnsiColorCode(44, bright);
      if (c is "b:magenta" or "bgmagenta")
         return ConvertBasicAnsiColorCode(45, bright);
      if (c is "b:cyan" or "bgcyan")
         return ConvertBasicAnsiColorCode(46, bright);
      if (c is "b:white" or "bgwhite")
         return ConvertBasicAnsiColorCode(47, bright);
      if (c[0] == ':' && c.Length > 1)
      {
         var xtermCode = c.Substring(1);
         if (int.TryParse(xtermCode, out var xcode))
            return ConvertXtermColorCode(xcode);
      }
      if (c.Length > 2 && c.StartsWith("b:"))
      {
         var code = c.Substring(2);
         if (int.TryParse(code, out var xcode))
            return ConvertXtermColorCode(xcode);
         var codes = ParseRgb(code);
         if (codes != null)
            return ConvertTrueColorCode(codes[0], codes[1], codes[2]);
      }
      else if (c.Length == 11)
      {
         var codes = ParseRgb(c);
         if (codes != null)
            return ConvertTrueColorCode(codes[0], codes[1], codes[2]);
      }

      // ReSharper restore StringLiteralTypo
      throw new ArgumentOutOfRangeException(nameof(colorName),
                                            $"Value {colorName} is not a recognized Moo ansi color name.");
   }

   /// <summary>
   /// Parses a string RGB sequence delimited by colons into a list of color integer codes.
   /// </summary>
   /// <param name="colorCodes">The color codes.</param>
   /// <returns>A list of integers.</returns>
   private static List<int>? ParseRgb(string colorCodes)
   {
      var codes = colorCodes.Split(':');
      if (codes.Length != 3)
         return null;

      var rgb = new List<int>();
      foreach (var code in codes)
      {
         if (!int.TryParse(code, out var colorInt))
            return null;

         rgb.Add(colorInt);
      }

      return rgb;
   }

   /// <summary>
   /// Converts the extended Moo color name to a drawing color.
   /// </summary>
   /// <param name="colorName">Name of the extended color.</param>
   /// <returns>A <see cref="Color"/> represented by the code.</returns>
   /// <exception cref="Exception">Color name is not recognized.</exception>
   public static Color ConvertExtendedMooColorName(string colorName)
   {
      return ColorTranslator.FromHtml(colorName);
   }
}