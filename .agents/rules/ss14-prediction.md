# Rule: prediction

SS14 is networked with **client-side prediction**. The client doesn't wait for
the server to act; it predicts the result locally, and the server later
confirms or corrects. Code that ignores prediction feels laggy, jitters, or
desyncs. **Every gameplay interaction must support prediction.** This is
mandatory, not optional.

## The short mental model

1. Client input → a **shared** event fires on the client **and** (after round
   trip) on the server.
2. The **shared** system handles the event identically on both sides → the
   client shows the predicted result instantly.
3. The server is authority. If the prediction was wrong, the server's state
   wins and the client corrects (a "mispredict"). Mis predicts are normal and
   fine; **not predicting at all** is the bug.
4. State that must be shown to the player (UI counts, visuals) is replicated via
   networked components / `Dirty`, so the client's view converges to the
   server's truth.

## What this means for where code lives

Prediction runs on **both** client and server from the **same** code. Therefore:

- The interaction logic lives in **`Content.Shared/_Art/...`**, in a shared
  system (`EntitySystem` in the Shared assembly). Both `Content.Server` and
  `Content.Client` reference it.
- The component whose state is predicted lives in `Content.Shared`.
- Server-only authority bits (persistence, DB, spawning, admin) stay in
  `Content.Server/_Art/...` and are split out as server-only overrides/addons.

If your interaction logic is in `Content.Server` only, it is **not predicted**
and is wrong by default. See `ss14-client-server-shared.md`.

## Patterns

- Subscribe to the interaction event (`AfterInteractEvent`,
  `InteractHandEvent`, etc.) in the **shared** system.
- Do the shared work (state change on the component) in the shared handler.
- `Dirty(uid, comp)` after mutating a networked component so the change
  replicates. See `ss14-networking.md`.
- For server-only consequences (e.g. actually deleting an item, spending
  currency, writing DB), do that in a **server** system subscribing to the same
   event, or in a server-only event the shared system raises. Keep the
  client-visible result predicted, the irreversible side-effect authoritative.
- Use `INetSpawn`/spawn prediction helpers and the existing predicted
  interaction systems as references; don't invent a new prediction scheme.

## What "support prediction" requires of you

- [ ] Interaction logic is in `Content.Shared` (shared system), not
      server-only.
- [ ] The component state the player sees is networked and `Dirty`'d on change.
- [ ] Server is the authority for irreversible effects; client predicts the
      reversible visible state.
- [ ] You tested (or at least reasoned through) the mispredict path: when the
      server disagrees, does the client correct cleanly without duped/deleted
      entities or stuck UI?

## Common failure modes to avoid

- **Server-only handler** → action is delayed by ping; feels bad. Move it to
  shared.
- **Mutating non-networked component state on the client** → client and server
  diverge silently. Only networked (`[RegisterComponent]` + replicated) state
  converges.
- **Spawning/deleting entities only on the server** for a predicted action →
  the client doesn't show it until the round trip. Use predicted spawn/delete.
- **Forgetting `Dirty`** → other clients never see the change.

## Where to read more

- `skills/ss14-client-server-shared.md` — the shared/server/client split in
  depth.
- `ss14-networking.md` — `NetEntity`, `Dirty`, networked components.
- `skills/ss14-common-api-patterns.md` — predicted interaction patterns.
