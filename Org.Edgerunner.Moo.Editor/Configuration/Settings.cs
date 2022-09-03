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

namespace Org.Edgerunner.Moo.Editor.Configuration
{
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
            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = filePath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var appSettings = config.AppSettings.Settings;
            LoadFrom(appSettings);
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
         var defGrammarDialect = GrammarDialect.Edgerunner;
         var defCodeFolding = true;
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
            DefaultGrammarDialect = defGrammarDialect;
            EditorShowTextIndentGuides = defIndentationGuides;
            EditorShowCodeFolding = defCodeFolding;
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

         // Fetch EditorAutocompleteDelay setting
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
          var defColor = Color.Tomato;
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
          var defColor = Color.Tomato;
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
