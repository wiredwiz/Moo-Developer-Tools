# Contextual Autocomplete via SDWC (udd-7g2) — Design

**Date:** 2026-06-09
**Bead:** udd-7g2 — wire verb/property/core-reference completion icons via contextual completion
**Status:** Approved by user (including the `me`/`player` deferral noted below)

## Goal

The Moo code editor's autocomplete currently offers only static items (keywords, built-in
functions, snippets). The `CompletionIconCategory.Verb` (purple), `Property` (cyan), and
`CoreReference` (green) icons exist in `CompletionIconFactory` but are never used because no
completion items of those kinds are ever generated.

This work adds **contextual member completion**: when the user types `$`, `:` or `.` in a verb
editor attached to a connected world, the editor queries the world (via the now-testable SDWC
`IMooWorldQueryProvider`) for the relevant verbs/properties and offers them as completion items
carrying the proper icons.

## Non-Goals

- Resolving `me` / `player` to an object id — **deferred** (no player-object-id source exists in
  the codebase yet; user approved the deferral).
- Resolving arbitrary expressions (`$foo:`, barewords, chained calls like `x.y.z`). These are
  unresolved contexts and member completion is silently skipped.
- Tooltips/documentation overlays (`GetVerbDocumentationAsync` etc.) — out of scope here.
- Any change to the static keyword/builtin/snippet completion behavior.

## Architecture

Three new units in `Org.Edgerunner.Moo.Editor` (pure logic, headless-testable), plus plumbing in
`Org.Edgerunner.Moo.Udditor`.

### 1. Completion context detector (pure)

Given the text to the left of the caret on the current line, classify the completion context:

| Input shape (left of caret) | Context | Query target |
|---|---|---|
| `$<frag>` | CoreReference | properties of `#0` |
| `<operand>:<frag>` | Verb | verbs of resolved operand |
| `<operand>.<frag>` | Property | properties of resolved operand |
| anything else | None | static completion only |

The detector extracts the operand token and the typed fragment. It must not misfire inside
strings or comments (reuse the existing token/line scanning approach where practical) and must
not treat `..` (range) or `:` in other positions as member triggers when the operand is absent.

### 2. Operand object resolver (pure)

Resolves the operand text to a `MooObjectId` or null:

| Operand | Resolution |
|---|---|
| empty + `$` trigger | `#0` |
| `#<n>` literal | `#n` |
| `this` | the page's `ContextObjectId` (see plumbing) — null when the page has none |
| `me`, `player` | null (deferred) |
| anything else | null |

Null resolution ⇒ member completion silently skipped; the static menu behaves exactly as today.

### 3. Async member completion source

Bridges the detector/resolver to `IMooWorldQueryProvider`:

- On a member-completion trigger with a resolved object id, kick off
  `GetVerbsAsync` (verb context) or `GetPropertiesAsync` (property / core-reference context)
  on a background task. Never block the UI thread.
- Use a `CancellationTokenSource` per request; a new trigger (or menu close) cancels the stale
  request.
- Results are marshalled back to the UI thread and injected into the `AutocompleteMenu` items
  for the open popup; if the popup has closed, results are discarded.
- The provider handed to the editor is the connection's **caching** registry provider
  (`session.QueryProviders.Query`), so repeated triggers against the same object are served from
  cache — no extra debounce layer is needed.
- All provider failures (timeout, `NotImplementedException` fall-through, disconnect, malformed
  payload) are caught and logged at trace/debug level; the user just sees the static menu.

### 4. Menu items and icons (the udd-7g2 acceptance)

| Item kind | `ImageIndex` |
|---|---|
| verb name | `(int)CompletionIconCategory.Verb` |
| property name | `(int)CompletionIconCategory.Property` |
| `$`-completion (property of `#0`) | `(int)CompletionIconCategory.CoreReference` |

Member items are filtered against the typed fragment using the menu's existing matching. In a
member context the member items are listed first, before any static items the fragment also
matches.

## Plumbing

### `MooCodeEditorPage`

- Add `MooObjectId? ContextObjectId { get; set; }` — the object whose verb is being edited
  (the meaning of `this`).
- `QueryProvider` already exists (`MooCodeEditorPage.cs:63`); it becomes live with this change.

### Wiring at page creation (both local-edit paths)

1. **`LocalEditHandler`** — parse the object id out of `UploadCommand`
   (e.g. `@program #123:verbname` → `#123`). Set `page.ContextObjectId` and
   `page.QueryProvider = client.QueryProviders.Query` after creating the code editor page.
2. **`WindowManagerSimpleEditConsumer`** — parse `request.Reference` the same way; wire
   `ContextObjectId` and `QueryProvider` from `uploader.ClientTerminal.QueryProviders.Query`.

Pages opened from files (no connection) get neither; member completion is simply inactive there.

The id parsing is a small shared helper (pure, testable): extract the first `#<digits>` object
reference from a command/reference string; null when absent.

## Error Handling

- No provider / no context id / unresolved operand → silently skip member completion.
- Query timeout (SDWC bounded 10 s) or cancellation → discard; static items remain.
- Disconnected mid-query → same as timeout; no user-visible error.
- Never throw from the autocomplete path; failures must not break the static menu.

## Testing

Headless unit tests only (NO GUI test hosts — instantiating editor controls in tests crashes
the test host):

- Context detector: trigger shapes, string/comment immunity, `..` and stray `:` cases.
- Operand resolver: `#n`, `this` (with/without context id), `me`/`player` → null, garbage → null.
- Upload-command/reference id parsing: `@program #123:foo`, simpleedit references, no-id cases.
- Member completion source: fake `IMooWorldQueryProvider` verifying correct query per context,
  cancellation of stale requests, exception swallowing, and correct `ImageIndex` per item kind.

Build-only verification for the WinForms layer; manual smoke test by the user against a live
SDWC-capable world.

## Implementation Risk (validate during planning)

The FastColoredTextBox `AutocompleteMenu` is currently fed a fully static item list at page
construction (`BuildAutocompleteMenu`). Injecting per-keystroke, async, context-dependent items
needs a supported insertion point (e.g. rebuilding `Items.SetAutocompleteItems(...)` on trigger,
or a dynamic `IEnumerable<AutocompleteItem>` source). The plan must confirm which mechanism the
forked control supports before committing to the bridge design.
