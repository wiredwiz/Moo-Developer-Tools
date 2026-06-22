# Inherited-member icon distinction — design

**Date:** 2026-06-22
**Status:** Approved (design), pending spec review
**Related:** member completion popup (udd-7fd, contextual autocomplete)

## Problem

The autocomplete popup for `obj:` / `obj.` / `$foo:` lists the queried object's verbs/
properties **plus everything inherited from its ancestors** (correct — you can call inherited
members on the object). All rows currently render with one icon per kind: verb = purple
lightning bolt, property = cyan tag. The user wants to distinguish members **defined on the
queried object** ("local") from members **inherited from an ancestor**, via an icon difference
that is usable by colorblind individuals, without changing the look of the local icons.

## Constraints

1. **`IMooWorldQueryProvider` has multiple implementations** (the MCP provider and SDWC). The
   interface and shared models must stay compatible with both. Only the MCP path can supply the
   local/inherited signal today; SDWC must keep working untouched.
2. **Summary listings carry no per-row defining-object data** (existing rule). `MooVerbSummary`/
   `MooPropertySummary` fill `DefiningObject` with the *queried* object as a placeholder; the true
   defining object lives only on the detail queries (`verb-info`/`doc`/`code` →
   `ResolvedObjectId`). We honor this: we add a *local-to-queried* flag, never an ancestor id, in
   a summary row.
3. **Colorblind usability.** The distinction must not rely on hue alone, nor on two shades of one
   hue (low contrast, fails CVD). The popup background is white, so icons must stay saturated.

## Approach (chosen)

Add a lightweight, optional **origin** signal to the shared summary models, supplied by the MCP
provider and defaulted to `Unknown` everywhere else. Inherited members render the **existing
local icon unchanged** plus a **gold up-chevron badge with a dark halo** in the top-right corner.
The badge — a structural, light/dark-contrasted glyph — is the colorblind-safe cue; the gold is
flavor. Local and `Unknown` items are pixel-identical to today.

The rich per-member defining object (`ResolvedObjectId`) is intentionally **not** added to
summaries; it remains available via the detail queries for a future on-hover tooltip.

## Components

### 1. Shared model — `Org.Edgerunner.Mud.Common/Querying/QueryModels.cs`

```csharp
/// <summary>Where a summarized member is defined relative to the queried object.</summary>
public enum MemberOrigin
{
   /// <summary>Provenance not supplied by the provider (e.g. SDWC). Renders as the local icon.</summary>
   Unknown = 0,
   /// <summary>Defined on the queried object itself.</summary>
   Local,
   /// <summary>Inherited from an ancestor of the queried object.</summary>
   Inherited
}

public record MooVerbSummary(IReadOnlyList<string> Aliases, MooObjectId DefiningObject,
                             MemberOrigin Origin = MemberOrigin.Unknown);

public record MooPropertySummary(string Name, MooObjectId DefiningObject,
                                 MemberOrigin Origin = MemberOrigin.Unknown);
```

The trailing defaulted parameter means **no `IMooWorldQueryProvider` method signature changes**
and every existing construction call site keeps compiling. `DefiningObject` is unchanged.

### 2. MCP provider — server package + mapping

**Wire (server `Server Packages/edgerunner-org-moo-query.moo`):** `handle_verbs` and `handle_props`
change their reply `d` array from `["name", …]` to **per-row `[["name", isLocal], …]`**, where
`isLocal` is `1` when the member was found on the queried object itself and `0` otherwise. The
parent-chain walk already visits the queried object first (`what == o`), so the flag is computed
for free — names collected on that first pass are local, the rest inherited. (The existing dedup
keeps the nearest definition, so a locally-overridden name is correctly `isLocal = 1`.)

**Mapping (`Org.Edgerunner.Mud.MCP/Packages/McpQueryMapping.cs`):** `MapVerbSummaries` /
`MapPropertySummaries` read each row tolerantly:
- row is `["name", 1|0]` → `Origin = Local | Inherited`;
- row is a bare `"name"` string (older server) → `Origin = Unknown`.

This keeps a new client working against an old server. `DefiningObject` continues to be the
queried id.

