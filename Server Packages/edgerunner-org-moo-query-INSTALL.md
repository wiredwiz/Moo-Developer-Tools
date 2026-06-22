# Installing the edgerunner-org-moo-query server package

This package answers the developer-information queries used by Moo Udditor (object browser,
contextual autocomplete, verb/property inspection). It targets cores with a JHCore-style MCP 2.1
implementation — the same framework that hosts `dns-org-mud-moo-simpleedit`.

Protocol reference: `docs/edgerunner-org-moo-query-protocol.md` in the Moo Developer Tools
repository. The package object's `description` property carries a condensed copy.

## Prerequisites

- A working server-side MCP 2.1 framework with package dispatch (handler verbs named
  `handle_<message>`, called as `(session, @params)`), as used by your simpleedit package.
  **The handler verb name must match the wire message name exactly, including dashes** — the
  `verb-info` request dispatches to a verb literally named `handle_verb-info`. The stock JHCore
  dispatcher derives the handler as `"handle_" + message` with **no** dash→underscore translation
  (see `message_name_to_verbname`), so the dump names every multi-word handler with hyphens
  (`handle_prop-value`, *not* `handle_prop_value`). The reference `dns-org-mud-moo-simpleedit`
  package uses only single-word messages, so it never reveals this; a mis-named handler is
  silently dropped (no reply, no error, no traceback). If you confirm your core's dispatcher
  *does* translate dashes to underscores, rename the handlers to underscores to match.
- A wizard character.

## Steps

1. **Create the package object** as a child of your core's generic MCP package parent (the same
   parent your simpleedit package object uses):

   ```
   @create <mcp-package-parent> named edgerunner-org-moo-query
   ```

   Note the object number it reports (e.g. `#231`).

2. **Replace the placeholder.** In `edgerunner-org-moo-query.moo`, search-replace every
   occurrence of `#XXX` with your object number. Also review the two `@chown … #2` lines per
   verb: `#2` assumes your archwizard is `#2`; adjust if not.

3. **Properties and verbs are created by the dump.** The dump file now contains `@prop` lines
   for all six metadata properties (`use_generate_json`, `version_range`, `messages_in`,
   `messages_out`, `aliases`, `description`) and `@verb` lines for all 17 handler verbs, so
   no manual pre-creation is needed. If your MCP package parent already defines some of those
   metadata properties (e.g. `aliases`, `description`), the corresponding `@prop` lines will
   fail with a harmless "property already defined"-style error; the `;;` assignment that
   follows will still set the value on the inherited property as expected.

4. **Load the dump.** Paste the edited file into your wizard connection (or use your usual
   dump-loading mechanism). The `@prop`/`@verb` lines create the properties and verbs; the
   `;;` lines then set the property values; and the `@args`/`@program` blocks set the verb
   argument specs and program the verbs.

5. **Adapt the session/send accessors if needed.** All wire output is isolated in two verbs,
   `send_reply` and `send_error`. They do **not** build MCP lines by hand — they hand the reply to
   your core's framework via `session:send(<message>, this:parse_send_args(<message>, …))`, which
   stamps the session authentication key, adds the package prefix, and chunks the multiline `data*`
   field. The package assumes the session object exposes:
   - `session:send(message, alist)` — the framework's outbound send entry (the same path your
     simpleedit package reaches, e.g. via the generic `send_*` verb), and
   - `session.connection` — the connected player object (used by `set_task_perms(session.connection)`
     at the top of each handler).

   The package never references the auth key directly — the framework supplies it from wherever your
   core stores it. If your core's session uses a different send entry or connection accessor, adjust
   those two verbs (and the `set_task_perms` line); nothing else touches the session.

6. **Register the package** with your core's MCP package registry, exactly the way your
   simpleedit package is registered (e.g. adding the object to the MCP registry's package list —
   consult how `dns-org-mud-moo-simpleedit` was installed on your core).

7. **Verify.** Connect with Moo Udditor. The client advertises `edgerunner-org-moo-query 1.0`
   during the MCP handshake; once the server's `mcp-negotiate-can` confirms it, Udditor's query
   features go live. Quick manual check from a raw client:

   ```
   #$#mcp authentication-key: TEST version: 2.1 to: 2.1
   #$#mcp-negotiate-can TEST package: edgerunner-org-moo-query min-version: "1.0" max-version: "1.0"
   #$#mcp-negotiate-end TEST
   #$#edgerunner-org-moo-query-parent TEST tag: 1 object: #1
   #$#edgerunner-org-moo-query-prop-value TEST tag: 2 object: #1 prop: name
   ```

   Expect a `-parent-reply` (data `{"p":<n>}`) and a `-prop-value-reply` (data `{"t":2,"v":"…"}`).
   The second line is the important one: it exercises a **hyphenated** message name, which is
   exactly where a non-translating dispatcher silently drops the request if the handler isn't
   named `handle_prop-value`.

## Notes

- Every handler runs under `set_task_perms()` of the connected player; players see exactly what
  their MOO permissions allow. Failures come back as `-error` replies (`E_PERM`, `E_VERBNF`, …).
- `-owned` relies on the core's `.owned_objects` bookkeeping (maintained by `@create`/`@recycle`
  in LambdaCore lineage). Cores without it answer `-error E_INVARG`; the package never walks the
  whole database.
- On ToastStunt the JSON encoder uses the `generate_json()` builtin (probed once, cached in
  `.use_generate_json`); on classic LambdaMOO it falls back to a hand-rolled encoder. Reset the
  probe with `;#231.use_generate_json = -1` after a server-family change.
