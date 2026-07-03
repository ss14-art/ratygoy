# Content.Tests — subtree router

`Content.Tests` is the **unit test** project (NUnit, no server/client pair).
Fast, in-process, no real game sim. Use for pure logic and client-side
constructs that don't need a running server.

## Read first

- Root `AGENTS.md` (router).
- `.agents/rules/ss14-ecs-basics.md` — most tests exercise systems/components.
- `.agents/skills/ss14-examples.md` — Example 6 (test anchors).
- `.agents/rules/ss14-ai-workflow.md` — **don't run the test runner unless
  asked**; write the test, describe it, let a human/CI run it.

## Fork tests here

- `Content.Tests/_Art/<Feature>/...`, namespace `Content.Tests._Art.<Feature>`.
- Mirror the upstream test folder layout.

## Patterns

- NUnit: `[TestFixture]` (often implicit), `[Test]`, `Assert.That(actual,
  Is.EqualTo(expected))`.
- Base classes: `ContentUnitTest` for content unit tests, plus subsystem-specific
  base classes. Read an existing test in the same subsystem before writing.
- For logic that needs an `EntityManager`/systems, prefer an **integration
  test** (`Content.IntegrationTests`) — unit tests can't easily spin the full
  sim. Use unit tests for pure logic, parsers, math, serializers.

## Hard rules

- **Don't build/run tests to validate** (`ss14-ai-workflow.md`). Write the test,
  reason about correctness, describe expected behavior in the PR.
- Don't edit upstream tests; add fork tests under `_Art`.
- Don't write a unit test that hand-rolls `EntityManager` for sim-dependent
  logic — that belongs in `Content.IntegrationTests`.

## Common mistakes

- A "unit" test that needs a real server → move to integration tests.
- Asserting on timing/ordering that's nondeterministic → flaky.
- Editing an upstream test to make a fork case pass → add a fork test instead.
