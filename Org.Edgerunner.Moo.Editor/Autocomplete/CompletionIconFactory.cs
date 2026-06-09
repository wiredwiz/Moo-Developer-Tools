#region BSD 3-Clause License
// <copyright file="CompletionIconFactory.cs" company="Edgerunner.org">
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Paints the autocomplete popup category icons at runtime using GDI+ (no external image assets).
/// </summary>
/// <remarks>
/// Each icon is rendered into a 64x64 32bpp-ARGB <see cref="Bitmap"/>. The source designs are the
/// 24x24 viewBox SVGs defined in <c>docs/keyword-icon-picker.html</c>; coordinates are translated
/// here and uniformly scaled by 64/24 via a world transform, so the GDI+ geometry mirrors the SVG
/// path data 1:1 at 24 units before scaling. The base resolution is deliberately higher than the
/// nominal 16px display size so the autocomplete popup can downscale crisply when the editor is
/// zoomed (sharp through ~400% zoom — a pure downscale from 64px).
/// Styles: CHIP = filled accent circle with a white centred glyph; LINE = the glyph stroked in the
/// accent colour on a transparent background; SOLID = a filled accent shape (with white detail
/// where noted).
/// The built <see cref="ImageList"/> is cached statically so it is created once and shared by all
/// editors.
/// </remarks>
public static class CompletionIconFactory
{
   private const int IconSize = 64;

   // Source viewBox of the reference SVGs.
   private const float ViewBox = 24f;

   private const float Scale = IconSize / ViewBox;

   private static readonly object SyncRoot = new();

   private static ImageList _sharedImageList;

   private static readonly Dictionary<CompletionIconCategory, Color> Colors = new()
   {
      [CompletionIconCategory.Constant]      = ColorTranslator.FromHtml("#E2A33B"), // amber
      [CompletionIconCategory.Variable]      = ColorTranslator.FromHtml("#4C8DF6"), // blue
      [CompletionIconCategory.CoreReference] = ColorTranslator.FromHtml("#19B27A"), // green
      [CompletionIconCategory.Verb]          = ColorTranslator.FromHtml("#A06CF0"), // purple
      [CompletionIconCategory.Property]      = ColorTranslator.FromHtml("#15B5C9"), // cyan
      [CompletionIconCategory.Function]      = ColorTranslator.FromHtml("#EC5C9B"), // pink
      [CompletionIconCategory.Snippet]       = ColorTranslator.FromHtml("#8FBF3F"), // lime
      [CompletionIconCategory.Keyword]       = ColorTranslator.FromHtml("#5E7CA8"), // steel
   };

