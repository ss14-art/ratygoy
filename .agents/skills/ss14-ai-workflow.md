# Skill: AI agent workflow (extended)

**When to read:** you're an AI agent starting a task in this repo. The rule
`ss14-ai-workflow.md` is the short form; this is the full checklist with
reasoning.

## 0. Preflight

Run `ss14-skill-preflight-and-refresh.md`: read `AGENTS.md`, read the rules it
routes you to, read only the matching skills for the paths you'll touch. Don't
read everything — read what applies.

## 1. Classify the task

- **New feature?** → all relevant `_Art` layers (Shared component, Server
  system, Client BUI/XAML, Resources prototype + locale). Decide assemblies via
  `ss14-client-server-shared.md`.
- **Edit an upstream behavior?** → narrow + marked (`ss14-fork-editing.md`),
  prefer subscribing from `_Art` over editing.
- **Resources-only (prototype/locale/sprite/audio)?** → `Resources/` + the
  matching domain skill (`ss14-prototype-basics.md`, `ss14-audio.md`,
  `ss14-sprite-overlays-shaders.md`).
- **Test-only?** → `Content.Tests` / `Content.IntegrationTests` + the subtree
  `AGENTS.md` there.
- **Engine?** → escalate, don't edit (`ss14-engine-edits.md`).

## 2. Find the pattern to imitate

Before writing, `grep`/read the real code for the pattern:
- A similar feature in `_Art` (the Market is the reference feature for
  console/BUI/market-style logic).
- The upstream equivalent for the subsystem (e.g. `Content.Server/Cargo` for
  economy-adjacent logic, `Content.Server/Atmos` for atmos).

Confirm the API you're about to call actually exists in this codebase version.
Patterns drift.

## 3. Place code correctly

- New file → `_Art`, mirroring upstream path structure
  (`ss14-code-placement.md`, `ss14-fork-editing.md` → preserve path similarity).
- Component / BUI msg-state → Shared. System → Server (authority) or Shared
  (predicted). Menu → Client.
- Locale → `Resources/Locale/en-US/_Art/<feature>.ftl`, keys `fork-...`.
- Prototype → `Resources/Prototypes/_Art/...`.

## 4. Write the code

- Follow `ss14-code-style.md` (naming, `Entity<T>`, `On→Try→Can→Do`,
  `ProtoId<T>`).
- Prediction + `Dirty` + `NetEntity` where the feature networks
  (`ss14-prediction.md`, `ss14-networking.md`).
- Localize every player-facing string (`ss14-localization.md`).
- Upstream edits: every changed line marked `// ss14-art edit` (single) or
  wrapped `// ss14-art edit start` / `end` (block). YAML/XAML equivalents.

## 5. Do NOT build/run/test

Unless a human explicitly asked you to build/run/test, **don't**. Don't run
`dotnet build`, `RUN_THIS.py`, `runserver.bat/sh`, `runclient.bat/sh`, the test
runner, or launch the game. You don't need to compile to write correct code
here. Reason from the code + rules; the reviewer and CI handle mechanical
verification. See `ss14-ai-workflow.md` rule for the full why.

If you *were* asked to run something, run exactly that, report the output, and
don't generalize into "I'll just compile everything to be safe."

## 6. Describe the change in the PR

For each non-trivial change, write (in the PR description / your summary):

- **What** it does.
- **Where** it lives (which `_Art` folder/assembly) and why that assembly.
- **How** it works: event flow, predicted vs authoritative, what's networked
  and `Dirty`'d, locale keys added, prototype ids added.
- **What you didn't do** and why (e.g. "didn't edit upstream `StorageSystem`;
  subscribed from `_Art` instead").
- **Upstream edits**, if any: list the files and the `ss14-art edit` marker
  locations.

This narrative prevents the next agent/reviewer from guessing at intent. Code
+ description = the deliverable.

## 7. Self-check (the gate before you finish)

- [ ] No build/run/test (unless explicitly asked).
- [ ] Every upstream-file change has a `ss14-art edit` marker.
- [ ] Fork-only code in `_Art`, not upstream files.
- [ ] Prediction + networking + localization handled where needed.
- [ ] `_Art` paths mirror upstream structure.
- [ ] PR describes what/where/how for each non-trivial change.
- [ ] No drive-by refactors / unrelated fixes mixed in.
- [ ] No edits under `RobustToolbox/`.

## Anti-patterns

- "I'll just compile to check" — no. Reason from code + rules.
- Dropping a verbatim upstream/fork copy into `_Art` without adapting
  conventions — adapt it (`ss14-porting-and-licensing.md`).
- Editing an upstream system's internals when subscribing from `_Art` would do.
- A server-only handler for a player action (no prediction).
- Raw English in a popup/entity name.
- Unmarked upstream edits.
