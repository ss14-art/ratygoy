# Rule: where code goes (fork vs upstream)

This is the single most important rule in the repo. Get this wrong and the rest
doesn't matter.

## The fork model

This repository is a **fork** of Space Station 14. Almost every file is
**upstream code** that we track from space-wizards/space-station-14. Only a
small, well-marked subset is **fork code** (ours).

- **Upstream code** = everything outside the `_Art` folders (see below). Treat it
  as read-mostly. Edits are allowed but must be minimal and **marked** (see
  `ss14-fork-editing.md`).
- **Fork code** = code that lives inside `_Art` folders and uses the
  `Content.<Assembly>._Art` namespace. New fork systems/files **always** go here,
  never in upstream files.

## The `_Art` folders

Fork code lives in these four locations and **only** these:

| Assembly / area | Fork folder | Namespace root |
|---|---|---|
| `Content.Shared` | `Content.Shared/_Art/...` | `Content.Shared._Art.*` |
| `Content.Server` | `Content.Server/_Art/...` | `Content.Server._Art.*` |
| `Content.Client` | `Content.Client/_Art/...` | `Content.Client._Art.*` |
| Prototypes / data | `Resources/Prototypes/_Art/...` | (YAML) |
| Locale (strings) | `Resources/Locale/en-US/_Art/...` | `.ftl` files |

Mirror the upstream folder layout *inside* `_Art`. If upstream puts systems in
`Content.Server/Foo/Systems/`, the fork equivalent is
`Content.Server/_Art/Foo/Systems/`. Keep path similarity — it makes merges and
review easier. See `ss14-fork-editing.md` → "preserve path similarity".

## Decision: new system / new file

When you need to add a system, component, prototype, locale file, or any new
code unit:

1. **Check whether `_Art` already has a related folder.** Search
   `Content.<Assembly>/_Art/` and `Resources/Prototypes/_Art/` first.
2. **If yes** → add to it, mirroring the upstream sub-structure.
3. **If no** → create the folder under `_Art` that mirrors where upstream would
   put the equivalent. Do **not** create the file in an upstream directory.
4. **Never** put new fork-only code in an upstream file or upstream folder.

Example: a new "Auction" gameplay system →
`Content.Server/_Art/Auction/AuctionSystem.cs`,
`Content.Shared/_Art/Auction/Component/AuctionComponent.cs`, prototype in
`Resources/Prototypes/_Art/auction.yml`, strings in
`Resources/Locale/en-US/_Art/auction.ftl`.

## Decision: shared vs server vs client

This is the **second** most common mistake. See `ss14-client-server-shared.md`
for the full version; the short form:

- Logic that must run on **both** client and server (prediction, shared state,
  components, BUI messages/state, events) → `Content.Shared/_Art/...`
- Authority / persistence / side effects (DB, spawning, admin, round logic) →
  `Content.Server/_Art/...`
- Pure presentation (XAML menus, client-only visuals, input wiring) →
  `Content.Client/_Art/...`

Components and `NetSerializable` BUI state/messages **always** live in
`Content.Shared` so both sides agree. The server system and the client BUI both
reference them.

## Decision: engine vs content vs fork

- **Engine** = `RobustToolbox/`. Do **not** edit. If you think you must, see
  `ss14-engine-edits.md` — it requires escalation, not a quiet edit.
- **Content (upstream)** = `Content.*/` outside `_Art`. Edit only with narrow,
  marked diffs (see `ss14-fork-editing.md`).
- **Fork** = `_Art` folders. Edit freely, normal SS14 conventions apply.

## What never goes in upstream files

- New fork-only components, systems, prototypes, or their registrations.
- Fork-only `using`/namespace pollution that isn't required by an inline edit.
- Fork-only localization keys referenced only by fork code.
- Anything that could instead live in `_Art` and subscribe/hook upstream events.

If you find yourself wanting to add more than a few marked lines to an upstream
file, stop: the fork-only part belongs in `_Art`, and the upstream file gets only
the smallest possible hook (e.g. raising an event, a `partial` extension), if it
needs one at all. Often it needs nothing — you can subscribe to existing
upstream events from `_Art` code.
