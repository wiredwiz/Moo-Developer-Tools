# Moo Developer Tools — CLAUDE.md

This file documents the codebase for AI-assisted development. Keep it updated as the project evolves.

---

## AI Development Workflow

- **Always use subagents** for implementation work — spawn agents via the `Agent` tool rather than making changes directly in the main session.
- **Always work in a git worktree** — use the `superpowers:using-git-worktrees` skill before starting any implementation to isolate changes from the main workspace.
- **Commit all work** — every completed task must be committed to the repo before the session ends.
- **Clean up worktrees** — after merging/completing work, remove the worktree and delete the feature branch.

---

## Project Overview

**Moo Udditor** is a Windows Forms IDE and terminal client targeting the **Moo programming language** — the scripting language used in MOO/MUD text-based multiplayer environments (LambdaMOO lineage). It gives Moo programmers real-time syntax checking, autocomplete, and a local edit workflow that replaces the raw telnet experience.

- **License:** BSD 3-Clause
- **Target framework:** .NET 6 / Windows Forms (`net6.0-windows`)
- **Author:** Thaddeus Ryker (Edgerunner.org)
- **Main binary:** `Moo Udditor.exe`

> **Important:** In this codebase, **MCP = Mud Communication Protocol** (not the LLM-related Model Context Protocol acronym).

---

## Solution Structure

```
Moo Developer Tools.sln
├── Org.Edgerunner.Moo.Udditor         ← Main WinForms application
├── Org.Edgerunner.Moo.Editor          ← Core editor component
├── Org.Edgerunner.Mud.Communication   ← TCP/TLS network layer
├── Org.Edgerunner.Mud.MCP             ← MUD Client Protocol (MCP 2.1) support
├── Org.Edgerunner.Moo.MooText         ← ANSI/color text processing pipeline
├── FastColoredTextBox                 ← Forked syntax-highlighting text control
├── Org.Edgerunner.Mud.Common          ← Shared config, address book, crypto
├── Org.Edgerunner.ANTLR4.Tools.Common ← ANTLR4 grammar utilities
├── Org.Edgerunner.Messaging           ← Internal messaging infrastructure
└── Org.Edgerunner.Common              ← General utilities/extensions
```

---

## Project Details

### Org.Edgerunner.Moo.Udditor (Main Application)

The IDE shell. Uses **Krypton docking/workspace** for MDI-style layout.

Key files:
- `Program.cs` — entry point
- `WindowManager.cs` — manages all dockable pages and their lifecycle
- `Main/Editor.cs` + `Editor_*.cs` — main form + menu partials (File, Edit, View, Terminal, Grammar, Window, Help)
- `Pages/TerminalPage.cs` — wraps the MooClientTerminal in a dockable page
- `Pages/MooCodeEditorPage.cs` — wraps the code editor in a dockable page
- `Pages/MooDocumentEditorPage.cs` — document editor page
- `Pages/ParserMessageDisplayPage.cs` — shows real-time parser error messages
- `Communication/OutOfBand/LocalEditHandler.cs` — handles incoming local edit OOB requests
- `Communication/OutOfBand/LocalEditUploader.cs` — sends edited verb code back to the server
- `Dialogs/ConnectionInfoPrompt.cs` — connection dialog
- `Dialogs/Setup.cs` — settings dialog

Dependencies: Krypton.Docking, NLog, ANTLR4.Runtime, Org.Edgerunner.MooSharp.Language.Grammar (referenced DLL)

### Org.Edgerunner.Moo.Editor

Core editor library. Contains the Moo language intelligence and terminal control.

Key areas:
- `Moo.cs` — static class: keyword list, ~200 built-in function names, lexer/parser factory by dialect
- `GrammarDialect.cs` — enum: `LambdaMoo`, `ToastStunt`, `Edgerunner`
- `Language/Parsing/` — ANTLR4-driven validators: `LambdaMooValidator`, `ToastStuntMooValidator`, `EdgerunnerMooValidator`
- `Language/Navigation/` — code navigation helpers
- `Autocomplete/` — `Snippets.cs`, `AutoIndentingSnippet.cs`, `DeclarationSnippet.cs`, etc.
- `SyntaxHighlighting/` — token-based colorization
- `Controls/` — `MooClientTerminal` (the terminal UI control)
- `Communication/` — editor-level OOB wiring
- `AnsiManager.cs` — ANSI escape code processing
- `WorldConfigurator.cs` / `WorldManager.cs` — per-world settings dialogs
- `Configuration/` — configuration model

Grammar dialects supported:
| Dialect | Description |
|---|---|
| LambdaMoo | Original classic Moo syntax |
| ToastStunt | Extended with new operators and types |
| Edgerunner | Personal fork: adds `+=`/`-=` etc. and `++`/`--` |

### Org.Edgerunner.Mud.Communication

Network layer for connecting to MUD/MOO servers.

Key files:
- `MudClientSession.cs` — plain TCP async socket session; 80,000-byte command buffer
- `TlsMudClientSession.cs` — TLS variant
- `MudClient.cs` — higher-level client wrapper
- `OutOfBand/OutOfBandMessageProcessor.cs` — detects and routes `#$#` prefixed lines
- `Buffers/` — internal async ring/channel buffers
- `RootMessageProcessor.cs` — top-level message dispatch

OOB prefix: `#$#` (configurable per session). Lines starting with `#$"` are quoted in-band.

