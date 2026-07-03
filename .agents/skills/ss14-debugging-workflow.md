# Skill: debugging workflow (extended)

**When to read:** something doesn't work and you need to find out why without
flailing. The rule `ss14-debugging-workflow.md` is the short form.

## First: form a hypothesis

Don't change code to "see what happens." Read the code path, state a hypothesis
("the `Dirty` is missing so the client never sees the new listing count"), then
verify that specific hypothesis with one of the tools below. Change one thing.

## ViewVariables (VV) — the primary tool

Any `[ViewVariables]` field is live-inspectable. In a running game, open the VV
panel on an entity (keybind; check the active input context) and read/mutate
fields.

- Add `[ViewVariables]` to fields you're debugging. `[ViewVariables(VVAccess.ReadWrite)]`
  to mutate them live.
- Compare the same component's state on **client** vs **server** for prediction
  bugs — open VV on both, watch a value diverge or fail to converge.
- The fork Market BUIs mark `_menu` as `[ViewVariables]` so you can inspect the
  open menu object.

```csharp
[ViewVariables] private MarketRequestMenu? _menu;
```

## Logging

`Log` on every `EntitySystem`:

- `Log.Debug` / `Log.Info` — temporary traces. Remove before merge.
- `Log.Warning` — recoverable anomaly.
- `Log.Error` — broken invariant a dev must fix. The fork `MarketManager` uses
  this for a missing `MarketStorageComponent`:

  ```csharp
  Log.Error("MarketManager: MarketStorageComponent not found on map entity! " +
            "Add EnsureComp<MarketStorageComponent> in GameTicker.RoundFlow.cs");
  ```

  Note it *also tells the dev where to fix it* — good error messages point at
  the remedy, not just the symptom.

- Log strings are **developer English in code**, not localized.

## Breakpoints

- Rider/VS attach to the client or server .NET process. Client and server are
  **separate processes** — attach to the one whose path you're debugging.
- For prediction: breakpoint the shared handler; it fires on the client first
  (predicted), then the server (authoritative). Watching both shows you where
  they diverge.
- Step through `On → Try → Can → Do` to find which precondition fails.

## The prediction debugging checklist

A predicted action misbehaves → check in order:

1. **Is the handler in Shared?** A Server-only handler isn't predicted
   (`ss14-prediction.md`). This is the most common cause.
2. **Is the mutated state networked + `Dirty`'d?** Missing `Dirty` → clients
   never get the update (`ss14-networking.md`).
3. **Is the client mutating non-networked state?** It diverges silently. Only
   replicated component state converges.
4. **Are spawn/delete calls predicted?** Server-only spawns show late.
5. **Are entities in messages `NetEntity`?** Raw `EntityUid` over the wire is
   wrong on the client.

VV the component on both sides and compare. The divergence tells you which.

## Common "it silently doesn't work" causes

- Missing `Dirty` after a replicated mutation.
- BUI message/state not `[Serializable, NetSerializable]` → silently not sent
  or deserialized wrong.
- Component in the wrong assembly → type missing on one side.
- `TryComp`/`HasComp` on a uid that doesn't have the component (e.g. checking
  the player's component when you meant the console's).
- Event handler subscribed to the wrong event / wrong component type param.
- YAML key camelCasing wrong (`MaxListings` vs `maxListings`) → silently uses
  the default.

## What not to do

- Don't rebuild repeatedly with random edits. One hypothesis, one change.
- Don't ship `Log.Debug` spam.
- Don't `Log.Error` for expected user error (use a popup / silent no-op).
- Don't "fix" prediction by removing it (making the handler server-only) —
  that trades a bug for a worse bug.

## Where to look

- Rule: `.agents/rules/ss14-debugging-workflow.md`.
- Networking rule: `ss14-networking.md`. Prediction rule: `ss14-prediction.md`.
