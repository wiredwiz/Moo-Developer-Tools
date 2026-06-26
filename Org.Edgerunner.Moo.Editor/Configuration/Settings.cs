#region BSD 3-Clause License
// <copyright file="Settings.cs" company="Edgerunner.org">
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

using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Text;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace Org.Edgerunner.Moo.Editor.Configuration
{
   /// <summary>
   /// Describes the outcome of an <see cref="Settings.ImportThemeFromJson"/> call.
   /// </summary>
   public sealed class ThemeImportResult
   {
      /// <summary>
      /// Gets the name of a requested font family that was not installed locally, or <see langword="null"/>
      /// when the imported font (if any) was available.
      /// </summary>
      /// <value>The missing font family name, or <see langword="null"/>.</value>
      public string MissingFontName { get; init; }
   }

   /// <summary>
   /// Wrapper object describing the JSON theme file shape (name, format version and settings map).
   /// </summary>
   internal sealed class ThemeFile
   {
      [JsonPropertyName("name")]
      public string Name { get; set; }

      [JsonPropertyName("formatVersion")]
      public int FormatVersion { get; set; }

      [JsonPropertyName("settings")]
      public Dictionary<string, string> Settings { get; set; }
   }
   /// <summary>
   /// Class that represents various code editor settings.
   /// </summary>
   [SuppressMessage("ReSharper", "CatchAllClause", Justification = "We want to recover gracefully regardless of errors in settings.")]
   public class Settings
   {
       public static Settings Instance { get; } = new Settings();

      /// <summary>
      /// Gets or sets the color of keywords.
      /// </summary>
      /// <value>The color of keywords.</value>
      public Color KeywordColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of keywords.
      /// </summary>
      /// <value>The background color of keywords.</value>
      public Color KeywordBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of keywords.
      /// </summary>
      /// <value>The font style of keywords.</value>
      public FontStyle KeywordFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of comments.
      /// </summary>
      /// <value>The color of comments.</value>
      public Color CommentColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of comments.
      /// </summary>
      /// <value>The background color of comments.</value>
      public Color CommentBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of comments.
      /// </summary>
      /// <value>The font style of comments.</value>
      public FontStyle CommentFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of literals.
      /// </summary>
      /// <value>The color of literals.</value>
      /// <remarks>String and object literals have their own settings</remarks>
      public Color LiteralColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of literals.
      /// </summary>
      /// <value>The background color of literals.</value>
      /// <remarks>String and object literals have their own settings</remarks>
      public Color LiteralBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of literals.
      /// </summary>
      /// <value>The font style of literals.</value>
      /// <remarks>String and object literals have their own settings</remarks>
      public FontStyle LiteralFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of strings.
      /// </summary>
      /// <value>The color of strings.</value>
      public Color StringColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of strings.
      /// </summary>
      /// <value>The background color of strings.</value>
      public Color StringBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of strings.
      /// </summary>
      /// <value>The font style of strings.</value>
      public FontStyle StringFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of symbols.
      /// </summary>
      /// <value>The color of symbols.</value>
      /// <remarks>Operators and the various braces have their own colors</remarks>
      public Color SymbolColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of symbols.
      /// </summary>
      /// <value>The background color of symbols.</value>
      /// <remarks>Operators and the various braces have their own colors</remarks>
      public Color SymbolBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of symbols.
      /// </summary>
      /// <value>The font style of symbols.</value>
      /// <remarks>Operators and the various braces have their own colors</remarks>
      public FontStyle SymbolFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of parentheses.
      /// </summary>
      /// <value>The color of parentheses.</value>
      public Color ParenthesisColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of parentheses.
      /// </summary>
      /// <value>The background color of parentheses.</value>
      public Color ParenthesisBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of parentheses.
      /// </summary>
      /// <value>The font style of parentheses.</value>
      public FontStyle ParenthesisFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of operators.
      /// </summary>
      /// <value>The color of operators.</value>
      public Color OperatorColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of operators.
      /// </summary>
      /// <value>The background color of operators.</value>
      public Color OperatorBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of operators.
      /// </summary>
      /// <value>The font style of operators.</value>
      public FontStyle OperatorFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of brackets.
      /// </summary>
      /// <value>The color of brackets.</value>
      public Color BracketColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of brackets.
      /// </summary>
      /// <value>The background color of brackets.</value>
      public Color BracketBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of brackets.
      /// </summary>
      /// <value>The font style of brackets.</value>
      public FontStyle BracketFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of curly braces.
      /// </summary>
      /// <value>The color of curly braces.</value>
      public Color CurlyBraceColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of curly braces.
      /// </summary>
      /// <value>The background color of curly braces.</value>
      public Color CurlyBraceBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of curly braces.
      /// </summary>
      /// <value>The font style of curly braces.</value>
      public FontStyle CurlyBraceFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of objects.
      /// </summary>
      /// <value>The color of objects.</value>
      public Color ObjectColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of objects.
      /// </summary>
      /// <value>The background color of objects.</value>
      public Color ObjectBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of objects.
      /// </summary>
      /// <value>The font style of objects.</value>
      public FontStyle ObjectFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of core references.
      /// </summary>
      /// <value>The color of core references.</value>
      public Color CoreReferenceColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of core references.
      /// </summary>
      /// <value>The background color of core references.</value>
      public Color CoreReferenceBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of core references.
      /// </summary>
      /// <value>The font style of core references.</value>
      public FontStyle CoreReferenceFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of builtin variables.
      /// </summary>
      /// <value>The color of builtin variables.</value>
      public Color BuiltinVariableColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of builtin variables.
      /// </summary>
      /// <value>The background color of builtin variables.</value>
      public Color BuiltinVariableBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of builtin variables.
      /// </summary>
      /// <value>The font style of builtin variables.</value>
      public FontStyle BuiltinVariableFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of builtin functions.
      /// </summary>
      /// <value>The color of builtin functions.</value>
      public Color BuiltinFunctionColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of builtin functions.
      /// </summary>
      /// <value>The background color of builtin functions.</value>
      public Color BuiltinFunctionBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of builtin functions.
      /// </summary>
      /// <value>The font style of builtin functions.</value>
      public FontStyle BuiltinFunctionFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of verbs.
      /// </summary>
      /// <value>The color of verbs.</value>
      public Color VerbColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of verbs.
      /// </summary>
      /// <value>The background color of verbs.</value>
      public Color VerbBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of verbs.
      /// </summary>
      /// <value>The font style of verbs.</value>
      public FontStyle VerbFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the color of properties.
      /// </summary>
      /// <value>The color of properties.</value>
      public Color PropertyColor { get; set; }

      /// <summary>
      /// Gets or sets the background color of properties.
      /// </summary>
      /// <value>The background color of properties.</value>
      public Color PropertyBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the font style of properties.
      /// </summary>
      /// <value>The font style of properties.</value>
      public FontStyle PropertyFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the default color of words.
      /// </summary>
      /// <value>The default color of words.</value>
      public Color DefaultWordColor { get; set; }

      /// <summary>
      /// Gets or sets the default background color of words.
      /// </summary>
      /// <value>The default background color of words.</value>
      public Color DefaultWordBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the default font style of words.
      /// </summary>
      /// <value>The default font style of words.</value>
      public FontStyle DefaultWordFontStyle { get; set; }

      /// <summary>
      /// Gets or sets the editor font family.
      /// </summary>
      /// <value>The editor font family.</value>
      public FontFamily EditorFontFamily { get; set; }

      /// <summary>
      /// Gets or sets the size of the editor font.
      /// </summary>
      /// <value>The size of the editor font.</value>
      public float EditorFontSize { get; set; }

      /// <summary>
      /// Gets or sets the the editor text color.
      /// </summary>
      /// <value>The editor text color.</value>
      public Color EditorTextColor { get; set; }

      /// <summary>
      /// Gets or sets the editor background color.
      /// </summary>
      /// <value>The editor background color.</value>
      public Color EditorBackgroundColor { get; set; }

      /// <summary>
      /// Gets or sets the editor caret color.
      /// </summary>
      /// <value>The editor caret color.</value>
      public Color EditorCaretColor { get; set; }

      /// <summary>
      /// Gets or sets the the editor line number indicator color.
      /// </summary>
      /// <value>The editor line number color.</value>
      public Color EditorLineNumberColor { get; set; }

      /// <summary>
      /// Gets or sets the editor "current line" color.
      /// </summary>
      /// <value>The editor "current line" color.</value>
      public Color EditorCurrentLineColor { get; set; }

      /// <summary>
      /// Gets or sets the editor text selection color.
      /// </summary>
      /// <value>The editor text selection color.</value>
      public Color EditorTextSelectionColor { get; set; }

      /// <summary>
      /// Gets or sets the color of the editor folding indicator.
      /// </summary>
      /// <value>
      /// The color of the editor folding indicator.
      /// </value>
      public Color EditorFoldingIndicatorColor { get; set; }

      /// <summary>
      /// Gets or sets the color of changed lines in the editor.
      /// </summary>
      /// <value>
      /// The color of changed lines in the editor.
      /// </value>
      public Color EditorChangedLineColor { get; set; }

      /// <summary>
      /// Gets or sets the back color of indents in the editor.
      /// </summary>
      /// <value>
      /// The back color of indents in the editor.
      /// </value>
      public Color EditorIndentBackColor { get; set; }

      /// <summary>
      /// Gets or sets the color of bookmarks in the editor.
      /// </summary>
      /// <value>
      /// The color of bookmarks in the editor.
      /// </value>
      public Color EditorBookmarkColor { get; set; }

      /// <summary>
      /// Gets or sets the color of service lines in the editor.
      /// </summary>
      /// <value>
      /// The color of service lines in the editor.
      /// </value>
      public Color EditorServiceLineColor { get; set; }

      /// <summary>
      /// Gets or sets a value that determines whether to show the current folding block highlight.
      /// </summary>
      /// <value><c>true</c> if folding block highlights are enabled; otherwise, <c>false</c>.</value>
      public bool EditorShowFoldingBlockHighlights { get; set; }

      /// <summary>
      /// Gets or sets the color of the current folding block in the editor.
      /// </summary>
      /// <value>
      /// The color of the current folding block in the editor.
      /// </value>
      public Color EditorFoldingHighlightColor { get; set; }

      /// <summary>
      /// Gets or sets the editor default zoom factor.
      /// </summary>
      /// <value>The editor zoom factor.</value>
      public int EditorZoomFactor { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether word wrap is enabled for the editor.
      /// </summary>
      /// <value><c>true</c> if word wrap enabled; otherwise, <c>false</c>.</value>
      public bool EditorWordWrap { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether word wrapped text should be indented.
      /// </summary>
      /// <value><c>true</c> if word wrap auto-indent is enabled; otherwise, <c>false</c>.</value>
      public bool EditorWordWrapAutoIndent { get; set; }

      /// <summary>
      /// Gets or sets the editor word wrap indent.
      /// </summary>
      /// <value>The editor word wrap indent.</value>
      public int EditorWordWrapIndent { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether automatic indent is enabled for the editor.
      /// </summary>
      /// <value><c>true</c> if automatic indent enabled; otherwise, <c>false</c>.</value>
      public bool EditorAutoIndent { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether to use a dark theme for the editor.
      /// </summary>
      /// <value><c>true</c> if dark theme enabled; otherwise, <c>false</c>.</value>
      public bool EditorDarkTheme { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether code folding is enabled for the editor.
      /// </summary>
      /// <value><c>true</c> if code folding enabled; otherwise, <c>false</c>.</value>
      public bool EditorShowCodeFolding { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether text indentation guides are enabled for the editor.
      /// </summary>
      /// <value><c>true</c> if text indentation guides enabled; otherwise, <c>false</c>.</value>
      public bool EditorShowTextIndentGuides { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether automatic brackets is enabled for the editor.
      /// </summary>
      /// <value><c>true</c> if automatic brackets enabled; otherwise, <c>false</c>.</value>
      public bool EditorAutoBrackets { get; set; }

      /// <summary>
      /// Gets or sets the number of spaces to convert tabs to.
      /// </summary>
      /// <value>The number of spaces representing a tab.</value>
      public int EditorTabLength { get; set; }

      /// <summary>
      /// Gets or sets the number of milliseconds delay before the autocomplete menu displays.
      /// </summary>
      /// <value>The number of milliseconds to wait before displaying the autocomplete menu.</value>
      public int EditorAutocompleteDelay { get; set; }

      /// <summary>
      /// Gets or sets the lifetime, in seconds, of cached editor world-query/member-completion data.
      /// </summary>
      /// <value>The number of seconds a cached value lives before it is re-fetched. Defaults to 60.</value>
      /// <remarks>
      /// This single setting drives both the member-completion controller cache and the per-connection
      /// <see cref="T:Org.Edgerunner.Mud.Common.Querying.CachingMooWorldQueryProvider"/> time-to-live.
      /// </remarks>
      public int EditorCacheTtlSeconds { get; set; }

      /// <summary>
      /// Gets or sets the color of error indicators.
      /// </summary>
      /// <value>The color of error indicators.</value>
      public Color ErrorIndicatorColor { get; set; }

      /// <summary>
      /// Gets or sets the parser message font family.
      /// </summary>
      /// <value>The parser message font family.</value>
      public FontFamily ParserMessageFontFamily { get; set; }

      /// <summary>
      /// Gets or sets the size of the parser message font.
      /// </summary>
      /// <value>The size of the parser message font.</value>
      public float ParserMessageFontSize { get; set; }

      /// <summary>
      /// Gets or sets the default Moo grammar dialect.
      /// </summary>
      /// <value>The default Moo grammar dialect.</value>
      public GrammarDialect DefaultGrammarDialect { get; set; }

      public FontStyle ParseFontStyles(string styles, FontStyle defaultStyle)
      {
         if (string.IsNullOrEmpty(styles))
            return defaultStyle;

         var styleGroup = styles.Split(';').ToList();
         FontStyle result = 0;
         foreach (var word in styleGroup)
            if (Enum.TryParse(word, out FontStyle style))
               result |= style;
            else
               return defaultStyle;

         return result;
      }

      /// <summary>
      /// The current theme JSON format version written by <see cref="ExportThemeToJson"/>.
      /// </summary>
      private const int ThemeFormatVersion = 1;

      /// <summary>
      /// Builds the appearance-only key/value dictionary that the JSON theme file persists.
      /// </summary>
      /// <returns>An ordered dictionary of theme keys to their serialized string values.</returns>
      /// <remarks>
      /// The scope is deliberately the appearance subset only: every syntax-token foreground,
      /// background and font style, every editor-chrome color, <see cref="ErrorIndicatorColor"/>,
      /// <see cref="EditorFontFamily"/>, <see cref="EditorFontSize"/> and <see cref="EditorDarkTheme"/>.
      /// Behavior settings (word wrap, tab length, autocomplete delay, dialect, zoom, etc.) are excluded.
      /// </remarks>
      private Dictionary<string, string> BuildThemeDictionary()
      {
         return new Dictionary<string, string>
         {
            // Syntax highlighting
            ["DefaultWordColor"] = SerializeColor(DefaultWordColor),
            ["DefaultWordBackgroundColor"] = SerializeColor(DefaultWordBackgroundColor),
            ["DefaultWordFontStyle"] = SerializeFontStyle(DefaultWordFontStyle),
            ["CommentColor"] = SerializeColor(CommentColor),
            ["CommentBackgroundColor"] = SerializeColor(CommentBackgroundColor),
            ["CommentFontStyle"] = SerializeFontStyle(CommentFontStyle),
            ["KeywordColor"] = SerializeColor(KeywordColor),
            ["KeywordBackgroundColor"] = SerializeColor(KeywordBackgroundColor),
            ["KeywordFontStyle"] = SerializeFontStyle(KeywordFontStyle),
            ["LiteralColor"] = SerializeColor(LiteralColor),
            ["LiteralBackgroundColor"] = SerializeColor(LiteralBackgroundColor),
            ["LiteralFontStyle"] = SerializeFontStyle(LiteralFontStyle),
            ["StringColor"] = SerializeColor(StringColor),
            ["StringBackgroundColor"] = SerializeColor(StringBackgroundColor),
            ["StringFontStyle"] = SerializeFontStyle(StringFontStyle),
            ["SymbolColor"] = SerializeColor(SymbolColor),
            ["SymbolBackgroundColor"] = SerializeColor(SymbolBackgroundColor),
            ["SymbolFontStyle"] = SerializeFontStyle(SymbolFontStyle),
            ["OperatorColor"] = SerializeColor(OperatorColor),
            ["OperatorBackgroundColor"] = SerializeColor(OperatorBackgroundColor),
            ["OperatorFontStyle"] = SerializeFontStyle(OperatorFontStyle),
            ["ParenthesisColor"] = SerializeColor(ParenthesisColor),
            ["ParenthesisBackgroundColor"] = SerializeColor(ParenthesisBackgroundColor),
            ["ParenthesisFontStyle"] = SerializeFontStyle(ParenthesisFontStyle),
            ["CurlyBraceColor"] = SerializeColor(CurlyBraceColor),
            ["CurlyBraceBackgroundColor"] = SerializeColor(CurlyBraceBackgroundColor),
            ["CurlyBraceFontStyle"] = SerializeFontStyle(CurlyBraceFontStyle),
            ["BracketColor"] = SerializeColor(BracketColor),
            ["BracketBackgroundColor"] = SerializeColor(BracketBackgroundColor),
            ["BracketFontStyle"] = SerializeFontStyle(BracketFontStyle),
            ["ObjectColor"] = SerializeColor(ObjectColor),
            ["ObjectBackgroundColor"] = SerializeColor(ObjectBackgroundColor),
            ["ObjectFontStyle"] = SerializeFontStyle(ObjectFontStyle),
            ["CoreReferenceColor"] = SerializeColor(CoreReferenceColor),
            ["CoreReferenceBackgroundColor"] = SerializeColor(CoreReferenceBackgroundColor),
            ["CoreReferenceFontStyle"] = SerializeFontStyle(CoreReferenceFontStyle),
            ["BuiltinVariableColor"] = SerializeColor(BuiltinVariableColor),
            ["BuiltinVariableBackgroundColor"] = SerializeColor(BuiltinVariableBackgroundColor),
            ["BuiltinVariableFontStyle"] = SerializeFontStyle(BuiltinVariableFontStyle),
            ["BuiltinFunctionColor"] = SerializeColor(BuiltinFunctionColor),
            ["BuiltinFunctionBackgroundColor"] = SerializeColor(BuiltinFunctionBackgroundColor),
            ["BuiltinFunctionFontStyle"] = SerializeFontStyle(BuiltinFunctionFontStyle),
            ["VerbColor"] = SerializeColor(VerbColor),
            ["VerbBackgroundColor"] = SerializeColor(VerbBackgroundColor),
            ["VerbFontStyle"] = SerializeFontStyle(VerbFontStyle),
            ["PropertyColor"] = SerializeColor(PropertyColor),
            ["PropertyBackgroundColor"] = SerializeColor(PropertyBackgroundColor),
            ["PropertyFontStyle"] = SerializeFontStyle(PropertyFontStyle),

            // Editor fonts
            ["EditorFontFamily"] = EditorFontFamily?.Name ?? string.Empty,
            ["EditorFontSize"] = EditorFontSize.ToString(CultureInfo.InvariantCulture),

            // Editor chrome colors
            ["EditorTextColor"] = SerializeColor(EditorTextColor),
            ["EditorBackgroundColor"] = SerializeColor(EditorBackgroundColor),
            ["EditorCaretColor"] = SerializeColor(EditorCaretColor),
            ["EditorLineNumberColor"] = SerializeColor(EditorLineNumberColor),
            ["EditorCurrentLineColor"] = SerializeColor(EditorCurrentLineColor),
            ["EditorTextSelectionColor"] = SerializeColor(EditorTextSelectionColor),
            ["EditorFoldingIndicatorColor"] = SerializeColor(EditorFoldingIndicatorColor),
            ["EditorChangedLineColor"] = SerializeColor(EditorChangedLineColor),
            ["EditorIndentBackColor"] = SerializeColor(EditorIndentBackColor),
            ["EditorBookmarkColor"] = SerializeColor(EditorBookmarkColor),
            ["EditorServiceLineColor"] = SerializeColor(EditorServiceLineColor),
            ["EditorFoldingHighlightColor"] = SerializeColor(EditorFoldingHighlightColor),
            ["ErrorIndicatorColor"] = SerializeColor(ErrorIndicatorColor),

            // Theme flag
            ["EditorDarkTheme"] = EditorDarkTheme.ToString()
         };
      }

      /// <summary>
      /// Exports the appearance subset of this <see cref="Settings"/> instance to a JSON theme file.
      /// </summary>
      /// <param name="filePath">The destination file path.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/> or empty.</exception>
      /// <exception cref="T:System.IO.IOException">The file could not be written.</exception>
      /// <remarks>
      /// The written keys, color formats and font-style formats match what <see cref="ImportThemeFromJson"/>
      /// reads, so a subsequent import round-trips every theme value. Behavior settings are not exported.
      /// </remarks>
      public void ExportThemeToJson([NotNull] string filePath)
      {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

         var theme = new ThemeFile
         {
            Name = Path.GetFileNameWithoutExtension(filePath),
            FormatVersion = ThemeFormatVersion,
            Settings = BuildThemeDictionary()
         };

         try
         {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
               Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(theme, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json, new UTF8Encoding(false));
         }
         catch (Exception ex)
         {
            throw new IOException($"Failed to export theme to '{filePath}': {ex.Message}", ex);
         }
      }

      /// <summary>
      /// Imports the appearance subset of a JSON theme file into this <see cref="Settings"/> instance.
      /// </summary>
      /// <param name="filePath">The source file path.</param>
      /// <returns>A <see cref="ThemeImportResult"/> describing any font fallback that occurred.</returns>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/> or empty.</exception>
      /// <exception cref="T:System.IO.InvalidDataException">The file could not be read or is not a valid theme file.</exception>
      /// <remarks>
      /// Only keys present in the file's <c>settings</c> map are applied; absent keys and any behavior
      /// settings keep their current value, and unparseable values keep their current value. The file is
      /// fully read, deserialized and validated <em>before</em> this instance is mutated, so a malformed or
      /// unreadable file throws and leaves this instance completely unchanged.
      /// </remarks>
      public ThemeImportResult ImportThemeFromJson([NotNull] string filePath)
      {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

         // Fully read, parse and validate the wrapper BEFORE touching this instance (atomicity).
         ThemeFile theme;
         try
         {
            var json = File.ReadAllText(filePath);
            theme = JsonSerializer.Deserialize<ThemeFile>(json);
         }
         catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or ArgumentException)
         {
            throw new InvalidDataException($"The theme file '{filePath}' could not be read: {ex.Message}", ex);
         }

         if (theme == null)
            throw new InvalidDataException($"The theme file '{filePath}' did not contain a valid theme.");

         var values = theme.Settings;
         if (values == null || values.Count == 0)
            return new ThemeImportResult();

         string missingFontName = null;

         foreach (var pair in values)
         {
            var key = pair.Key;
            var value = pair.Value;
            if (value == null)
               continue;

            switch (key)
            {
               case "EditorFontFamily":
                  if (!string.IsNullOrEmpty(value))
                     EditorFontFamily = ResolveFontFamily(value, out missingFontName);
                  break;
               case "EditorFontSize":
                  if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fontSize))
                     EditorFontSize = fontSize;
                  break;
               case "EditorDarkTheme":
                  if (bool.TryParse(value, out var darkTheme))
                     EditorDarkTheme = darkTheme;
                  break;
               default:
                  ApplyThemeColorOrStyle(key, value);
                  break;
            }
         }

         return new ThemeImportResult { MissingFontName = missingFontName };
      }

      /// <summary>
      /// Resolves a font family name, falling back to generic monospace when the named family is not installed.
      /// </summary>
      /// <param name="name">The requested font family name.</param>
      /// <param name="missingFontName">Set to <paramref name="name"/> when the family was not available; otherwise unchanged.</param>
      /// <returns>The resolved <see cref="FontFamily"/>.</returns>
      private static FontFamily ResolveFontFamily(string name, out string missingFontName)
      {
         missingFontName = null;
         try
         {
            return new FontFamily(name);
         }
         catch (ArgumentException)
         {
            missingFontName = name;
            return FontFamily.GenericMonospace;
         }
      }

      /// <summary>
      /// Applies a single color or font-style theme value by key, ignoring unrecognized keys and
      /// keeping the current value when a color value fails to parse.
      /// </summary>
      /// <param name="key">The theme key.</param>
      /// <param name="value">The serialized value.</param>
      private void ApplyThemeColorOrStyle(string key, string value)
      {
         switch (key)
         {
            // Font styles
            case "DefaultWordFontStyle": DefaultWordFontStyle = ParseFontStyles(value, DefaultWordFontStyle); break;
            case "CommentFontStyle": CommentFontStyle = ParseFontStyles(value, CommentFontStyle); break;
            case "KeywordFontStyle": KeywordFontStyle = ParseFontStyles(value, KeywordFontStyle); break;
            case "LiteralFontStyle": LiteralFontStyle = ParseFontStyles(value, LiteralFontStyle); break;
            case "StringFontStyle": StringFontStyle = ParseFontStyles(value, StringFontStyle); break;
            case "SymbolFontStyle": SymbolFontStyle = ParseFontStyles(value, SymbolFontStyle); break;
            case "OperatorFontStyle": OperatorFontStyle = ParseFontStyles(value, OperatorFontStyle); break;
            case "ParenthesisFontStyle": ParenthesisFontStyle = ParseFontStyles(value, ParenthesisFontStyle); break;
            case "CurlyBraceFontStyle": CurlyBraceFontStyle = ParseFontStyles(value, CurlyBraceFontStyle); break;
            case "BracketFontStyle": BracketFontStyle = ParseFontStyles(value, BracketFontStyle); break;
            case "ObjectFontStyle": ObjectFontStyle = ParseFontStyles(value, ObjectFontStyle); break;
            case "CoreReferenceFontStyle": CoreReferenceFontStyle = ParseFontStyles(value, CoreReferenceFontStyle); break;
            case "BuiltinVariableFontStyle": BuiltinVariableFontStyle = ParseFontStyles(value, BuiltinVariableFontStyle); break;
            case "BuiltinFunctionFontStyle": BuiltinFunctionFontStyle = ParseFontStyles(value, BuiltinFunctionFontStyle); break;
            case "VerbFontStyle": VerbFontStyle = ParseFontStyles(value, VerbFontStyle); break;
            case "PropertyFontStyle": PropertyFontStyle = ParseFontStyles(value, PropertyFontStyle); break;

            // Colors
            case "DefaultWordColor": DefaultWordColor = ParseColor(value, DefaultWordColor); break;
            case "DefaultWordBackgroundColor": DefaultWordBackgroundColor = ParseColor(value, DefaultWordBackgroundColor); break;
            case "CommentColor": CommentColor = ParseColor(value, CommentColor); break;
            case "CommentBackgroundColor": CommentBackgroundColor = ParseColor(value, CommentBackgroundColor); break;
            case "KeywordColor": KeywordColor = ParseColor(value, KeywordColor); break;
            case "KeywordBackgroundColor": KeywordBackgroundColor = ParseColor(value, KeywordBackgroundColor); break;
            case "LiteralColor": LiteralColor = ParseColor(value, LiteralColor); break;
            case "LiteralBackgroundColor": LiteralBackgroundColor = ParseColor(value, LiteralBackgroundColor); break;
            case "StringColor": StringColor = ParseColor(value, StringColor); break;
            case "StringBackgroundColor": StringBackgroundColor = ParseColor(value, StringBackgroundColor); break;
            case "SymbolColor": SymbolColor = ParseColor(value, SymbolColor); break;
            case "SymbolBackgroundColor": SymbolBackgroundColor = ParseColor(value, SymbolBackgroundColor); break;
            case "OperatorColor": OperatorColor = ParseColor(value, OperatorColor); break;
            case "OperatorBackgroundColor": OperatorBackgroundColor = ParseColor(value, OperatorBackgroundColor); break;
            case "ParenthesisColor": ParenthesisColor = ParseColor(value, ParenthesisColor); break;
            case "ParenthesisBackgroundColor": ParenthesisBackgroundColor = ParseColor(value, ParenthesisBackgroundColor); break;
            case "CurlyBraceColor": CurlyBraceColor = ParseColor(value, CurlyBraceColor); break;
            case "CurlyBraceBackgroundColor": CurlyBraceBackgroundColor = ParseColor(value, CurlyBraceBackgroundColor); break;
            case "BracketColor": BracketColor = ParseColor(value, BracketColor); break;
            case "BracketBackgroundColor": BracketBackgroundColor = ParseColor(value, BracketBackgroundColor); break;
            case "ObjectColor": ObjectColor = ParseColor(value, ObjectColor); break;
            case "ObjectBackgroundColor": ObjectBackgroundColor = ParseColor(value, ObjectBackgroundColor); break;
            case "CoreReferenceColor": CoreReferenceColor = ParseColor(value, CoreReferenceColor); break;
            case "CoreReferenceBackgroundColor": CoreReferenceBackgroundColor = ParseColor(value, CoreReferenceBackgroundColor); break;
            case "BuiltinVariableColor": BuiltinVariableColor = ParseColor(value, BuiltinVariableColor); break;
            case "BuiltinVariableBackgroundColor": BuiltinVariableBackgroundColor = ParseColor(value, BuiltinVariableBackgroundColor); break;
            case "BuiltinFunctionColor": BuiltinFunctionColor = ParseColor(value, BuiltinFunctionColor); break;
            case "BuiltinFunctionBackgroundColor": BuiltinFunctionBackgroundColor = ParseColor(value, BuiltinFunctionBackgroundColor); break;
            case "VerbColor": VerbColor = ParseColor(value, VerbColor); break;
            case "VerbBackgroundColor": VerbBackgroundColor = ParseColor(value, VerbBackgroundColor); break;
            case "PropertyColor": PropertyColor = ParseColor(value, PropertyColor); break;
            case "PropertyBackgroundColor": PropertyBackgroundColor = ParseColor(value, PropertyBackgroundColor); break;

            // Editor chrome colors
            case "EditorTextColor": EditorTextColor = ParseColor(value, EditorTextColor); break;
            case "EditorBackgroundColor": EditorBackgroundColor = ParseColor(value, EditorBackgroundColor); break;
            case "EditorCaretColor": EditorCaretColor = ParseColor(value, EditorCaretColor); break;
            case "EditorLineNumberColor": EditorLineNumberColor = ParseColor(value, EditorLineNumberColor); break;
            case "EditorCurrentLineColor": EditorCurrentLineColor = ParseColor(value, EditorCurrentLineColor); break;
            case "EditorTextSelectionColor": EditorTextSelectionColor = ParseColor(value, EditorTextSelectionColor); break;
            case "EditorFoldingIndicatorColor": EditorFoldingIndicatorColor = ParseColor(value, EditorFoldingIndicatorColor); break;
            case "EditorChangedLineColor": EditorChangedLineColor = ParseColor(value, EditorChangedLineColor); break;
            case "EditorIndentBackColor": EditorIndentBackColor = ParseColor(value, EditorIndentBackColor); break;
            case "EditorBookmarkColor": EditorBookmarkColor = ParseColor(value, EditorBookmarkColor); break;
            case "EditorServiceLineColor": EditorServiceLineColor = ParseColor(value, EditorServiceLineColor); break;
            case "EditorFoldingHighlightColor": EditorFoldingHighlightColor = ParseColor(value, EditorFoldingHighlightColor); break;
            case "ErrorIndicatorColor": ErrorIndicatorColor = ParseColor(value, ErrorIndicatorColor); break;

            // Unknown / future keys are ignored (forward-compatible).
         }
      }

      /// <summary>
      /// Parses an HTML/named color string, returning <paramref name="currentValue"/> when the value is empty or invalid.
      /// </summary>
      /// <param name="value">The serialized color value.</param>
      /// <param name="currentValue">The value to keep when parsing fails.</param>
      /// <returns>The parsed color, or <paramref name="currentValue"/>.</returns>
      private static Color ParseColor(string value, Color currentValue)
      {
         if (string.IsNullOrEmpty(value))
            return currentValue;

         try
         {
            return ColorTranslator.FromHtml(value);
         }
         catch (Exception)
         {
            return currentValue;
         }
      }

      /// <summary>
      /// Creates a deep copy of this <see cref="Settings"/> instance.
      /// </summary>
      /// <returns>A new <see cref="Settings"/> instance with every property copied from this one.</returns>
      /// <remarks>
      /// All <see cref="Color"/>, <see cref="FontStyle"/>, <see cref="FontFamily"/> and value-type
      /// properties are value semantics (or immutable for our purposes), so a shallow member copy
      /// produces a fully independent working copy. The singleton <see cref="Instance"/> is untouched.
      /// </remarks>
      public Settings Clone()
      {
         var clone = new Settings();

         // Syntax highlighting colors and styles
         clone.DefaultWordColor = DefaultWordColor;
         clone.DefaultWordBackgroundColor = DefaultWordBackgroundColor;
         clone.DefaultWordFontStyle = DefaultWordFontStyle;
         clone.KeywordColor = KeywordColor;
         clone.KeywordBackgroundColor = KeywordBackgroundColor;
         clone.KeywordFontStyle = KeywordFontStyle;
         clone.CommentColor = CommentColor;
         clone.CommentBackgroundColor = CommentBackgroundColor;
         clone.CommentFontStyle = CommentFontStyle;
         clone.LiteralColor = LiteralColor;
         clone.LiteralBackgroundColor = LiteralBackgroundColor;
         clone.LiteralFontStyle = LiteralFontStyle;
         clone.StringColor = StringColor;
         clone.StringBackgroundColor = StringBackgroundColor;
         clone.StringFontStyle = StringFontStyle;
         clone.SymbolColor = SymbolColor;
         clone.SymbolBackgroundColor = SymbolBackgroundColor;
         clone.SymbolFontStyle = SymbolFontStyle;
         clone.OperatorColor = OperatorColor;
         clone.OperatorBackgroundColor = OperatorBackgroundColor;
         clone.OperatorFontStyle = OperatorFontStyle;
         clone.ParenthesisColor = ParenthesisColor;
         clone.ParenthesisBackgroundColor = ParenthesisBackgroundColor;
         clone.ParenthesisFontStyle = ParenthesisFontStyle;
         clone.BracketColor = BracketColor;
         clone.BracketBackgroundColor = BracketBackgroundColor;
         clone.BracketFontStyle = BracketFontStyle;
         clone.CurlyBraceColor = CurlyBraceColor;
         clone.CurlyBraceBackgroundColor = CurlyBraceBackgroundColor;
         clone.CurlyBraceFontStyle = CurlyBraceFontStyle;
         clone.ObjectColor = ObjectColor;
         clone.ObjectBackgroundColor = ObjectBackgroundColor;
         clone.ObjectFontStyle = ObjectFontStyle;
         clone.CoreReferenceColor = CoreReferenceColor;
         clone.CoreReferenceBackgroundColor = CoreReferenceBackgroundColor;
         clone.CoreReferenceFontStyle = CoreReferenceFontStyle;
         clone.BuiltinVariableColor = BuiltinVariableColor;
         clone.BuiltinVariableBackgroundColor = BuiltinVariableBackgroundColor;
         clone.BuiltinVariableFontStyle = BuiltinVariableFontStyle;
         clone.BuiltinFunctionColor = BuiltinFunctionColor;
         clone.BuiltinFunctionBackgroundColor = BuiltinFunctionBackgroundColor;
         clone.BuiltinFunctionFontStyle = BuiltinFunctionFontStyle;
         clone.VerbColor = VerbColor;
         clone.VerbBackgroundColor = VerbBackgroundColor;
         clone.VerbFontStyle = VerbFontStyle;
         clone.PropertyColor = PropertyColor;
         clone.PropertyBackgroundColor = PropertyBackgroundColor;
         clone.PropertyFontStyle = PropertyFontStyle;

         // Editor fonts
         clone.EditorFontFamily = EditorFontFamily;
         clone.EditorFontSize = EditorFontSize;

         // Editor chrome colors
         clone.EditorTextColor = EditorTextColor;
         clone.EditorBackgroundColor = EditorBackgroundColor;
         clone.EditorCaretColor = EditorCaretColor;
         clone.EditorLineNumberColor = EditorLineNumberColor;
         clone.EditorCurrentLineColor = EditorCurrentLineColor;
         clone.EditorTextSelectionColor = EditorTextSelectionColor;
         clone.EditorFoldingIndicatorColor = EditorFoldingIndicatorColor;
         clone.EditorChangedLineColor = EditorChangedLineColor;
         clone.EditorIndentBackColor = EditorIndentBackColor;
         clone.EditorBookmarkColor = EditorBookmarkColor;
         clone.EditorServiceLineColor = EditorServiceLineColor;
         clone.EditorFoldingHighlightColor = EditorFoldingHighlightColor;
         clone.ErrorIndicatorColor = ErrorIndicatorColor;

         // Editor behavior
         clone.EditorShowFoldingBlockHighlights = EditorShowFoldingBlockHighlights;
         clone.EditorZoomFactor = EditorZoomFactor;
         clone.EditorWordWrap = EditorWordWrap;
         clone.EditorWordWrapAutoIndent = EditorWordWrapAutoIndent;
         clone.EditorWordWrapIndent = EditorWordWrapIndent;
         clone.EditorAutoIndent = EditorAutoIndent;
         clone.EditorDarkTheme = EditorDarkTheme;
         clone.EditorShowCodeFolding = EditorShowCodeFolding;
         clone.EditorShowTextIndentGuides = EditorShowTextIndentGuides;
         clone.EditorAutoBrackets = EditorAutoBrackets;
         clone.EditorTabLength = EditorTabLength;
         clone.EditorAutocompleteDelay = EditorAutocompleteDelay;
         clone.EditorCacheTtlSeconds = EditorCacheTtlSeconds;

         // Parser message font
         clone.ParserMessageFontFamily = ParserMessageFontFamily;
         clone.ParserMessageFontSize = ParserMessageFontSize;

         // Dialect
         clone.DefaultGrammarDialect = DefaultGrammarDialect;

         return clone;
      }

      /// <summary>
      /// Copies every property value from the supplied <paramref name="source"/> into this instance.
      /// </summary>
      /// <param name="source">The settings to copy from.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
      public void CopyFrom([NotNull] Settings source)
      {
         if (source == null)
            throw new ArgumentNullException(nameof(source));

         var clone = source.Clone();

         // Syntax highlighting colors and styles
         DefaultWordColor = clone.DefaultWordColor;
         DefaultWordBackgroundColor = clone.DefaultWordBackgroundColor;
         DefaultWordFontStyle = clone.DefaultWordFontStyle;
         KeywordColor = clone.KeywordColor;
         KeywordBackgroundColor = clone.KeywordBackgroundColor;
         KeywordFontStyle = clone.KeywordFontStyle;
         CommentColor = clone.CommentColor;
         CommentBackgroundColor = clone.CommentBackgroundColor;
         CommentFontStyle = clone.CommentFontStyle;
         LiteralColor = clone.LiteralColor;
         LiteralBackgroundColor = clone.LiteralBackgroundColor;
         LiteralFontStyle = clone.LiteralFontStyle;
         StringColor = clone.StringColor;
         StringBackgroundColor = clone.StringBackgroundColor;
         StringFontStyle = clone.StringFontStyle;
         SymbolColor = clone.SymbolColor;
         SymbolBackgroundColor = clone.SymbolBackgroundColor;
         SymbolFontStyle = clone.SymbolFontStyle;
         OperatorColor = clone.OperatorColor;
         OperatorBackgroundColor = clone.OperatorBackgroundColor;
         OperatorFontStyle = clone.OperatorFontStyle;
         ParenthesisColor = clone.ParenthesisColor;
         ParenthesisBackgroundColor = clone.ParenthesisBackgroundColor;
         ParenthesisFontStyle = clone.ParenthesisFontStyle;
         BracketColor = clone.BracketColor;
         BracketBackgroundColor = clone.BracketBackgroundColor;
         BracketFontStyle = clone.BracketFontStyle;
         CurlyBraceColor = clone.CurlyBraceColor;
         CurlyBraceBackgroundColor = clone.CurlyBraceBackgroundColor;
         CurlyBraceFontStyle = clone.CurlyBraceFontStyle;
         ObjectColor = clone.ObjectColor;
         ObjectBackgroundColor = clone.ObjectBackgroundColor;
         ObjectFontStyle = clone.ObjectFontStyle;
         CoreReferenceColor = clone.CoreReferenceColor;
         CoreReferenceBackgroundColor = clone.CoreReferenceBackgroundColor;
         CoreReferenceFontStyle = clone.CoreReferenceFontStyle;
         BuiltinVariableColor = clone.BuiltinVariableColor;
         BuiltinVariableBackgroundColor = clone.BuiltinVariableBackgroundColor;
         BuiltinVariableFontStyle = clone.BuiltinVariableFontStyle;
         BuiltinFunctionColor = clone.BuiltinFunctionColor;
         BuiltinFunctionBackgroundColor = clone.BuiltinFunctionBackgroundColor;
         BuiltinFunctionFontStyle = clone.BuiltinFunctionFontStyle;
         VerbColor = clone.VerbColor;
         VerbBackgroundColor = clone.VerbBackgroundColor;
         VerbFontStyle = clone.VerbFontStyle;
         PropertyColor = clone.PropertyColor;
         PropertyBackgroundColor = clone.PropertyBackgroundColor;
         PropertyFontStyle = clone.PropertyFontStyle;

         // Editor fonts
         EditorFontFamily = clone.EditorFontFamily;
         EditorFontSize = clone.EditorFontSize;

         // Editor chrome colors
         EditorTextColor = clone.EditorTextColor;
         EditorBackgroundColor = clone.EditorBackgroundColor;
         EditorCaretColor = clone.EditorCaretColor;
         EditorLineNumberColor = clone.EditorLineNumberColor;
         EditorCurrentLineColor = clone.EditorCurrentLineColor;
         EditorTextSelectionColor = clone.EditorTextSelectionColor;
         EditorFoldingIndicatorColor = clone.EditorFoldingIndicatorColor;
         EditorChangedLineColor = clone.EditorChangedLineColor;
         EditorIndentBackColor = clone.EditorIndentBackColor;
         EditorBookmarkColor = clone.EditorBookmarkColor;
         EditorServiceLineColor = clone.EditorServiceLineColor;
         EditorFoldingHighlightColor = clone.EditorFoldingHighlightColor;
         ErrorIndicatorColor = clone.ErrorIndicatorColor;

         // Editor behavior
         EditorShowFoldingBlockHighlights = clone.EditorShowFoldingBlockHighlights;
         EditorZoomFactor = clone.EditorZoomFactor;
         EditorWordWrap = clone.EditorWordWrap;
         EditorWordWrapAutoIndent = clone.EditorWordWrapAutoIndent;
         EditorWordWrapIndent = clone.EditorWordWrapIndent;
         EditorAutoIndent = clone.EditorAutoIndent;
         EditorDarkTheme = clone.EditorDarkTheme;
         EditorShowCodeFolding = clone.EditorShowCodeFolding;
         EditorShowTextIndentGuides = clone.EditorShowTextIndentGuides;
         EditorAutoBrackets = clone.EditorAutoBrackets;
         EditorTabLength = clone.EditorTabLength;
         EditorAutocompleteDelay = clone.EditorAutocompleteDelay;
         EditorCacheTtlSeconds = clone.EditorCacheTtlSeconds;

         // Parser message font
         ParserMessageFontFamily = clone.ParserMessageFontFamily;
         ParserMessageFontSize = clone.ParserMessageFontSize;

         // Dialect
         DefaultGrammarDialect = clone.DefaultGrammarDialect;
      }

      /// <summary>
      /// Serializes a <see cref="Color"/> into the form the loader accepts (named color or hex).
      /// </summary>
      /// <param name="color">The color.</param>
      /// <returns>A string the loader's <see cref="ColorTranslator.FromHtml"/> path can parse.</returns>
      /// <remarks>
      /// <see cref="ColorTranslator.ToHtml"/> returns a named color when possible (e.g. "Transparent",
      /// "Red") and a "#RRGGBB" hex string otherwise. <see cref="Color.Transparent"/> round-trips
      /// because it is a known named color.
      /// </remarks>
      private static string SerializeColor(Color color)
      {
         var html = ColorTranslator.ToHtml(color);
         if (!string.IsNullOrEmpty(html))
            return html;

         // Fall back to an explicit hex string for any color ToHtml could not name.
         return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
      }

      /// <summary>
      /// Serializes a <see cref="FontStyle"/> into the ';'-joined form <see cref="ParseFontStyles"/> reads.
      /// </summary>
      /// <param name="style">The font style.</param>
      /// <returns>A string such as "Regular" or "Bold;Italic".</returns>
      private static string SerializeFontStyle(FontStyle style)
      {
         if (style == FontStyle.Regular)
            return nameof(FontStyle.Regular);

         var parts = new List<string>();
         if (style.HasFlag(FontStyle.Bold))
            parts.Add(nameof(FontStyle.Bold));
         if (style.HasFlag(FontStyle.Italic))
            parts.Add(nameof(FontStyle.Italic));
         if (style.HasFlag(FontStyle.Underline))
            parts.Add(nameof(FontStyle.Underline));
         if (style.HasFlag(FontStyle.Strikeout))
            parts.Add(nameof(FontStyle.Strikeout));

         return parts.Count == 0 ? nameof(FontStyle.Regular) : string.Join(";", parts);
      }

      /// <summary>
      /// Saves all editor settings to the specified file as an appSettings XML document.
      /// </summary>
      /// <param name="filePath">The destination file path.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/> or empty.</exception>
      /// <exception cref="T:System.IO.IOException">The file could not be written.</exception>
      /// <remarks>
      /// The written keys, color formats and font-style formats exactly match what <see cref="LoadFrom(string)"/>
      /// reads, so a subsequent load round-trips every value.
      /// </remarks>
      public void SaveTo([NotNull] string filePath)
      {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

         var settings = new Dictionary<string, string>
         {
            // Syntax highlighting
            ["DefaultWordColor"] = SerializeColor(DefaultWordColor),
            ["DefaultWordBackgroundColor"] = SerializeColor(DefaultWordBackgroundColor),
            ["DefaultWordFontStyle"] = SerializeFontStyle(DefaultWordFontStyle),
            ["CommentColor"] = SerializeColor(CommentColor),
            ["CommentBackgroundColor"] = SerializeColor(CommentBackgroundColor),
            ["CommentFontStyle"] = SerializeFontStyle(CommentFontStyle),
            ["KeywordColor"] = SerializeColor(KeywordColor),
            ["KeywordBackgroundColor"] = SerializeColor(KeywordBackgroundColor),
            ["KeywordFontStyle"] = SerializeFontStyle(KeywordFontStyle),
            ["LiteralColor"] = SerializeColor(LiteralColor),
            ["LiteralBackgroundColor"] = SerializeColor(LiteralBackgroundColor),
            ["LiteralFontStyle"] = SerializeFontStyle(LiteralFontStyle),
            ["StringColor"] = SerializeColor(StringColor),
            ["StringBackgroundColor"] = SerializeColor(StringBackgroundColor),
            ["StringFontStyle"] = SerializeFontStyle(StringFontStyle),
            ["SymbolColor"] = SerializeColor(SymbolColor),
            ["SymbolBackgroundColor"] = SerializeColor(SymbolBackgroundColor),
            ["SymbolFontStyle"] = SerializeFontStyle(SymbolFontStyle),
            ["OperatorColor"] = SerializeColor(OperatorColor),
            ["OperatorBackgroundColor"] = SerializeColor(OperatorBackgroundColor),
            ["OperatorFontStyle"] = SerializeFontStyle(OperatorFontStyle),
            ["ParenthesisColor"] = SerializeColor(ParenthesisColor),
            ["ParenthesisBackgroundColor"] = SerializeColor(ParenthesisBackgroundColor),
            ["ParenthesisFontStyle"] = SerializeFontStyle(ParenthesisFontStyle),
            ["CurlyBraceColor"] = SerializeColor(CurlyBraceColor),
            ["CurlyBraceBackgroundColor"] = SerializeColor(CurlyBraceBackgroundColor),
            ["CurlyBraceFontStyle"] = SerializeFontStyle(CurlyBraceFontStyle),
            ["BracketColor"] = SerializeColor(BracketColor),
            ["BracketBackgroundColor"] = SerializeColor(BracketBackgroundColor),
            ["BracketFontStyle"] = SerializeFontStyle(BracketFontStyle),
            ["ObjectColor"] = SerializeColor(ObjectColor),
            ["ObjectBackgroundColor"] = SerializeColor(ObjectBackgroundColor),
            ["ObjectFontStyle"] = SerializeFontStyle(ObjectFontStyle),
            ["CoreReferenceColor"] = SerializeColor(CoreReferenceColor),
            ["CoreReferenceBackgroundColor"] = SerializeColor(CoreReferenceBackgroundColor),
            ["CoreReferenceFontStyle"] = SerializeFontStyle(CoreReferenceFontStyle),
            ["BuiltinVariableColor"] = SerializeColor(BuiltinVariableColor),
            ["BuiltinVariableBackgroundColor"] = SerializeColor(BuiltinVariableBackgroundColor),
            ["BuiltinVariableFontStyle"] = SerializeFontStyle(BuiltinVariableFontStyle),
            ["BuiltinFunctionColor"] = SerializeColor(BuiltinFunctionColor),
            ["BuiltinFunctionBackgroundColor"] = SerializeColor(BuiltinFunctionBackgroundColor),
            ["BuiltinFunctionFontStyle"] = SerializeFontStyle(BuiltinFunctionFontStyle),
            ["VerbColor"] = SerializeColor(VerbColor),
            ["VerbBackgroundColor"] = SerializeColor(VerbBackgroundColor),
            ["VerbFontStyle"] = SerializeFontStyle(VerbFontStyle),
            ["PropertyColor"] = SerializeColor(PropertyColor),
            ["PropertyBackgroundColor"] = SerializeColor(PropertyBackgroundColor),
            ["PropertyFontStyle"] = SerializeFontStyle(PropertyFontStyle),

            // Editor fonts
            ["EditorFontFamily"] = EditorFontFamily?.Name ?? string.Empty,
            ["EditorFontSize"] = EditorFontSize.ToString(System.Globalization.CultureInfo.InvariantCulture),

            // Editor chrome colors
            ["EditorTextColor"] = SerializeColor(EditorTextColor),
            ["EditorBackgroundColor"] = SerializeColor(EditorBackgroundColor),
            ["EditorCaretColor"] = SerializeColor(EditorCaretColor),
            ["EditorLineNumberColor"] = SerializeColor(EditorLineNumberColor),
            ["EditorCurrentLineColor"] = SerializeColor(EditorCurrentLineColor),
            ["EditorTextSelectionColor"] = SerializeColor(EditorTextSelectionColor),
            ["EditorFoldingIndicatorColor"] = SerializeColor(EditorFoldingIndicatorColor),
            ["EditorChangedLineColor"] = SerializeColor(EditorChangedLineColor),
            ["EditorIndentBackColor"] = SerializeColor(EditorIndentBackColor),
            ["EditorBookmarkColor"] = SerializeColor(EditorBookmarkColor),
            ["EditorServiceLineColor"] = SerializeColor(EditorServiceLineColor),
            ["EditorFoldingHighlightColor"] = SerializeColor(EditorFoldingHighlightColor),
            ["ErrorIndicatorColor"] = SerializeColor(ErrorIndicatorColor),

            // Editor behavior
            ["EditorShowCodeFolding"] = EditorShowCodeFolding.ToString(),
            ["EditorShowFoldingBlockHighlights"] = EditorShowFoldingBlockHighlights.ToString(),
            ["EditorShowTextIndentGuides"] = EditorShowTextIndentGuides.ToString(),
            ["EditorWordWrap"] = EditorWordWrap.ToString(),
            ["EditorWordWrapAutoIndent"] = EditorWordWrapAutoIndent.ToString(),
            ["EditorWordWrapIndent"] = EditorWordWrapIndent.ToString(),
            ["EditorZoomFactor"] = EditorZoomFactor.ToString(),
            ["EditorAutoIndent"] = EditorAutoIndent.ToString(),
            ["EditorAutoBrackets"] = EditorAutoBrackets.ToString(),
            ["EditorTabLength"] = EditorTabLength.ToString(),
            ["EditorAutocompleteDelay"] = EditorAutocompleteDelay.ToString(),
            ["EditorCacheTtlSeconds"] = EditorCacheTtlSeconds.ToString(),
            ["EditorDarkTheme"] = EditorDarkTheme.ToString(),

            // Parser message font
            ["ParserMessageFontFamily"] = ParserMessageFontFamily?.Name ?? string.Empty,
            ["ParserMessageFontSize"] = ParserMessageFontSize.ToString(System.Globalization.CultureInfo.InvariantCulture),

            // Dialect
            ["DefaultGrammarDialect"] = DefaultGrammarDialect.ToString()
         };

         try
         {
            var xmlSettings = new XmlWriterSettings
            {
               Indent = true,
               IndentChars = "   ",
               Encoding = new UTF8Encoding(false)
            };

            // Ensure the destination directory exists.
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
               Directory.CreateDirectory(directory);

            using var writer = XmlWriter.Create(filePath, xmlSettings);
            writer.WriteStartDocument();
            writer.WriteStartElement("configuration");
            writer.WriteStartElement("appSettings");
            foreach (var pair in settings)
            {
               writer.WriteStartElement("add");
               writer.WriteAttributeString("key", pair.Key);
               writer.WriteAttributeString("value", pair.Value);
               writer.WriteEndElement();
            }

            writer.WriteEndElement(); // appSettings
            writer.WriteEndElement(); // configuration
            writer.WriteEndDocument();
         }
         catch (Exception ex)
         {
            throw new IOException($"Failed to save editor settings to '{filePath}': {ex.Message}", ex);
         }
      }

      /// <summary>
      /// Loads the editor settings from the specified file.
      /// </summary>
      /// <param name="filePath">The file path.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/> or empty.</exception>
      /// <exception cref="T:System.Configuration.ConfigurationErrorsException">A configuration file could not be loaded.</exception>
      public void LoadFrom([NotNull] string filePath)
      {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

         if (File.Exists(filePath))
         {
            try
            {
               var configMap = new ExeConfigurationFileMap { ExeConfigFilename = filePath };
               var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
               var appSettings = config.AppSettings.Settings;
               LoadFrom(appSettings);
            }
            catch (ConfigurationErrorsException)
            {
               LoadDefaults();
               throw;
            }
         }
         else
            LoadFrom(null as KeyValueConfigurationCollection);
      }

      /// <summary>
      /// Loads the editor settings from the supplied application settings.
      /// </summary>
      /// <param name="appSettings">The application settings.</param>
      public void LoadFrom(KeyValueConfigurationCollection appSettings)
      {
         LoadSyntaxHighlightingSettings(appSettings);
         LoadEditorSettings(appSettings);
         LoadParserMessageFontSettings(appSettings);
      }

      /// <summary>
      /// Loads the editor settings with standard default values.
      /// </summary>
      public void LoadDefaults()
      {
         LoadSyntaxHighlightingSettings(null);
         LoadEditorSettings(null);
         LoadParserMessageFontSettings(null);
      }

      private void LoadEditorSettings(KeyValueConfigurationCollection appSettings)
      {
         LoadEditorFontSettings(appSettings);
         LoadEditorColorSettings(appSettings);
         LoadEditorBehaviorSettings(appSettings);
      }

      private void LoadEditorFontSettings(KeyValueConfigurationCollection appSettings)
      {
         float defFontSize = 8f;
         FontFamily defFontFamily = FontFamily.GenericMonospace;

         if (appSettings == null)
         {
            EditorFontFamily = FontFamily.GenericMonospace;
            EditorFontSize = defFontSize;
            return;
         }

         // Fetch EditorFontFamily setting
         var result = appSettings["EditorFontFamily"]?.Value ?? string.Empty;
         try
         {
            EditorFontFamily = !string.IsNullOrEmpty(result) ? new FontFamily(result) : defFontFamily;
         }
         catch (ArgumentException)
         {
            EditorFontFamily = defFontFamily;
         }

         // Fetch EditorFontSize setting
         result = appSettings["EditorFontSize"]?.Value ?? string.Empty;
         EditorFontSize = !float.TryParse(result, out var settingValueFloat) ? defFontSize : settingValueFloat;
      }

      private void LoadEditorColorSettings(KeyValueConfigurationCollection appSettings)
      {
         var defTextColor = Color.Black;
         var defCaretColor = Color.Black;
         var defLineNumberColor = Color.Teal;
         var defBackgroundColor = Color.White;
         var defCurrentLineColor = Color.Transparent;
         var defErrorIndicatorColor = Color.Red;
         var defTextSelectionColor = Color.MediumPurple;
         var defFoldingIndicatorColor = Color.Green;
         var defChangedLineColor = Color.Yellow;
         var defIndentBackColor = Color.WhiteSmoke;
         var defBookmarkColor = Color.PowderBlue;
         var defServiceLineColor = Color.Silver;
         var defFoldingBlockHighlightColor = Color.AliceBlue;

         if (appSettings == null)
         {
            EditorTextColor = defTextColor;
            EditorCaretColor = defCaretColor;
            EditorBackgroundColor = defBackgroundColor;
            EditorCurrentLineColor = defCurrentLineColor;
            EditorLineNumberColor = defLineNumberColor;
            ErrorIndicatorColor = defErrorIndicatorColor;
            EditorTextSelectionColor = defTextSelectionColor;
            EditorFoldingIndicatorColor = defFoldingIndicatorColor;
            EditorChangedLineColor = defChangedLineColor;
            EditorIndentBackColor = defIndentBackColor;
            EditorBookmarkColor = defBookmarkColor;
            EditorServiceLineColor = defServiceLineColor;
            EditorFoldingHighlightColor = defFoldingBlockHighlightColor;
            return;
         }

         // Fetch EditorTextColor setting
         var result = appSettings["EditorTextColor"]?.Value ?? string.Empty;
         try
         {
            EditorTextColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defTextColor;
         }
         catch (Exception)
         {
            EditorTextColor = defTextColor;
         }

         // Fetch EditorErrorIndicatorColor setting
         result = appSettings["ErrorIndicatorColor"]?.Value ?? string.Empty;
         try
         {
            ErrorIndicatorColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defErrorIndicatorColor;
         }
         catch (Exception)
         {
            ErrorIndicatorColor = defErrorIndicatorColor;
         }

         // Fetch EditorCaretColor setting
         result = appSettings["EditorCaretColor"]?.Value ?? string.Empty;
         try
         {
            EditorCaretColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defCaretColor;
         }
         catch (Exception)
         {
            EditorCaretColor = defCaretColor;
         }

         // Fetch EditorBackgroundColor setting
         result = appSettings["EditorBackgroundColor"]?.Value ?? string.Empty;
         try
         {
            EditorBackgroundColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackgroundColor;
         }
         catch (Exception)
         {
            EditorBackgroundColor = defBackgroundColor;
         }

         // Fetch EditorCurrentLineColor setting
         result = appSettings["EditorCurrentLineColor"]?.Value ?? string.Empty;
         try
         {
            EditorCurrentLineColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defCurrentLineColor;
         }
         catch (Exception)
         {
            EditorCurrentLineColor = defCurrentLineColor;
         }

         // Fetch EditorLineNumberColor setting
         result = appSettings["EditorLineNumberColor"]?.Value ?? string.Empty;
         try
         {
            EditorLineNumberColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defLineNumberColor;
         }
         catch (Exception)
         {
            EditorLineNumberColor = defLineNumberColor;
         }

         // Fetch EditorTextSelectionColor setting
         result = appSettings["EditorTextSelectionColor"]?.Value ?? string.Empty;
         try
         {
            EditorTextSelectionColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defTextSelectionColor;
         }
         catch (Exception)
         {
            EditorTextSelectionColor = defTextSelectionColor;
         }

         // Fetch FoldingIndicatorColor setting
         result = appSettings["EditorFoldingIndicatorColor"]?.Value ?? string.Empty;
         try
         {
            EditorFoldingIndicatorColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defFoldingIndicatorColor;
         }
         catch (Exception)
         {
            EditorFoldingIndicatorColor = defFoldingIndicatorColor;
         }

         // Fetch ChangedLineColor setting
         result = appSettings["EditorChangedLineColor"]?.Value ?? string.Empty;
         try
         {
            EditorChangedLineColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defChangedLineColor;
         }
         catch (Exception)
         {
            EditorChangedLineColor = defChangedLineColor;
         }

         // Fetch IndentBackColor setting
         result = appSettings["EditorIndentBackColor"]?.Value ?? string.Empty;
         try
         {
            EditorIndentBackColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defIndentBackColor;
         }
         catch (Exception)
         {
            EditorIndentBackColor = defIndentBackColor;
         }

         // Fetch BookmarkColor setting
         result = appSettings["EditorBookmarkColor"]?.Value ?? string.Empty;
         try
         {
            EditorBookmarkColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBookmarkColor;
         }
         catch (Exception)
         {
            EditorBookmarkColor = defBookmarkColor;
         }

         // Fetch ServiceLineColor setting
         result = appSettings["EditorServiceLineColor"]?.Value ?? string.Empty;
         try
         {
            EditorServiceLineColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defServiceLineColor;
         }
         catch (Exception)
         {
            EditorServiceLineColor = defServiceLineColor;
         }

         // Fetch FoldingHighlightColor setting
         result = appSettings["EditorFoldingHighlightColor"]?.Value ?? string.Empty;
         try
         {
            EditorFoldingHighlightColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defFoldingBlockHighlightColor;
         }
         catch (Exception)
         {
            EditorFoldingHighlightColor = defFoldingBlockHighlightColor;
         }
      }

      [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse", Justification = "better to centralize defaults")]
      private void LoadEditorBehaviorSettings(KeyValueConfigurationCollection appSettings)
      {
         var defAutoIndent = true;
         var defAutoComplete = true;
         var defWordWrap = true;
         var defWordWrapAutoIndent = true;
         var defWordWrapIndent = 2;
         var defTabLength = 2;
         var defAutocompleteDelay = 50;
         var defCacheTtlSeconds = 60;
         var defGrammarDialect = GrammarDialect.Edgerunner;
         var defCodeFolding = true;
         var defShowFoldingBlockHighlights = true;
         var defIndentationGuides = true;
         var defEditorZoomFactor = 0;
         var defDarkTheme = false;

         if (appSettings == null)
         {
            EditorAutoIndent = defAutoIndent;
            EditorWordWrap = defWordWrap;
            EditorAutoBrackets = defAutoComplete;
            EditorWordWrapIndent = defWordWrapIndent;
            EditorWordWrapAutoIndent = defWordWrapAutoIndent;
            EditorTabLength = defTabLength;
            EditorAutocompleteDelay = defAutocompleteDelay;
            EditorCacheTtlSeconds = defCacheTtlSeconds;
            DefaultGrammarDialect = defGrammarDialect;
            EditorShowTextIndentGuides = defIndentationGuides;
            EditorShowCodeFolding = defCodeFolding;
            EditorShowFoldingBlockHighlights = defShowFoldingBlockHighlights;
            EditorZoomFactor = defEditorZoomFactor;
            EditorDarkTheme = defDarkTheme;
            return;
         }

         // Fetch EditorAutoIndent settings
         var result = appSettings["EditorAutoIndent"]?.Value ?? string.Empty;
         EditorAutoIndent = !bool.TryParse(result, out var settingValueBoolean) ? defAutoIndent : settingValueBoolean;

         // Fetch EditorWordWrap settings
         result = appSettings["EditorWordWrap"]?.Value ?? string.Empty;
         EditorWordWrap = !bool.TryParse(result, out settingValueBoolean) ? defWordWrap : settingValueBoolean;

         // Fetch EditorWordWrap settings
         result = appSettings["EditorWordWrapAutoIndent"]?.Value ?? string.Empty;
         EditorWordWrapAutoIndent = !bool.TryParse(result, out settingValueBoolean) ? defWordWrapAutoIndent : settingValueBoolean;

         // Fetch EditorWordWrapIndent setting
         result = appSettings["EditorWordWrapIndent"]?.Value ?? string.Empty;
         EditorWordWrapIndent = !int.TryParse(result, out var settingValueInt) ? defWordWrapIndent : settingValueInt;

         // Fetch EditorShowTextIndentGuides setting
         result = appSettings["EditorShowTextIndentGuides"]?.Value ?? string.Empty;
         EditorShowTextIndentGuides = !bool.TryParse(result, out settingValueBoolean) ? defIndentationGuides : settingValueBoolean;

         // Fetch EditorShowCodeFolding setting
         result = appSettings["EditorShowCodeFolding"]?.Value ?? string.Empty;
         EditorShowCodeFolding = !bool.TryParse(result, out settingValueBoolean) ? defCodeFolding : settingValueBoolean;

         // Fetch EditorShowCodeFolding setting
         result = appSettings["EditorShowFoldingBlockHighlights"]?.Value ?? string.Empty;
         EditorShowFoldingBlockHighlights = !bool.TryParse(result, out settingValueBoolean) ? defShowFoldingBlockHighlights : settingValueBoolean;

         // Fetch EditorAutoBrackets settings
         result = appSettings["EditorAutoBrackets"]?.Value ?? string.Empty;
         EditorAutoBrackets = !bool.TryParse(result, out settingValueBoolean) ? defAutoComplete : settingValueBoolean;

         // Fetch EditorDarkTheme settings
         result = appSettings["EditorDarkTheme"]?.Value ?? string.Empty;
         EditorDarkTheme = !bool.TryParse(result, out settingValueBoolean) ? defDarkTheme : settingValueBoolean;

         // Fetch EditorWordWrapIndent setting
         result = appSettings["EditorTabLength"]?.Value ?? string.Empty;
         EditorTabLength = !int.TryParse(result, out settingValueInt) ? defTabLength : (settingValueInt > 0) ? settingValueInt : defTabLength;

         // Fetch EditorAutocompleteDelay setting
         result = appSettings["EditorAutocompleteDelay"]?.Value ?? string.Empty;
         EditorAutocompleteDelay = !int.TryParse(result, out settingValueInt) ? defAutocompleteDelay : settingValueInt;

         // Fetch EditorCacheTtlSeconds setting
         result = appSettings["EditorCacheTtlSeconds"]?.Value ?? string.Empty;
         EditorCacheTtlSeconds = !int.TryParse(result, out settingValueInt) ? defCacheTtlSeconds : settingValueInt;

         // Fetch EditorZoomFactor setting
         result = appSettings["EditorZoomFactor"]?.Value ?? string.Empty;
         EditorZoomFactor = !int.TryParse(result, out settingValueInt) ? defEditorZoomFactor : settingValueInt;

         // Fetch DefaultGrammarDialect setting
         result = appSettings["DefaultGrammarDialect"]?.Value ?? string.Empty;
         DefaultGrammarDialect = !Enum.TryParse(typeof(GrammarDialect), result, out var settingValueObj) ? defGrammarDialect : (GrammarDialect)settingValueObj;
      }

      private void LoadParserMessageFontSettings(KeyValueConfigurationCollection appSettings)
      {
         var defFontFamily = FontFamily.GenericSerif;
         var defFontSize = 8.25f;

         if (appSettings == null)
         {
            ParserMessageFontFamily = FontFamily.GenericMonospace;
            ParserMessageFontSize = defFontSize;
            return;
         }

         // Fetch EditorFontFamily setting
         var result = appSettings["ParserMessageFontFamily"]?.Value ?? string.Empty;
         try
         {
            ParserMessageFontFamily = !string.IsNullOrEmpty(result) ? new FontFamily(result) : defFontFamily;
         }
         catch (ArgumentException)
         {
            ParserMessageFontFamily = defFontFamily;
         }

         // Fetch EditorFontSize setting
         result = appSettings["ParserMessageFontSize"]?.Value ?? string.Empty;
         ParserMessageFontSize = !float.TryParse(result, out var settingValueFloat) ? defFontSize : settingValueFloat;
      }

      private void LoadSyntaxHighlightingSettings(KeyValueConfigurationCollection appSettings)
      {
         LoadDefaultWordHighlightSettings(appSettings);
         LoadKeywordHighlightSettings(appSettings);
         LoadLiteralHighlightSettings(appSettings);
         LoadStringHighlightSettings(appSettings);
         LoadSymbolHighlightSettings(appSettings);
         LoadOperatorHighlightSettings(appSettings);
         LoadParenthesisHighlightSettings(appSettings);
         LoadBracketHighlightSettings(appSettings);
         LoadCurlyBraceHighlightSettings(appSettings);
         LoadObjectHighlightSettings(appSettings);
         LoadCoreReferenceHighlightSettings(appSettings);
         LoadBuiltinVariableHighlightSettings(appSettings);
         LoadBuiltinFunctionHighlightSettings(appSettings);
         LoadPropertyHighlightSettings(appSettings);
         LoadVerbHighlightSettings(appSettings);
         LoadCommentTokenHighlightSettings(appSettings);
      }

      private void LoadCommentTokenHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
         var defColor = Color.Green;
         var defBackColor = Color.Transparent;
         var defStyle = FontStyle.Italic;

         if (appSettings == null)
         {
            CommentColor = defColor;
            CommentBackgroundColor = defBackColor;
            CommentFontStyle = defStyle;
            return;
         }

         // Fetch CommentColor setting
         var result = appSettings["CommentColor"]?.Value ?? string.Empty;
         try
         {
            CommentColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
         }
         catch (Exception)
         {
            CommentColor = defColor;
         }

         // Fetch CommentBackgroundColor setting
         result = appSettings["CommentBackgroundColor"]?.Value ?? string.Empty;
         try
         {
            CommentBackgroundColor =
               !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
         }
         catch (Exception)
         {
            CommentBackgroundColor = defBackColor;
         }

         // Fetch CommentFontStyle setting
         result = appSettings["CommentFontStyle"]?.Value ?? string.Empty;
         CommentFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadLiteralHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
         var defColor = Color.BlueViolet;
         var defBackColor = Color.Transparent;
         var defStyle = FontStyle.Regular;

         if (appSettings == null)
         {
            LiteralColor = defColor;
            LiteralBackgroundColor = defBackColor;
            LiteralFontStyle = defStyle;
            return;
         }

         // Fetch LiteralColor setting
         var result = appSettings["LiteralColor"]?.Value ?? string.Empty;
         try
         {
            LiteralColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
         }
         catch (Exception)
         {
            LiteralColor = defColor;
         }

         // Fetch LiteralBackgroundColor setting
         result = appSettings["LiteralBackgroundColor"]?.Value ?? string.Empty;
         try
         {
            LiteralBackgroundColor =
               !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
         }
         catch (Exception)
         {
            LiteralBackgroundColor = defBackColor;
         }

         // Fetch LiteralFontStyle setting
         result = appSettings["LiteralFontStyle"]?.Value ?? string.Empty;
         LiteralFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadStringHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.Red;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              StringColor = defColor;
              StringBackgroundColor = defBackColor;
              StringFontStyle = defStyle;
              return;
          }

          // Fetch StringColor setting
          var result = appSettings["StringColor"]?.Value ?? string.Empty;
          try
          {
              StringColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              StringColor = defColor;
          }

          // Fetch StringBackgroundColor setting
          result = appSettings["StringBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              StringBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              StringBackgroundColor = defBackColor;
          }

          // Fetch StringFontStyle setting
          result = appSettings["StringFontStyle"]?.Value ?? string.Empty;
          StringFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadObjectHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.Goldenrod;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              ObjectColor = defColor;
              ObjectBackgroundColor = defBackColor;
              ObjectFontStyle = defStyle;
              return;
          }

          // Fetch ObjectColor setting
          var result = appSettings["ObjectColor"]?.Value ?? string.Empty;
          try
          {
              ObjectColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              ObjectColor = defColor;
          }

          // Fetch ObjectBackgroundColor setting
          result = appSettings["ObjectBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              ObjectBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              ObjectBackgroundColor = defBackColor;
          }

          // Fetch ObjectFontStyle setting
          result = appSettings["ObjectFontStyle"]?.Value ?? string.Empty;
          ObjectFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadCoreReferenceHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.Goldenrod;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              CoreReferenceColor = defColor;
              CoreReferenceBackgroundColor = defBackColor;
              CoreReferenceFontStyle = defStyle;
              return;
          }

          // Fetch CoreReferenceColor setting
          var result = appSettings["CoreReferenceColor"]?.Value ?? string.Empty;
          try
          {
              CoreReferenceColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              CoreReferenceColor = defColor;
          }

          // Fetch CoreReferenceBackgroundColor setting
          result = appSettings["CoreReferenceBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              CoreReferenceBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              CoreReferenceBackgroundColor = defBackColor;
          }

          // Fetch CoreReferenceFontStyle setting
          result = appSettings["CoreReferenceFontStyle"]?.Value ?? string.Empty;
          CoreReferenceFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadPropertyHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.Black;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              PropertyColor = defColor;
              PropertyBackgroundColor = defBackColor;
              PropertyFontStyle = defStyle;
              return;
          }

          // Fetch PropertyColor setting
          var result = appSettings["PropertyColor"]?.Value ?? string.Empty;
          try
          {
              PropertyColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              PropertyColor = defColor;
          }

          // Fetch PropertyBackgroundColor setting
          result = appSettings["PropertyBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              PropertyBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              PropertyBackgroundColor = defBackColor;
          }

          // Fetch PropertyFontStyle setting
          result = appSettings["PropertyFontStyle"]?.Value ?? string.Empty;
          PropertyFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadVerbHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.Black;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              VerbColor = defColor;
              VerbBackgroundColor = defBackColor;
              VerbFontStyle = defStyle;
              return;
          }

          // Fetch VerbColor setting
          var result = appSettings["VerbColor"]?.Value ?? string.Empty;
          try
          {
              VerbColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              VerbColor = defColor;
          }

          // Fetch VerbBackgroundColor setting
          result = appSettings["VerbBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              VerbBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              VerbBackgroundColor = defBackColor;
          }

          // Fetch VerbFontStyle setting
          result = appSettings["VerbFontStyle"]?.Value ?? string.Empty;
          VerbFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadSymbolHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.DarkCyan;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              SymbolColor = defColor;
              SymbolBackgroundColor = defBackColor;
              SymbolFontStyle = defStyle;
              return;
          }

          // Fetch SymbolColor setting
          var result = appSettings["SymbolColor"]?.Value ?? string.Empty;
          try
          {
              SymbolColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              SymbolColor = defColor;
          }

          // Fetch SymbolBackgroundColor setting
          result = appSettings["SymbolBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              SymbolBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              SymbolBackgroundColor = defBackColor;
          }

          // Fetch SymbolFontStyle setting
          result = appSettings["SymbolFontStyle"]?.Value ?? string.Empty;
          SymbolFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadOperatorHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.DarkCyan;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              OperatorColor = defColor;
              OperatorBackgroundColor = defBackColor;
              OperatorFontStyle = defStyle;
              return;
          }

          // Fetch OperatorColor setting
          var result = appSettings["OperatorColor"]?.Value ?? string.Empty;
          try
          {
              OperatorColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              OperatorColor = defColor;
          }

          // Fetch OperatorBackgroundColor setting
          result = appSettings["OperatorBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              OperatorBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              OperatorBackgroundColor = defBackColor;
          }

          // Fetch OperatorFontStyle setting
          result = appSettings["OperatorFontStyle"]?.Value ?? string.Empty;
          OperatorFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadParenthesisHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.DarkCyan;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              ParenthesisColor = defColor;
              ParenthesisBackgroundColor = defBackColor;
              ParenthesisFontStyle = defStyle;
              return;
          }

          // Fetch ParenthesisColor setting
          var result = appSettings["ParenthesisColor"]?.Value ?? string.Empty;
          try
          {
              ParenthesisColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              ParenthesisColor = defColor;
          }

          // Fetch ParenthesisBackgroundColor setting
          result = appSettings["ParenthesisBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              ParenthesisBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              ParenthesisBackgroundColor = defBackColor;
          }

          // Fetch ParenthesisFontStyle setting
          result = appSettings["ParenthesisFontStyle"]?.Value ?? string.Empty;
          ParenthesisFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadBracketHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.SaddleBrown;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              BracketColor = defColor;
              BracketBackgroundColor = defBackColor;
              BracketFontStyle = defStyle;
              return;
          }

          // Fetch BracketColor setting
          var result = appSettings["BracketColor"]?.Value ?? string.Empty;
          try
          {
              BracketColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              BracketColor = defColor;
          }

          // Fetch BracketBackgroundColor setting
          result = appSettings["BracketBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              BracketBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              BracketBackgroundColor = defBackColor;
          }

          // Fetch BracketFontStyle setting
          result = appSettings["BracketFontStyle"]?.Value ?? string.Empty;
          BracketFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadCurlyBraceHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.SaddleBrown;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              CurlyBraceColor = defColor;
              CurlyBraceBackgroundColor = defBackColor;
              CurlyBraceFontStyle = defStyle;
              return;
          }

          // Fetch CurlyBraceColor setting
          var result = appSettings["CurlyBraceColor"]?.Value ?? string.Empty;
          try
          {
              CurlyBraceColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              CurlyBraceColor = defColor;
          }

          // Fetch CurlyBraceBackgroundColor setting
          result = appSettings["CurlyBraceBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              CurlyBraceBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              CurlyBraceBackgroundColor = defBackColor;
          }

          // Fetch CurlyBraceFontStyle setting
          result = appSettings["CurlyBraceFontStyle"]?.Value ?? string.Empty;
          CurlyBraceFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadBuiltinVariableHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.Black;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              BuiltinVariableColor = defColor;
              BuiltinVariableBackgroundColor = defBackColor;
              BuiltinVariableFontStyle = defStyle;
              return;
          }

          // Fetch BuiltinVariableColor setting
          var result = appSettings["BuiltinVariableColor"]?.Value ?? string.Empty;
          try
          {
              BuiltinVariableColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              BuiltinVariableColor = defColor;
          }

          // Fetch BuiltinVariableBackgroundColor setting
          result = appSettings["BuiltinVariableBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              BuiltinVariableBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              BuiltinVariableBackgroundColor = defBackColor;
          }

          // Fetch BuiltinVariableFontStyle setting
          result = appSettings["BuiltinVariableFontStyle"]?.Value ?? string.Empty;
          BuiltinVariableFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadBuiltinFunctionHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
          var defColor = Color.Black;
          var defBackColor = Color.Transparent;
          var defStyle = FontStyle.Regular;

          if (appSettings == null)
          {
              BuiltinFunctionColor = defColor;
              BuiltinFunctionBackgroundColor = defBackColor;
              BuiltinFunctionFontStyle = defStyle;
              return;
          }

          // Fetch BuiltinFunctionColor setting
          var result = appSettings["BuiltinFunctionColor"]?.Value ?? string.Empty;
          try
          {
              BuiltinFunctionColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
          }
          catch (Exception)
          {
              BuiltinFunctionColor = defColor;
          }

          // Fetch BuiltinFunctionBackgroundColor setting
          result = appSettings["BuiltinFunctionBackgroundColor"]?.Value ?? string.Empty;
          try
          {
              BuiltinFunctionBackgroundColor =
                  !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
          }
          catch (Exception)
          {
              BuiltinFunctionBackgroundColor = defBackColor;
          }

          // Fetch BuiltinFunctionFontStyle setting
          result = appSettings["BuiltinFunctionFontStyle"]?.Value ?? string.Empty;
          BuiltinFunctionFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadKeywordHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
         var defColor = Color.Blue;
         var defBackColor = Color.Transparent;
         var defStyle = FontStyle.Regular;

         if (appSettings == null)
         {
            KeywordColor = defColor;
            KeywordBackgroundColor = defBackColor;
            KeywordFontStyle = defStyle;
            return;
         }

         // Fetch KeywordColor setting
         var result = appSettings["KeywordColor"]?.Value ?? string.Empty;
         try
         {
            KeywordColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
         }
         catch (Exception)
         {
            KeywordColor = defColor;
         }

         // Fetch KeywordBackgroundColor setting
         result = appSettings["KeywordBackgroundColor"]?.Value ?? string.Empty;
         try
         {
            KeywordBackgroundColor =
               !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
         }
         catch (Exception)
         {
            KeywordBackgroundColor = defBackColor;
         }

         // Fetch KeywordFontStyle setting
         result = appSettings["KeywordFontStyle"]?.Value ?? string.Empty;
         KeywordFontStyle = ParseFontStyles(result, defStyle);
      }

      private void LoadDefaultWordHighlightSettings(KeyValueConfigurationCollection appSettings)
      {
         var defColor = Color.Black;
         var defBackColor = Color.Transparent;
         var defStyle = FontStyle.Regular;

         if (appSettings == null)
         {
            DefaultWordColor = defColor;
            DefaultWordBackgroundColor = defBackColor;
            DefaultWordFontStyle = defStyle;
            return;
         }

         // Fetch DefaultWordColor setting
         var result = appSettings["DefaultWordColor"]?.Value ?? string.Empty;
         try
         {
            DefaultWordColor = !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defColor;
         }
         catch (Exception)
         {
            DefaultWordColor = defColor;
         }

         // Fetch DefaultWordBackgroundColor setting
         result = appSettings["DefaultWordBackgroundColor"]?.Value ?? string.Empty;
         try
         {
            DefaultWordBackgroundColor =
               !string.IsNullOrEmpty(result) ? ColorTranslator.FromHtml(result) : defBackColor;
         }
         catch (Exception)
         {
            DefaultWordBackgroundColor = defBackColor;
         }

         // Fetch DefaultWordFontStyle setting
         result = appSettings["DefaultWordFontStyle"]?.Value ?? string.Empty;
         DefaultWordFontStyle = ParseFontStyles(result, defStyle);
      }
   }
}
