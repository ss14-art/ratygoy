# Content.Server — subtree router

`Content.Server` runs **server only**. Authority: persistence, DB, spawning
truth, admin, round logic, irreversible side effects. Does not run on the
client.

## Read first

- Root `AGENTS.md` (router).
- `.agents/rules/ss14-client-server-shared.md` — when Server vs Shared.
- `.agents/rules/ss14-prediction.md` — don't put predicted logic here.
- `.agents/rules/ss14-networking.md` — `Dirty` from here replicates to clients.
- `.agents/rules/ss14-databases-migrations.md` — persistence.
- `.agents/rules/ss14-ecs-basics.md`, `ss14-code-style.md`.

## Fork code here

- `Content.Server/_Art/<Feature>/...`, namespace `Content.Server._Art.<Feature>[.<Sub>]`.
- Server systems that own BUIs (`Subs.BuiEvents<T>` + `UserInterfaceSystem`),
  server-only authority logic, DB access, spawning-truth, admin actions.

## Domain skills that apply in Server

- `ss14-ui-bui.md` — the **Server** half of a BUI (owning system, `SetUiState`).
- `ss14-atmos.md` — the authoritative atmos systems live here.
- `ss14-databases-migrations.md` — `Content.Server.Database` is server-side
  persistence.
- `ss14-npc-ai.md` — NPC/HTN logic is server-authoritative.
- `ss14-transform-physics.md` — server-side teleport/attach/spawn.
- `ss14-audio.md` — `PlayPvs`/`PlayGlobal` are typically called server-side.

## Hard rules specific to Server

- **Predicted player interactions don't live here alone.** If a player action
  should feel responsive, the shared part is in `Content.Shared`; the server
  system confirms + does irreversible effects. A server-only handler for a
  player action = no prediction = laggy.
- May `using Content.Shared.*` and `Robust.Server.*`. **Cannot** `using
  Content.Client.*`.
- Server-only scratch state that the client must never see → server-only
  component, or a non-replicated field, not a Shared replicated field.
- DB schema changes need migrations in **both** Postgres and Sqlite
  (`ss14-databases-migrations.md`). Don't run the generator unless asked
  (`ss14-ai-workflow.md`).

## Common mistakes

- Putting the BUI messages/state here instead of Shared → client can't
  deserialize.
- Server-only handler for a player action → not predicted.
- Calling a `Content.Client.*` type → won't compile in the server build.
- Editing an upstream `Content.Server` system's internals when subscribing
  from `_Art` would do (`ss14-fork-editing.md`).