### Org.Edgerunner.Mud.MCP

MUD Client Protocol (MCP 2.1) support — **partially implemented, actively being completed**.

Key files:
- `Message.cs` — represents a parsed MCP message (`Name`, `Key`, `Data` dictionary)
- `McpClientSession.cs` — a negotiated session: holds auth key, protocol version, supported packages
- `McpClientSessionManager.cs` — handles the initial `mcp` handshake, version negotiation, session creation
- `McpUtils.cs` — generates random session keys
- `Interfaces/IMcpSession.cs`, `IMcpPackage.cs`, `IMcpProtocolHandler.cs`, `IMCPConfiguration.cs`
- `Exceptions/InvalidMcpMessageException.cs`

Current state: version negotiation (the `mcp` handshake) is implemented. Package negotiation (`mcp-negotiate`) and cord support (`mcp-cord`) are not yet complete.

See `docs/mcp2-protocol.md` for the full MCP 2.1 protocol reference.

### Org.Edgerunner.Moo.MooText

Text processing pipeline for Moo color/ANSI output.

- `MooTextPipeline.cs` / `MooTextPipelineBuilder.cs` — fluent pipeline builder
- `MooColorTextProcessor.cs` — processes Moo `@color` tags
- `PlainTextToHtmlConverter.cs` — renders output as HTML for RichText display
- `MooText.cs` — core text model
- `ITextPipelineProcessorExtension.cs` / `ITextProcessorPipeline.cs` — extension interfaces

### FastColoredTextBox

Fork of the open-source FastColoredTextBox control, modified for Moo-specific syntax highlighting needs (including `ExportToRTF.cs` which has pending changes).

### Org.Edgerunner.Mud.Common

Shared infrastructure:
- `WorldConfiguration.cs` — per-server connection settings
- `AddressBook.cs` — saved connection profiles
- `UserLogin.cs` — credential model
- `ApplicationKeyManager.cs` — manages application-level keys
- `ColorConverter.cs` — color utility
- `Cryptography/` — encryption helpers
- `Navigation/` — navigation utilities

---

## Key Architectural Patterns

- **Dockable pages** — all major UI surfaces are `ManagedPage` subclasses managed by `WindowManager`
- **OOB pipeline** — incoming text is scanned for `#$#` prefix; matching lines are routed to registered `IOutOfBandMessageHandler` implementations
- **Text pipeline** — `MooTextPipeline` is a composable chain of `ITextPipelineProcessorExtension` stages transforming raw server text before display
- **ANTLR4 parsing** — each dialect has its own generated lexer/parser; the editor runs a background parse on every change and reports errors via `ParsingCompleteEventArgs`
- **MCP sessions** — `McpClientSessionManager` brokers the handshake; resulting `McpClientSession` holds the auth key used in all subsequent messages

---

## Current Work / Known Gaps

- **MCP package negotiation** (`mcp-negotiate`) — not yet implemented beyond the initial `mcp` handshake
- **MCP cord support** (`mcp-cord`) — not yet implemented
- **MCP package handlers** — no concrete `IMcpPackage` implementations exist yet
- **Local edit OOB protocol** — implemented; MCP-based local edit may follow once MCP is complete
- `FastColoredTextBox/Text/ExportToRTF.cs` — has uncommitted modifications
- `Org.Edgerunner.Moo.MooText` project — has uncommitted modifications

---

## Dependencies

| Package | Purpose |
|---|---|
| `Antlr4.Runtime` 4.6.6 | Grammar parsing |
| `Krypton.Docking` / `Krypton.Toolkit` | Dockable MDI UI |
| `NLog` 5.2.8 | Structured logging |
| `Markdig` | Markdown rendering |
| `Org.Edgerunner.MooSharp.Language.Grammar` | Pre-built grammar DLL (in `References/`) |


<!-- BEGIN BEADS INTEGRATION v:1 profile:minimal hash:ca08a54f -->
## Beads Issue Tracker

This project uses **bd (beads)** for issue tracking. Run `bd prime` to see full workflow context and commands.

### Quick Reference

```bash
bd ready              # Find available work
bd show <id>          # View issue details
bd update <id> --claim  # Claim work
bd close <id>         # Complete work
```

### Rules

- Use `bd` for ALL task tracking — do NOT use TodoWrite, TaskCreate, or markdown TODO lists
- Run `bd prime` for detailed command reference and session close protocol
- Use `bd remember` for persistent knowledge — do NOT use MEMORY.md files

## Session Completion

**When ending a work session**, you MUST complete ALL steps below. Work is NOT complete until `git push` succeeds.

**MANDATORY WORKFLOW:**

1. **File issues for remaining work** - Create issues for anything that needs follow-up
2. **Run quality gates** (if code changed) - Tests, linters, builds
3. **Update issue status** - Close finished work, update in-progress items
4. **PUSH TO REMOTE** - This is MANDATORY:
   ```bash
   git pull --rebase
   bd dolt push
   git push
   git status  # MUST show "up to date with origin"
   ```
5. **Clean up** - Clear stashes, prune remote branches
6. **Verify** - All changes committed AND pushed
7. **Hand off** - Provide context for next session

**CRITICAL RULES:**
- Work is NOT complete until `git push` succeeds
- NEVER stop before pushing - that leaves work stranded locally
- NEVER say "ready to push when you are" - YOU must push
- If push fails, resolve and retry until it succeeds
<!-- END BEADS INTEGRATION -->
