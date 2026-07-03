# Skill: atmos

**When to read:** you're touching gas mixtures, pipes, reactions, tile
atmosphere, pressure, temperature, or gas containers.

## Where atmos lives

- `Content.Shared/Atmos/` — shared components, `GasMixture`, constants
  (`Atmospherics`), reagent/atmos reactions shared.
- `Content.Server/Atmos/` — the authoritative systems:
  `EntitySystems/` (gas stuff), `Reactions/` (gas→gas/reagent reactions),
  `Piping/`, `Monitor/`, `Portable/`, `Rotting/`.
- Fork atmos code → `Content.{Shared,Server}/_Art/Atmos/...`.

Atmos is **server-authoritative** (the sim runs on the server; clients get
visuals). Most atmos logic is `Content.Server`. Don't try to predict gas
mixtures client-side.

## `GasMixture`

The core data. Don't mutate raw arrays; use the `GasMixture` API and the
atmos systems (`AtmosphereSystem`, `GasMixtureSystem`). Gases are integer IDs
(`Gas.Oxygen`, `Gas.Nitrogen`, …) with `Atmospherics` constants for amounts.

- `GetMoles(gas)`, `SetMoles(gas, moles)`, `AdjustMoles(gas, delta)`.
- `Temperature`, `Pressure`, `Volume`.
- Reactions: `Content.Server/Atmos/Reactions/` — each reaction is a class; the
  sim runs them per-tick per mixture. Fork reactions go in `_Art`.

## Components

- `GasMixtureHolder`/atmos device components for things that contain gas.
- `Pipe`/`AtmosDevice` components for piping networks.
- Read the existing component before adding a fork one — most atmos needs are
  met by composing existing components.

## Reactions (gas + reagent)

A gas reaction is a class under `Reactions/`. To add a fork reaction, mirror
the structure in `Content.Server/_Art/Atmos/Reactions/`, register it, and keep
it narrow. Reagent-atmos effects use `!type:...` effect entries in reagent
prototypes (see `ss14-examples.md` Example 3) — fork reagent effects go in the
fork reagent YAML + any effect class in `_Art`.

## Tiles & maps

Tile atmosphere is managed by `AtmosphereSystem` per-map. Don't hand-edit tile
gas; use the system API. The fork Market stores its storage on the map entity
(`MarketStorageComponent` on the default map) — a similar "per-map singleton"
pattern is common for atmos-adjacent global state.

## Pitfalls

- **Client-side atmos logic** — it won't match the server. Keep atmos
  authoritative server-side; surface visuals via networked components.
- **Mutating `GasMixture` internals directly** — use the API or you corrupt the
  sim.
- **Forgetting `Dirty`** on a networked atmos component the client visualizes.
- **New gas ID without registering** — gases are an enum + prototype; adding one
  is a non-trivial change, prefer composing existing gases.
- **Performance** — atmos is hot. Don't run per-entity per-tick work you can
  defer; reuse the sim's batching.

## Where to look

- `Content.Server/Atmos/EntitySystems/`, `Content.Server/Atmos/Reactions/`.
- `Content.Shared/Atmos/` for `GasMixture`, `Atmospherics`, shared components.
- Rule reminder: server-only is fine for atmos (authority), but networked
  *visuals* the client shows must be Shared components + `Dirty`.
