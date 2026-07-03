# AGENTS.md — router for AI agents and review bots

You are working in **hakumai** (ss14-art), a fork of Space Station 14. This
file routes you to the canonical rules and skills. **Read it first.**

## The one-paragraph model

This is an SS14 fork. **Fork code lives in `_Art` folders** —
`Content.Shared/_Art`, `Content.Server/_Art`, `Content.Client/_Art`,
`Resources/Prototypes/_Art`, `Resources/Locale/en-US/_Art`,
`Resources/Textures/_Art`, `Resources/Audio/_Art`. **Everything outside `_Art`
is upstream code** (read-mostly). Engine code is `RobustToolbox/` (don't edit).
New fork systems/files go in `_Art`, never in upstream files. Edits to upstream
files must be minimal and marked with `// ss14-art edit` (single line) or
`// ss14-art edit start` / `// ss14-art edit end` (block). **Do not build, run,
or test the project** to validate your code — reason from the code + rules, and
describe your change in the PR.

## Step 1 — read the rules (mandatory, short)

The rules are the source of truth. Read the ones your task touches. All live in
`.agents/rules/`:

| Rule | Read when |
|---|---|
| `ss14-code-placement.md` | **always** — fork vs upstream, where code goes |
| `ss14-fork-editing.md` | **always** — `ss14-art edit` markers, narrow diffs |
| `ss14-ai-workflow.md` | **always** — no build/run, describe in PR, narrow diffs |
| `ss14-engine-edits.md` | before touching `RobustToolbox/` |
| `ss14-client-server-shared.md` | deciding Shared vs Server vs Client |
| `ss14-ecs-basics.md` | writing any component/system |
| `ss14-prediction.md` | any player interaction / networked state |
| `ss14-networking.md` | `NetEntity`, `Dirty`, networked components |
| `ss14-localization.md` | any player-readable string |
| `ss14-code-style.md` | naming, `Entity<T>`, `On→Try→Can→Do`, `ProtoId<T>` |
| `ss14-debugging-workflow.md` | debugging (VV, logs, breakpoints) |
| `ss14-skill-preflight-and-refresh.md` | how to pick skills + keep canon fresh |
| `ss14-bridges.md` | maintaining the tool adapters |

## Step 2 — load the matching skills (deeper, on demand)

Skills live in `.agents/skills/`. Read only the ones for the subsystems you're
touching. **Onboarding** (read if unsure of basics):

- `ss14-prototype-basics.md` — prototype YAML
- `ss14-ecs-basics.md` — ECS walkthrough
- `ss14-client-server-shared.md` — the assembly split in depth
- `ss14-debugging-workflow.md` — debugging walkthrough
- `ss14-common-api-patterns.md` — popups/audio/prototypes/queries/containers
- `ss14-porting-and-licensing.md` — porting from other forks/upstream
- `ss14-ai-workflow.md` — full agent workflow
- `ss14-examples.md` — worked examples (component+system, proto, reagent,
  GenericVisualizer, locale, test anchors, the upstream-edit non-example)

**Domain** (read the matching one before editing that subsystem):

| Path / subsystem | Skill |
|---|---|
| `**/Audio/**`, `Resources/Audio/**`, sound playback | `ss14-audio.md` |
| `**/Atmos/**`, gas/pipe/reaction/tile atmosphere | `ss14-atmos.md` |
| `**/Movement/**`, `**/Physics/**`, transform, coords, collision | `ss14-transform-physics.md` |
| PVs, entity visibility, "distant player can't see X" | `ss14-pvs.md` |
| `**/BUI/**`, machine/console UIs | `ss14-ui-bui.md` |
| `*.xaml`, client menus/windows | `ss14-ui-xaml.md` |
| `**/EUI/**`, admin/player panels (non-entity) | `ss14-ui-eui.md` |
| `**/Sprite/**`, `**/Overlays/**`, `*.swsl`, `GenericVisualizer` | `ss14-sprite-overlays-shaders.md` |
| `Content.Server.Database/**`, migrations, persistence | `ss14-databases-migrations.md` |
| `**/NPC/**`, HTN, pathfinding, mob behavior | `ss14-npc-ai.md` |

## Step 3 — path-specific routing (subtree AGENTS.md)

These subtree routers add path-specific guidance on top of the canon. Read the
one for the area you're in:

- `Content.Shared/AGENTS.md`
- `Content.Server/AGENTS.md`
- `Content.Client/AGENTS.md`
- `Resources/AGENTS.md`
- `Content.Tests/AGENTS.md`
- `Content.IntegrationTests/AGENTS.md`

## Universal invariants (the things you must not forget)

1. **Fork code → `_Art`.** Upstream code → don't add fork-only stuff to it.
2. **Upstream edits → narrow + `ss14-art edit` markers** (single-line or
   `start`/`end` block). Every changed upstream line carries a marker.
3. **New system/file → check `_Art` exists for that area, write there.**
4. **Don't build/run/test** to validate. Describe the code in the PR instead.
5. **Predict** player interactions (Shared logic), **`Dirty`** replicated
   state, **`NetEntity`** over the wire.
6. **Localize** every player-facing string (`fork-`-prefixed keys, `_Art`
   `.ftl`).
7. **Don't edit `RobustToolbox/`** — escalate (`ss14-engine-edits.md`).

## What "done" looks like for a task

- [ ] Read the always-rules + the rules/skills for the paths touched.
- [ ] Fork code in `_Art`; upstream edits minimal + marked.
- [ ] Prediction/networking/localization handled where needed.
- [ ] No build/run (unless explicitly asked); PR describes what/where/how.
- [ ] No edits under `RobustToolbox/`; no drive-by refactors.

## For review bots (CodeRabbit etc.)

Apply the same routing. A PR that edits an upstream file without `ss14-art edit`
markers, adds fork-only code outside `_Art`, edits `RobustToolbox/`, or shows
player-facing raw English should be flagged. See `.coderabbit.yaml`.