**Versioning (open item — decide at review):** changing the `d` row shape is a payload change.
Options: (a) keep package version `1.0` and rely on the tolerant mapper for mixed new-client/
old-server, accepting that a *new server + old client* would mis-parse; or (b) bump the package
to `1.1` and negotiate, with the client accepting both shapes. Recommendation: **(a)** — the
tolerant mapper covers the realistic upgrade order (client and server are co-developed and the
server is updated from this repo's dump), and it avoids negotiation complexity. Update
`docs/edgerunner-org-moo-query-protocol.md` and the package `description` to document the row
shape either way.

### 3. SDWC provider — mapping only

`Org.Edgerunner.Mud.Communication/Sdwc/SdwcMapping.cs` `MapVerbs`/`MapProperties` set
`Origin = MemberOrigin.Unknown` explicitly. No SDWC wire/transport change. (If the SDWC payload is
later found to carry provenance, this is the single place to map it.)

### 4. Icons — `CompletionIconCategory` + `CompletionIconFactory`

Append two categories so existing `ImageIndex` values are preserved:

```csharp
VerbInherited = 8,
PropertyInherited = 9
```

`CompletionIconFactory`:
- `VerbInherited` draws the existing `DrawVerb` (unchanged purple bolt) then the badge.
- `PropertyInherited` draws the existing `DrawProperty` (unchanged cyan tag) then the badge.
- New `DrawInheritedBadge(g, …)`: a small **up-chevron** in the top-right corner of the 24-unit
  space (around x≈16–22, y≈3–8), rendered as a **dark halo stroke** (thicker, `~#202530`) with a
  **gold stroke** (`~#FFC400`) on top — so the chevron's edges read on the colored icon body and
  on the white popup background. Stroke caps/joins rounded; sizes scale with the existing
  64/24 world transform.
- `CreateImageList` already enumerates all categories in order, so the two new icons are appended
  automatically at index 8 and 9.

### 5. Completion-item building — `MemberCompletionController`

`BuildVerbItems(IReadOnlyList<MooVerbSummary>)` and `BuildPropertyItems(...)` select the icon
category per item:
- verb: `Origin == Inherited` → `VerbInherited`, else `Verb` (covers `Local` and `Unknown`);
- property: `Origin == Inherited` → `PropertyInherited`, else `Property`
  (CoreReference properties keep `CoreReference`).

Note the current `BuildVerbItems` flattens aliases into a `SortedSet<string>` of individual
names; carrying `Origin` per emitted item means iterating verbs/properties with their `Origin`
rather than collapsing to a names-only set first. De-duplication and `*`-stripping behavior are
preserved; when the same name appears under two origins (shouldn't happen given server dedup),
`Local`/`Inherited` wins over `Unknown` deterministically.

## Behavior summary

| Source | Origin | Icon |
|---|---|---|
| SDWC (any) | `Unknown` | current verb/property icon (unchanged) |
| MCP, local member | `Local` | current verb/property icon (unchanged) |
| MCP, inherited member | `Inherited` | current icon + gold haloed up-chevron badge |

## Testing

- **Model:** default `Origin == Unknown`; record equality/with-expressions include `Origin`.
- **MCP mapping:** `[["a",1],["b",0]]` → `Local`/`Inherited`; bare `["a","b"]` (legacy) → `Unknown`;
  malformed rows degrade to `Unknown` without throwing.
- **SDWC mapping:** summaries come back with `Origin == Unknown`.
- **Icon factory:** `CreateImageList` yields 10 images; `VerbInherited`/`PropertyInherited` differ
  from `Verb`/`Property` (non-identical bitmaps); rendering does not throw.
- **Controller:** `BuildVerbItems`/`BuildPropertyItems` assign `VerbInherited`/`PropertyInherited`
  only for `Inherited`, and the current category for `Local`/`Unknown`.

## Out of scope

- The on-hover detail tooltip that would surface the real `ResolvedObjectId` (separate feature).
- Property detail `ResolvedObjectId` (the `prop-info` payload has no defining object today).
- Any change to SDWC's wire/transport.
