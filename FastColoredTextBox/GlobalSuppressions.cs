// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// ReSharper disable once StringLiteralTypo
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "It is well known that a custom winforms user control will only work on Windows.", Scope = "namespaceanddescendants", Target = "~M:FastColoredTextBoxNS")]