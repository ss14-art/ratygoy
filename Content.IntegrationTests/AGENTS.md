# Content.IntegrationTests — subtree router

`Content.IntegrationTests` spins a real **server + client pair** via
`PoolManager`. This is where tests that need the actual sim, networking,
prediction, or a running entity system live. Slower than unit tests; use when a
unit test can't exercise the behavior.

## Read first

- Root `AGENTS.md` (router).
- `.agents/rules/ss14-prediction.md` — integration tests are how you exercise
  the predicted path server+client together.
- `.agents/rules/ss14-networking.md` — replicate/`Dirty` behavior is testable
  here.
- `.agents/skills/ss14-examples.md` — Example 6.
- `.agents/rules/ss14-ai-workflow.md` — **don't run the tests unless asked**.

## Fork tests here

- `Content.IntegrationTests/Tests/_Art/<Feature>/...`, namespace
  `Content.IntegrationTests.Tests._Art.<Feature>`.
- Mirror the upstream `Tests/<Subsystem>/` layout.

## Patterns (from the real codebase)

- **`InteractionTest`** base class (`Content.IntegrationTests/Tests/Interaction/`)
  is the workhorse for interaction scenarios — see
  `Tests/Vending/VendingInteractionTest.cs`. It provides `Player`, `Target`,
  `Server`, `Client`, spawn/assert helpers, and an interaction flow.
- **`[TestPrototypes]`** with a `private const string TestPrototypes = $@"..."`
  inline YAML block defines test-only prototypes (entity/prototype ids). The
  attribute registers them for the test. Use fork-named ids
  (e.g. `ForkInteractionTest...`) to avoid collisions.
- **`PoolManager`** / `PoolSettings` acquire a server+client pair
  (`PoolManager.GetPairAsync(...)`). `InteractionTest` wraps this; drop to raw
  `PoolManager` only when you need non-default settings (no client, specific
  cvars, etc.).
- **`ProtoId<T>`** for prototype references in test code (typed).
- Assert via `Assert.That`. For prediction tests, assert on both client and
  server state.

## Hard rules

- **Don't run the integration tests to validate** (`ss14-ai-workflow.md`).
  They're slow; write the test, reason about it, describe expected behavior.
- Don't edit upstream integration tests; add fork tests under `_Art` (or
  `Tests/_Art`).
- Test prototypes use `[TestPrototypes]` inline YAML, fork-named ids.
- Mark tests `[Test]`; group with `[TestFixture]`. Don't leave tests that
  require manual setup (they must run headless in CI).

## Common mistakes

- Spawning a real prototype id that collides with upstream → use
  `[TestPrototypes]` with a fork-named id.
- Asserting only on server state for a predicted action → also assert the
  client's predicted state converged.
- A test that depends on wall-clock timing → flaky in CI. Use the sim's tick
  stepping / `RunTicks` helpers, not `Task.Delay`.
- Editing an upstream test for a fork case → add a fork test instead.
- Forgetting the test needs a client pair (predicted) and using a server-only
  pool → prediction path isn't exercised.

## Where to look

- `Content.IntegrationTests/PoolManager.cs`, `PoolSettings.cs`.
- `Content.IntegrationTests/Tests/Interaction/` (the `InteractionTest` base).
- `Content.IntegrationTests/Tests/Vending/VendingInteractionTest.cs` — a full
  `[TestPrototypes]` + interaction example to mirror.