   /// <summary>
   /// Builds a freshly-rendered 64x64 icon bitmap for the supplied <paramref name="category"/>.
   /// </summary>
   /// <param name="category">The completion category to render.</param>
   /// <returns>A new 64x64 32bpp-ARGB <see cref="Image"/>. The caller owns the returned instance.</returns>
   public static Image GetIcon(CompletionIconCategory category)
   {
      var bmp = new Bitmap(IconSize, IconSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      bmp.MakeTransparent();
      using var g = Graphics.FromImage(bmp);
      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
      g.PixelOffsetMode = PixelOffsetMode.HighQuality;
      g.InterpolationMode = InterpolationMode.HighQualityBicubic;

      // Draw using the 24-unit reference coordinate system, scaled up to the 64px base resolution.
      g.ScaleTransform(Scale, Scale);

      Draw(g, category, Colors[category]);
      return bmp;
   }

   /// <summary>
   /// Returns the shared <see cref="ImageList"/> containing all eight category icons in
   /// <see cref="CompletionIconCategory"/> order (so <c>ImageIndex == (int)category</c>).
   /// The instance is created on first call and reused thereafter.
   /// </summary>
   /// <returns>The cached, shared image list.</returns>
   public static ImageList CreateImageList()
   {
      if (_sharedImageList != null)
         return _sharedImageList;

      lock (SyncRoot)
      {
         if (_sharedImageList != null)
            return _sharedImageList;

         var imageList = new ImageList
         {
            ImageSize = new Size(IconSize, IconSize),
            ColorDepth = ColorDepth.Depth32Bit
         };

         foreach (CompletionIconCategory category in Enum.GetValues(typeof(CompletionIconCategory)))
            imageList.Images.Add(GetIcon(category));

         _sharedImageList = imageList;
      }

      return _sharedImageList;
   }

   private static void Draw(Graphics g, CompletionIconCategory category, Color accent)
   {
      switch (category)
      {
         case CompletionIconCategory.Constant:
            DrawChip(g, accent, "C", italic: false);
            break;
         case CompletionIconCategory.Variable:
            DrawChip(g, accent, "x", italic: true);
            break;
         case CompletionIconCategory.Keyword:
            DrawChip(g, accent, "K", italic: false);
            break;
         case CompletionIconCategory.CoreReference:
            DrawCoreReference(g, accent);
            break;
         case CompletionIconCategory.Function:
            DrawFunction(g, accent);
            break;
         case CompletionIconCategory.Verb:
            DrawVerb(g, accent);
            break;
         case CompletionIconCategory.Property:
            DrawProperty(g, accent);
            break;
         case CompletionIconCategory.Snippet:
            DrawSnippet(g, accent);
            break;
      }
   }

   /// <summary>
   /// CHIP style: a filled accent circle (cx=12 cy=12 r=9.4) with a white centred glyph.
   /// SVG: &lt;circle cx="12" cy="12" r="9.4"/&gt; + centred &lt;text&gt;.
   /// </summary>
   private static void DrawChip(Graphics g, Color accent, string glyph, bool italic)
   {
      const float cx = 12f, cy = 12f, r = 9.4f;
      using (var brush = new SolidBrush(accent))
         g.FillEllipse(brush, cx - r, cy - r, r * 2f, r * 2f);

      // The reference uses font-size ~13 (12.5 for 'K') in the 24-unit space.
      var style = italic ? FontStyle.Italic | FontStyle.Bold : FontStyle.Bold;
      var family = italic ? "Georgia" : "Arial";
      using var font = new Font(family, 12.5f, style, GraphicsUnit.Pixel);
      using var format = new StringFormat
      {
         Alignment = StringAlignment.Center,
         LineAlignment = StringAlignment.Center
      };
      using var white = new SolidBrush(Color.White);
      // Centre the glyph in the chip's bounding box.
      var box = new RectangleF(cx - r, cy - r, r * 2f, r * 2f);
      g.DrawString(glyph, font, white, box, format);
   }

   /// <summary>
   /// LINE style core reference: an accent-coloured '$' glyph (stem + S curve), no badge.
   /// SVG path d="M12 4.2V19.8M15.6 7.6c... (stroke 1.7).
   /// Rendered with a $-shaped string for fidelity of the S curve.
   /// </summary>
   private static void DrawCoreReference(Graphics g, Color accent)
   {
      using var font = new Font("Arial", 17f, FontStyle.Bold, GraphicsUnit.Pixel);
      using var format = new StringFormat
      {
         Alignment = StringAlignment.Center,
         LineAlignment = StringAlignment.Center
      };
      using var brush = new SolidBrush(accent);
      g.DrawString("$", font, brush, new RectangleF(0, 0, ViewBox, ViewBox), format);
   }

   /// <summary>
   /// LINE style function: accent-coloured parentheses '( )'.
   /// SVG: two arc paths stroked 1.7. Drawn as two open arcs.
   /// </summary>
   private static void DrawFunction(Graphics g, Color accent)
   {
      using var pen = new Pen(accent, 1.7f)
      {
         StartCap = LineCap.Round,
         EndCap = LineCap.Round,
         LineJoin = LineJoin.Round
      };

      // Left paren "(" : arc opening to the right, centred vertically.
      // Approximates SVG "M9.5 4.6C6.6 7.4 6.6 16.6 9.5 19.4".
      g.DrawArc(pen, 6.0f, 4.6f, 7.0f, 14.8f, 110f, 140f);

      // Right paren ")" : arc opening to the left.
      // Approximates SVG "M14.5 4.6c2.9 2.8 2.9 12 0 14.8".
      g.DrawArc(pen, 11.0f, 4.6f, 7.0f, 14.8f, -70f, 140f);
   }

   /// <summary>
   /// SOLID style verb: a filled lightning bolt.
   /// SVG path d="M13.6 2.5 5 14h5.4l-1.5 7.5L19 9.5h-5.6z".
   /// </summary>
   private static void DrawVerb(Graphics g, Color accent)
   {
      using var path = new GraphicsPath();
      var pts = new[]
      {
         new PointF(13.6f, 2.5f),
         new PointF(5f, 14f),
         new PointF(10.4f, 14f),   // 5 + 5.4
         new PointF(8.9f, 21.5f),  // 10.4 - 1.5, 14 + 7.5
         new PointF(19f, 9.5f),
         new PointF(13.4f, 9.5f),  // 19 - 5.6
      };
      path.AddPolygon(pts);
      using var brush = new SolidBrush(accent);
      g.FillPath(brush, path);
   }

   /// <summary>
   /// SOLID style property: a filled tag with a small punched hole.
   /// SVG path d="M12.8 3 21 11.2a1.8 1.8 0 0 1 0 2.6l-7.2 7.2a1.8 1.8 0 0 1-2.6 0L3 12.8V3z"
   /// + punched hole circle cx=8.3 cy=8.3 r=1.9.
   /// The rounded corner of the tag is approximated with a straight corner.
   /// </summary>
   private static void DrawProperty(Graphics g, Color accent)
   {
      using var path = new GraphicsPath();
      // Tag outline (rounded tip corner approximated by its corner point ~ (22.3, 12.5)).
      var pts = new[]
      {
         new PointF(12.8f, 3f),
         new PointF(22.3f, 12.5f), // rounded outer corner of the tag point
         new PointF(13.8f, 21.5f), // bottom point after the curve back (11.2 + 2.6 left)
         new PointF(3f, 12.8f),
         new PointF(3f, 3f),
      };
      path.AddPolygon(pts);
      using (var brush = new SolidBrush(accent))
         g.FillPath(brush, path);

      // Punched hole: cut out with a transparent (background) ellipse.
      const float hx = 8.3f, hy = 8.3f, hr = 1.9f;
      var prevMode = g.CompositingMode;
      g.CompositingMode = CompositingMode.SourceCopy;
      using (var hole = new SolidBrush(Color.Transparent))
         g.FillEllipse(hole, hx - hr, hy - hr, hr * 2f, hr * 2f);
      g.CompositingMode = prevMode;
   }

   /// <summary>
   /// SOLID style snippet: a filled accent document with a WHITE folded corner and
   /// WHITE inner '&lt;&gt;' marks.
   /// SVG: body path d="M6 3h8l4 4v14H6z"; fold path d="M14 3v4h4" (white, faint);
   /// brackets d="M10.2 11.5 8 14l2.2 2.5M13.8 11.5 16 14l-2.2 2.5" stroked white 1.7.
   /// </summary>
   private static void DrawSnippet(Graphics g, Color accent)
   {
      // Document body.
      using (var body = new GraphicsPath())
      {
         body.AddPolygon(new[]
         {
            new PointF(6f, 3f),
            new PointF(14f, 3f),
            new PointF(18f, 7f),
            new PointF(18f, 21f),
            new PointF(6f, 21f),
         });
         using var brush = new SolidBrush(accent);
         g.FillPath(brush, body);
      }

      // Folded corner: a faint white triangle (matches SVG fold opacity .35).
      using (var fold = new GraphicsPath())
      {
         fold.AddPolygon(new[]
         {
            new PointF(14f, 3f),
            new PointF(14f, 7f),
            new PointF(18f, 7f),
         });
         using var foldBrush = new SolidBrush(Color.FromArgb(90, 255, 255, 255));
         g.FillPath(foldBrush, fold);
      }

      // Inner "<>" marks in white.
      using var pen = new Pen(Color.White, 1.7f)
      {
         StartCap = LineCap.Round,
         EndCap = LineCap.Round,
         LineJoin = LineJoin.Round
      };
      // "<" : 10.2,11.5 -> 8,14 -> 10.2,16.5
      g.DrawLines(pen, new[]
      {
         new PointF(10.2f, 11.5f),
         new PointF(8f, 14f),
         new PointF(10.2f, 16.5f),
      });
      // ">" : 13.8,11.5 -> 16,14 -> 13.8,16.5
      g.DrawLines(pen, new[]
      {
         new PointF(13.8f, 11.5f),
         new PointF(16f, 14f),
         new PointF(13.8f, 16.5f),
      });
   }
}
