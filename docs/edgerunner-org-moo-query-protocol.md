# edgerunner-org-moo-query — MCP Package Protocol (v1.0)

**Status:** Normative. Both the server package (`Server Packages/edgerunner-org-moo-query.moo`)
and the client implementation (`Org.Edgerunner.Mud.MCP/Packages/McpQueryPackage.cs`) are written
against this document.

> MCP throughout = **MUD Client Protocol 2.1** (https://www.moo.mud.org/mcp/mcp2.html),
> NOT the LLM "Model Context Protocol".

## 1. Package identity

- Package name: `edgerunner-org-moo-query`
- Version: `1.0` (min = max = 1.0)
- Transport: standard MCP 2.1 messages over the negotiated session; no cords.

## 2. Negotiation

Support is settled during the initial MCP handshake: the `mcp` startup exchange followed by
`mcp-negotiate`, in which each side advertises its packages and versions via
`mcp-negotiate-can`. The client advertises `edgerunner-org-moo-query` with
`min-version: 1.0 max-version: 1.0`. When the server's `mcp-negotiate-can` for this package
overlaps that range, the package is in effect for the session. If the server never advertises
it, the client MUST NOT send any message of this package.

## 3. Message model

Every request is a single-line MCP message carrying the session auth key, a client-generated
`tag` field, and the parameters listed in §5. Every reply echoes the tag and carries the
payload in one `data*` multiline field containing **minified JSON**.

### 3.1 Correlation

- `tag` is an opaque client-generated string, unique per in-flight request (the reference
  client uses a monotonically increasing integer rendered as a string).
- The server MUST echo the request's tag verbatim on the reply (or error) message.
- The client matches replies to pending requests solely by tag. Replies bearing unknown tags
  are dropped.

### 3.2 Chunking (`data*` framing)

The reply JSON is one logical string. The server splits it into continuation lines of at most
**4000 characters** purely as transport framing:

```
#$#<reply-name> <authkey> tag: "<tag>" data*: "" _data-tag: <dtag>
#$#* <dtag> data: <chunk-1>
#$#* <dtag> data: <chunk-2>
#$#: <dtag>
```

The client concatenates all `data` chunks **verbatim with no separator**, then parses the
result as JSON once the closing line arrives. Because the JSON is minified and MOO strings
cannot contain newline characters, chunk boundaries never need escaping.

### 3.3 Encoding conventions

- Object numbers are **bare JSON ints** (no `#`, never quoted strings).
- Verb names stay as **raw MOO verb-names strings** (e.g. `"g*et put"`); consumers split on
  whitespace and interpret `*`.
- Envelope keys are single characters; list rows are positional arrays.
- `q` = queried object number; `r` = resolved (defining) object number. These appear ONLY on
  `-verb-info`, `-verb-doc`, and `-verb-code` replies.
- All JSON is minified (no insignificant whitespace).

### 3.4 Request parameter conventions

- `object` / `owner` values are object references in `#123` or `123` form.
- `verb` / `prop` values are plain names (a verb reference may be any alias).
- `owner` on `-owned` is always present on the wire; the **empty string** means "the
  connected player".

## 4. Permissions

Every server handler runs under `set_task_perms()` of the connected player. Visibility and
readability outcomes are exactly what the player's own MOO permissions yield; permission
failures surface as `-error` replies with code `E_PERM`.

## 5. Message catalog

All names below are suffixes of `edgerunner-org-moo-query`. Every request also carries `tag`.

| Request | Params | Reply | JSON payload |
|---|---|---|---|
| `-core-objects` | — | `-core-objects-reply` | `{"d":[[num,name,[aliases]],…]}` — one row per object referenced by a `#0` property (`$`-registered), deduped, valid objects only |
| `-player` | — | `-player-reply` | `{"p":num}` — the object number of the player connected on this session (`toint(session.connection)`); `-1` = none |
| `-children` | `object` | `-children-reply` | `{"d":[[num,name,[aliases]],…]}` — immediate children |
| `-owned` | `owner` | `-owned-reply` | `{"d":[[num,name,[aliases]],…]}` — from the target's `.owned_objects` bookkeeping; a core without that property answers `-error E_INVARG` (servers MUST NOT fall back to a DB walk) |
| `-parent` | `object` | `-parent-reply` | `{"p":num}`; `-1` = no parent |
| `-verbs` | `object` | `-verbs-reply` | `{"d":[["g*et put",isLocal],…]}` — each row is `[raw verb-names string, isLocal]` where `isLocal` is `1` when the name is local to the queried object and `0` when inherited; local + inherited (ancestor walk), deduped (nearest definition wins); unreadable ancestors contribute nothing |
| `-verb-info` | `object`, `verb` | `-verb-info-reply` | `{"q":num,"r":num,"a":"names","o":num,"p":"rxd","g":["this","none","this"]}` — `a` = raw names string, `o` = owner, `p` = permission flags, `g` = dobj/prep/iobj specs as returned by `verb_args()` |
| `-verb-doc` | `object`, `verb` | `-verb-doc-reply` | `{"q":num,"r":num,"l":[lines]}` — `l` = the leading string-literal lines of the verb code (unescaped) |
| `-verb-code` | `object`, `verb` | `-verb-code-reply` | `{"q":num,"r":num,"l":[lines]}` — `verb_code()` lines |
| `-props` | `object` | `-props-reply` | `{"d":[["name",isLocal],…]}` — each row is `[property name, isLocal]` where `isLocal` is `1` when the name is local to the queried object and `0` when inherited; local + inherited, deduped (nearest definition wins) |
| `-prop-info` | `object`, `prop` | `-prop-info-reply` | `{"n":"name","o":num,"p":"rc","t":typecode,"v":"preview"}` — `t` = `typeof()` code, `v` = first 80 characters of `toliteral(value)` |
| `-prop-doc` | `object`, `prop` | `-prop-doc-reply` | `{"l":[lines]}` — `toliteral(value)` split into ≤78-char lines, capped at 50 lines |
| `-prop-value` | `object`, `prop` | `-prop-value-reply` | `{"t":typecode,"v":"literal"}` — full `toliteral(value)` |
| `-constant-value` | `constant` | `-constant-value-reply` | `{"v":"<toliteral(value)>"}` — the server evaluates the named language constant (`eval("return <constant>;")`) and returns `toliteral(value)`; e.g. `NUM` → `{"v":"0"}`. `constant` MUST be a bare identifier (letters/digits/underscore); any other name answers `-error E_INVARG` |
| `-constant-tostr` | `constant` | `-constant-tostr-reply` | `{"v":"<tostr(value)>"}` — `eval("return tostr(<constant>);")`; e.g. `E_PERM` → `{"v":"Permission denied"}`. Bare-identifier names only |

The constant queries let the client show authoritative, server-accurate type codes and error
messages on hover; clients that cannot reach the server fall back to a built-in table. The
bare-identifier restriction means the server's `eval` can only resolve a single constant token,
never run arbitrary code.

Verb info/doc/code resolve the **defining ancestor**: the server walks up from the queried
object to the first ancestor whose `verb_info()` answers for the name; that ancestor is `r`.
No match anywhere on the chain → `-error E_VERBNF`.

### 5.1 Worked example

```
C→S: #$#edgerunner-org-moo-query-verbs K7% tag: 12 object: #123
S→C: #$#edgerunner-org-moo-query-verbs-reply K7% tag: "12" data*: "" _data-tag: 9911
     #$#* 9911 data: {"d":[["g*et put",1],["look_self",0]]}
     #$#: 9911
```

```
C→S: #$#edgerunner-org-moo-query-verb-info K7% tag: 13 object: #123 verb: "g*et"
S→C: #$#edgerunner-org-moo-query-verb-info-reply K7% tag: "13" data*: "" _data-tag: 9912
     #$#* 9912 data: {"q":123,"r":6,"a":"g*et put","o":2,"p":"rxd","g":["this","none","this"]}
     #$#: 9912
```

```
C→S: #$#edgerunner-org-moo-query-owned K7% tag: 14 owner: ""
S→C: #$#edgerunner-org-moo-query-owned-reply K7% tag: "14" data*: "" _data-tag: 9913
     #$#* 9913 data: {"d":[[101,"my room",["room"]],[102,"hat",[]]]}
     #$#: 9913
```

```
C→S: #$#edgerunner-org-moo-query-player K7% tag: 15
S→C: #$#edgerunner-org-moo-query-player-reply K7% tag: "15" data*: "" _data-tag: 9914
     #$#* 9914 data: {"p":62}
     #$#: 9914
```

```
C→S: #$#edgerunner-org-moo-query-constant-tostr K7% tag: 16 constant: E_PERM
S→C: #$#edgerunner-org-moo-query-constant-tostr-reply K7% tag: "16" data*: "" _data-tag: 9915
     #$#* 9915 data: {"v":"Permission denied"}
     #$#: 9915
```

## 6. Errors

Shared single-line error reply:

```
#$#edgerunner-org-moo-query-error <authkey> tag: "<tag>" code: E_PERM message: "You can't read that"
```

- `code` — the MOO error constant name (`E_PERM`, `E_INVARG`, `E_VERBNF`, `E_PROPNF`, …).
- `message` — human-readable text; the server replaces embedded `"` with `'` so the value
  survives MCP quoting.

Client behavior on error: degrade to the `IMooWorldQueryProvider` contract value (`null` /
empty list) and log the event — never throw into editor consumers.

## 7. Type codes

`t` values are the MOO `typeof()` codes: 0 = INT, 1 = OBJ, 2 = STR, 3 = ERR, 4 = LIST,
9 = FLOAT (further codes per server family, e.g. ToastStunt MAP = 10, transmitted as-is).
