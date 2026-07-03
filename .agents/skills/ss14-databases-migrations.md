# Skill: databases / migrations

**When to read:** you're persisting server state across rounds/restarts
(player data, bans, admin, character profiles, economy, records) and need a
DB schema change.

## What persists and where

The server database (`Content.Server.Database`) is **EF Core** with two
providers: **Postgres** (production) and **Sqlite** (dev/test). Migrations are
**provider-specific** and live in:

- `Content.Server.Database/Migrations/Postgres/`
- `Content.Server.Database/Migrations/Sqlite/`

A migration is a `<timestamp>_<Name>.cs` + `.Designer.cs` pair. Both provider
trees must stay in sync for a schema change (you add the migration to **each**
provider's folder, because EF keeps separate histories per provider).

## Models

DB model classes are in `Content.Server.Database/Model*.cs` (`Model.cs`,
`Model.Ban.cs`, `ModelPostgres.cs`, `ModelSqlite.cs`). The `ServerDbContext`
(`ServerDbBase`/provider contexts) maps them. To add a persisted entity:

1. Add a `DbSet<Foo>` + model class in a `Model*.cs` (fork: prefer a
   `Content.Server.Database/_Art/Model.Fork.cs` if the project layout permits a
   fork folder; otherwise a fork `partial` extension — keep it isolated and
   marked). Check whether a fork `_Art` subfolder is accepted by the context
   before assuming.
2. Add the migration to **both** `Postgres/` and `Sqlite/`.
3. Reference upstream's `SnakeCaseNaming` for column naming conventions where
   the project uses it.

## Adding a migration (the mechanics)

There are helper scripts: `Content.Server.Database/add-migration.sh` /
`add-migration.ps1`. They wrap `dotnet ef migrations add`. Use them rather than
hand-writing migration files. **Do not** generate/run migrations unless a human
asks you to — see `ss14-ai-workflow.md` (no build/run). You can *write* the
model + describe the needed migration in the PR; a human runs the generator.
If asked to run it, run the helper for both providers.

## What goes in the DB

- **Server-authoritative, cross-round, cross-session** data: player records,
  bans, admin ranks, character profiles, round metadata, persistence-style
  records.
- **Not**: transient gameplay state (that's components, not DB), predicted
  state, client data.

If a feature's data must survive a server restart and isn't just map-defined,
it's a DB concern. If it's per-round/per-entity, it's an ECS component.

## Fork DB considerations

- This repo also has `_Persistence14` (a persistence fork) present as a
  dependency. Don't assume its schema is yours to edit — treat it like upstream
  (read-mostly). Fork persistence for *our* features goes in our model classes.
- If your feature needs cross-round persistence, check whether an existing
  fork/`_NF` table already stores it before adding a new table. The Market
  currently stores listings in an in-map component (`MarketStorageComponent`),
  not the DB — so a round-reset clears it. That's a deliberate choice; if you
  need persistence, that's a different design and a DB table.

## Pitfalls

- **Adding a migration to only one provider** — the other build/deploy breaks.
  Always both.
- **Renaming a column without a migration** — the schema and model desync;
  runtime errors. Schema changes *require* a migration.
- **Heavy writes in a hot path** — DB I/O is slow; batch/cache where you can.
- **Storing per-round gameplay state in the DB** that should be a component —
  the DB is for cross-session data.
- **Hand-writing migration `.cs`** instead of the generator — drift from the
  model. Use `add-migration.*`.

## Where to look

- `Content.Server.Database/Model.cs`, `Model.Ban.cs`, `ModelPostgres.cs`,
  `ModelSqlite.cs`.
- `Content.Server.Database/Migrations/Postgres/` and `.../Sqlite/` — read an
  existing migration to see the shape.
- `add-migration.sh` / `add-migration.ps1` for the generator entry point.
- Rule: `ss14-ai-workflow.md` (don't run the generator unless asked).
