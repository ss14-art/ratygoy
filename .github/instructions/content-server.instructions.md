---
applyTo: 'Content.Server/**'
description: Server assembly guidance — authority, DB, BUI owning systems, spawn truth. Predicted logic stays in Shared.
---

# Content.Server

Runs **server only**. Authority: persistence, DB, spawning truth, admin, round
logic, irreversible side effects. Does not run on the client.

Read: `AGENTS.md` (root), `.agents/rules/ss14-client-server-shared.md`,
`.agents/rules/ss14-prediction.md`, `.agents/rules/ss14-databases-migrations.md`.

- Fork code: `Content.Server/_Art/<Feature>/...`, namespace
  `Content.Server._Art.<Feature>[.<Sub>]`.
- Server systems own BUIs: `Subs.BuiEvents<T>(uiKey, subs => {...})` +
  `_uiSystem.SetUiState(...)`.
- Predicted player interactions don't live here alone — the shared part is in
  `Content.Shared`; this system confirms + does irreversible effects.
- May `using Content.Shared.*`, `Robust.Server.*`. Cannot `using
  Content.Client.*`.
- DB schema changes need migrations in **both** Postgres and Sqlite
  (`ss14-databases-migrations.md`). Don't run the generator unless asked
  (`ss14-ai-workflow.md`).
- Server-only scratch state the client must never see → server-only component.
