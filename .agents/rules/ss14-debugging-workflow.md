# Rule: debugging — VV, logs, breakpoints

Don't flail. Use the tools SS14 gives you. This is how you find out *why*
something doesn't work without rebuilding blind.

## ViewVariables (VV)

The single most useful debugging tool. Any field marked `[ViewVariables]` (or
`[ViewVariables(VVAccess.ReadWrite)]`) is inspectable live in the running game
via the VV panel (default keybind: open VV on the entity under cursor / selected).

- Mark fields you'll want to inspect: `[ViewVariables] public int Foo;`.
- ReadWrite lets you mutate it live to test behavior without recompiling.
- Use VV to confirm component state, check predicted vs server state, watch a
  value change as you interact.
- Client `_menu = this.CreateWindow<...>()` fields are often marked
  `[ViewVariables]` to inspect the open menu (see the fork Market BUIs).

When a value "isn't what I expect", VV it before changing code.

## Logging

`Log` is available on every `EntitySystem` (`Log.Debug`, `Log.Info`,
`Log.Warning`, `Log.Error`). Use it, but:

- `Log.Error` — only for genuinely broken invariants the dev must fix (see
  `MarketManager` logging a missing `MarketStorageComponent`). Not for user
  errors.
- `Log.Warning` — recoverable but suspicious.
- `Log.Debug`/`Log.Info` — transient diagnostics. Remove or gate behind a
  verbose flag before merging; don't ship spammy logs.
- Log messages are **developer-facing English in code**, not localized — see
  `ss14-localization.md`. Don't put them in `.ftl`.

Pattern for a one-off trace while debugging:

```csharp
Log.Debug($"market create: seller={sellerName} lot={lotName} price={price}");
```

Remove it when the bug is fixed.

## Breakpoints / stepping

- The project runs under a normal .NET debugger (Rider/VS). Set breakpoints in
  your system handlers and attach to the client/server process.
- Client and server are separate processes — attach to the one whose code path
  you're debugging. Prediction bugs often need both.
- For prediction: breakpoint the **shared** handler and watch it fire on the
  client first (predicted), then on the server (authoritative).

## Reasoning the prediction path

When a predicted action misbehaves, the bug is usually one of:
1. Server-only handler (not predicted) → see `ss14-prediction.md`.
2. Missing `Dirty` → state not replicated → see `ss14-networking.md`.
3. Client mutating non-networked state → diverges silently.
Trace through both sides; VV the component on client and server and compare.

## What not to do

- Don't "fix" by rebuilding repeatedly with random changes. Read the code, VV
  the state, form a hypothesis, then change one thing.
- Don't leave `Log.Debug` spam in merged code.
- Don't ship `Log.Error` for expected runtime conditions (use a popup or
  silent no-op).

See `skills/ss14-debugging-workflow.md` for the extended walkthrough.
