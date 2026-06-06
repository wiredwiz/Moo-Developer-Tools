# Feature Suggestions
**Date:** 2026-06-06  
**Context:** Moo Udditor — a Windows IDE and terminal client for MOO/MUD programmers. Gives Moo coders real-time syntax checking, autocomplete, and a local edit workflow that replaces the raw telnet experience.

---

## Connectivity & Workflow

### Multi-Server Split View
Show two terminal connections side by side in a split docking cell. Useful when testing objects across two servers, comparing production vs. development environments, or running commands on one server while watching output on another.

### Session Transcript Logging
Opt-in per-world logging of all terminal I/O to a timestamped file. Invaluable for debugging async verb interactions, reproducing intermittent bugs, and sharing session output with other developers. Configurable via world settings; log rotation by size or date.

### Macro / Alias System
Define short aliases that expand to full MOO commands, stored per-world in the address book. Common pattern in MUD clients (e.g., `@go` → `@go to #1234`). Support basic variable substitution (`%1`, `%2`) and multi-line macros for sequences of commands.

---

## Editor / Code Intelligence

### Complete MCP Package Negotiation
The `mcp-negotiate` and `mcp-cord` phases of MUD Client Protocol 2.1 are not yet implemented (noted as a known gap). Completing these would enable the full round-trip local-edit workflow via MCP rather than the current OOB fallback — a significant improvement for servers that support it.

### Verb Diff View
When uploading an edited verb back to the server, show a diff of the local version vs. the version currently on the server before sending. Prevents accidental overwrites and gives the programmer a clear picture of what is about to change.

### Object / Verb Browser
A dockable side panel that issues MOO introspection commands (`@show`, `@list`, `@props`) and populates a tree of objects → verbs → properties. Double-click a verb to open it in the code editor. Refreshable on demand. Makes navigating large MOO codebases significantly faster than typing commands manually.

### Inline Server Error Annotation
When the server returns a compile error on verb upload, parse the line number from the error response and annotate it directly in the editor gutter with a red squiggle and tooltip — the same experience as a local compiler. Currently errors appear in the terminal and the programmer has to manually find the line.

---

## Quality of Life

### Command History Persistence
The `CommandBuffer` (15-item ring) is held in memory only and lost on close. Persist command history per-world to disk across sessions. Configurable history depth. Provides the same experience as a terminal shell's command history.

### World-Aware Autocomplete
Seed the code editor's autocomplete with verb names, object names, and property names fetched live from the connected server via introspection commands. Local grammar-based completions would then reflect the actual server state rather than only the built-in function list. Updates on reconnect.

### Connection Health Indicator
A status bar chip showing round-trip latency (measured by timing a known no-op command) and bytes in/out per second. Lets the programmer immediately distinguish a slow server from a stuck client, and see whether a long-running verb is still executing or the connection has silently dropped.

### Per-World Color Scheme
Allow each world connection to have its own ANSI color palette and editor theme. Useful when connected to multiple servers simultaneously — the visual distinction makes it immediately obvious which terminal belongs to which server.

### Find in Terminal Output
A search bar (Ctrl+F style) that highlights and navigates matches in the terminal scrollback buffer. MOO programmers often need to locate a specific error or value in a long stream of output without exporting to a file.
