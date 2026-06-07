# Moo Udditor — Feature Suggestions

## Connectivity & Workflow

1. **Linked split connections** — Link two terminal connections so they share a single command input. Both connections tile side by side automatically when linked, and any command typed in the shared input bar is transmitted to both servers simultaneously. Output from each server remains independent and visible in its own pane. Useful for testing a verb change across a production and development server at the same time, verifying identical behaviour on two different MOO codebases, or running parallel sessions where commands need to be issued to both worlds in lockstep. Linking and unlinking would be a per-session toggle, with each terminal reverting to its own independent input when unlinked.

2. **Session transcript logging** — Opt-in per-world logging of all terminal I/O to a timestamped file. Invaluable for debugging async verb interactions and reproducing intermittent bugs. Configurable via world settings with log rotation by size or date.

3. **Macro / alias system** — Define short aliases that expand to full MOO commands, stored per-world in the address book. Support basic variable substitution (`%1`, `%2`) and multi-line macros for sequences of commands.

## Editor / Code Intelligence

4. **Complete MCP package negotiation** — `mcp-negotiate` and `mcp-cord` phases of MUD Client Protocol 2.1 are currently unimplemented. Finishing these enables the full round-trip local-edit workflow via MCP rather than the current OOB fallback — a significant improvement for servers that support it.

5. **Verb diff view** — When uploading an edited verb, show a diff of the local version vs. the server's current version before sending. Prevents accidental overwrites.

6. **Object / verb browser** — A dockable side panel that issues MOO introspection commands (`@show`, `@list`, `@props`) and populates a tree of objects → verbs → properties. Double-click a verb to open it in the code editor.

7. **Inline server error annotation** — When the server returns a compile error on verb upload, parse the line number and annotate it directly in the editor gutter with a squiggle and tooltip — same experience as a local compiler.

## Quality of Life

8. **Command history persistence** — The 15-item command buffer is lost on close. Persist command history per-world to disk across sessions with a configurable depth.

9. **World-aware autocomplete** — Seed the code editor's autocomplete with verb names, object names, and property names fetched live from the connected server via introspection commands, updated on reconnect.

10. **Connection health indicator** — A status bar chip showing round-trip latency and bytes in/out per second. Lets the programmer immediately distinguish a slow server from a stuck client.

11. **Per-world color scheme** — Each world connection gets its own ANSI color palette and editor theme. When connected to multiple servers simultaneously, the visual distinction makes it immediately obvious which terminal belongs to which server.
