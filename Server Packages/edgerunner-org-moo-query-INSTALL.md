# Installing the edgerunner-org-moo-query server package

This package answers the developer-information queries used by Moo Udditor (object browser,
contextual autocomplete, verb/property inspection). It targets cores with a JHCore-style MCP 2.1
implementation — the same framework that hosts `dns-org-mud-moo-simpleedit`.

Protocol reference: `docs/edgerunner-org-moo-query-protocol.md` in the Moo Developer Tools
repository. The package object's `description` property carries a condensed copy.

## Prerequisites

- A working server-side MCP 2.1 framework with package dispatch (handler verbs named
  `handle_<message>` called as `(session, @params)`), as used by your simpleedit package.
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

3. **Add the properties.** The framework may already define some metadata properties on the
   parent. For each property the dump assigns (`use_generate_json`, `version_range`,
   `messages_in`, `messages_out`, `aliases`, `description`), create it on your object if the
   parent doesn't provide it:

   ```
   @property #231.use_generate_json -1
   ```

   (repeat for the others, or rely on inherited definitions).

4. **Load the dump.** Paste the edited file into your wizard connection (or use your usual
   dump-loading mechanism). The `;;` lines set the properties; the `@args`/`@program` blocks
   create the verbs.

5. **Adapt the session accessors if needed.** All wire output is isolated in two verbs:
   `send_reply` and `send_error`. They assume the session object exposes:
   - `session.key` — the MCP session authentication key, and
   - `session.connection` — the connected player object.

   If your core's MCP session object uses different property names, fix them in those two verbs
   (and the `set_task_perms(session.connection)` line at the top of each handler) — nothing else
   touches the session.

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
   ```

   Expect a `-parent-reply` multiline block whose data is `{"p":<n>}`.

## Notes

- Every handler runs under `set_task_perms()` of the connected player; players see exactly what
  their MOO permissions allow. Failures come back as `-error` replies (`E_PERM`, `E_VERBNF`, …).
- `-owned` relies on the core's `.owned_objects` bookkeeping (maintained by `@create`/`@recycle`
  in LambdaCore lineage). Cores without it answer `-error E_INVARG`; the package never walks the
  whole database.
- On ToastStunt the JSON encoder uses the `generate_json()` builtin (probed once, cached in
  `.use_generate_json`); on classic LambdaMOO it falls back to a hand-rolled encoder. Reset the
  probe with `;#231.use_generate_json = -1` after a server-family change.
