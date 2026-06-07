# Design: Per-user first-run seeding of default data files

**Date:** 2026-06-07
**Status:** Approved (design)
**Related beads:** none yet (implementation plan to follow)

## Problem

Udditor stores per-user data in `%APPDATA%\Moo Udditor` (the Windows Roaming
AppData folder, which is per Windows user profile). The Inno Setup installer runs
once and seeds **only the installing user's** appdata with the default
`Moo.Editor.config` (plus the Darkmode example) and `Snippets.txt`. Because we
also stopped shipping those defaults into the shared application directory,
`ApplicationPaths.ResolveDataFile`'s base-directory fallback finds nothing for a
second user.

Result: a different Windows user on the same machine who launches
`Moo Udditor.exe` from Program Files gets **no default config and no snippets**.

The underlying mistake was conflating two concerns:

- **Defaults** must be shared across all users (read).
- **User customizations** must be per-user (write).

Both `Moo.Editor.config` and `Snippets.txt` are intended to be edited by each
user (via the upcoming Options/Theme dialogs *and* by hand), so each user needs
their own writable copy seeded from an app-provided default.

## Decisions

1. **Eager seed-on-first-run** (not lazy copy-on-write). On startup, if a
   managed file is missing from the user's appdata, create it from the
   app-provided default. Runtime read/write logic then always targets appdata.
2. **Embedded resources** are the source of the defaults (not files shipped to a
   shared on-disk location). This removes the shared-file dependency that caused
   the bug, gives a single source of truth in the binary, and avoids installer
   file-placement and Program Files / ProgramData permission concerns.
3. **Seed-if-missing only** — never overwrite an existing user file. The user's
   copy is sacred.

## Scope

**Managed (seeded) files:**

- `Moo.Editor.config`
- `Moo.Editor.Darkmode.Example.config`
- `Snippets.txt`

**Not in scope:**

- `Worlds.xml` — pure user data, no shipped default; created on save. Unchanged.
- `nlog.config` — stays installed in `{app}` and is read from there; only its log
  *output* is directed to appdata. Unchanged.

## Components

1. **Embedded default resources.** The three files become `EmbeddedResource`s in
   the `Org.Edgerunner.Moo.Udditor` assembly, co-located with the seeder and
   `ApplicationPaths`. They are no longer loose content copied to the build output
   directory. (They currently live as `None`/content in
   `Org.Edgerunner.Moo.Editor`; they move into the Udditor project as embedded
   resources.)

2. **`ApplicationDataSeeder` (new, in the Udditor project).**
   - Public entry point: `EnsureSeeded()`.
   - Holds a manifest mapping each embedded resource logical name → target file
     name (e.g. `Moo.Editor.config`, `Moo.Editor.Darkmode.Example.config`,
     `Snippets.txt`).
   - Ensures `%APPDATA%\Moo Udditor` exists (reusing
     `ApplicationPaths.AppDataFolder`).
   - For each manifest entry: if the target file does **not** exist, extract the
     embedded resource stream and write it. If it exists, skip (never overwrite).
   - Each file operation is wrapped in `try/catch`; failures are logged via NLog
     and are **non-fatal**.
   - Implemented so the seeding routine accepts a target folder + manifest, making
     it exercisable against a temporary directory if desired (see Testing).

3. **`Program.Main` calls `EnsureSeeded()` first** — at the very start of the
   startup `try` block, **before** `Settings.Instance.LoadFrom(...)`, so the
   config file is present when the settings loader runs.

4. **`ApplicationPaths` unchanged.** Reads continue through `ResolveDataFile`
   (which now reliably finds the seeded appdata copy; the base-directory fallback
   remains as a harmless safety net for dev/portable runs). Writes continue
   through `GetWritableDataFile`.

5. **Installer cleanup (`Installer\Udditor.iss`).** Remove the three loose-file
   deployment lines for `Moo.Editor.config`, `Moo.Editor.Darkmode.Example.config`,
   and `Snippets.txt`. The installer no longer writes to any user's appdata. The
   `nlog.config` line (to `{app}`) stays.

## Data flow (startup)

```
App start
  -> ApplicationDataSeeder.EnsureSeeded()
       - ensure %APPDATA%\Moo Udditor exists
       - for each (embedded resource -> file name):
            if target missing: write it; else skip
  -> Settings.Instance.LoadFrom(ApplicationPaths.ResolveDataFile("Moo.Editor.config"))
       - reads the seeded appdata copy
  -> (later) MooCodeEditorPage loads Snippets via
       Snippets.LoadSnippets(ApplicationPaths.ResolveDataFile("Snippets.txt"))
       - reads the seeded appdata copy
```

## Error handling

Seeding is best-effort. If the folder cannot be created or a file cannot be
written, log a warning and continue. Downstream degrades gracefully:

- Config: the `Settings` loader falls back to its coded defaults for any missing
  file or key.
- Snippets: load as an empty set.

The application never crashes due to a seeding failure.

## Multi-user outcome

With embedded defaults plus first-run seeding, **every** Windows user — the
installing user and any other user on the machine — gets their own seeded,
writable copies the first time they launch Udditor. The installer is no longer
involved in per-user data at all.

## Out of scope / future enhancements

- **Delivering new built-in *snippets* to existing users on upgrade.** Per the
  seed-if-missing decision, existing files are never overwritten, so new built-in
  snippets will not reach a user who already has a `Snippets.txt`. The config
  loader's per-key default fallback means new *config* keys are handled
  gracefully, so this gap is specific to snippets. A future enhancement could keep
  always-loaded embedded built-in snippets merged with a user snippets file. Not
  built now.

## Testing

Per decision, **no unit tests** for this piece given its scope. Manual
verification:

1. Remove (or rename) `%APPDATA%\Moo Udditor`, launch Udditor, confirm the three
   files are created and the editor loads config + snippets normally.
2. Edit a seeded file (e.g., add a snippet), relaunch, confirm the edit is
   preserved (file not overwritten).
3. (Optional) Launch under a second Windows user account and confirm that user
   also gets seeded files.
