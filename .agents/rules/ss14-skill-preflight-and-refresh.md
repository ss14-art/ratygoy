# Rule: skill preflight and refresh

Before writing code, run this preflight so you load the right skills and don't
work from stale assumptions. This rule is also what keeps the instruction layer
maintained.

## Preflight (do this at the start of every task)

1. **Read `AGENTS.md` at the repo root.** It's the router — it maps file
   paths/subsystems to the `rules/*.md` and `skills/*.md` that apply.
2. **Read every rule the router points at for your task's paths.** Rules are
   mandatory and short. (The core rules: code-placement, fork-editing,
   engine-edits, ecs-basics, prediction, localization, client-server-shared,
   networking, debugging, code-style, ai-workflow.)
3. **Read only the skills relevant to the files you'll touch.** Skills are
   deeper and domain-specific. Don't read all of them — read the matching ones:
   - Touching audio → `skills/ss14-audio.md`
   - Touching atmos → `skills/ss14-atmos.md`
   - Touching transform/physics → `skills/ss14-transform-physics.md`
   - Touching PVs → `skills/ss14-pvs.md`
   - Touching BUI → `skills/ss14-ui-bui.md`; XAML → `skills/ss14-ui-xaml.md`;
     EUI → `skills/ss14-ui-eui.md`
   - Touching sprites/overlays/shaders → `skills/ss14-sprite-overlays-shaders.md`
   - Touching DB/migrations → `skills/ss14-databases-migrations.md`
   - Touching NPC/AI → `skills/ss14-npc-ai.md`
   - Onboarding / unsure of basics → the `ss14-*-basics` skills.
4. **Confirm the pattern you're imitating is real** — `grep`/read the actual
   file, don't trust memory. Patterns drift across upstream versions.

## When you're unsure which skill applies

Use the path. The root `AGENTS.md` and the subtree `AGENTS.md` files
(`Content.Shared/AGENTS.md`, `Content.Server/AGENTS.md`,
`Content.Client/AGENTS.md`, `Resources/AGENTS.md`, `Content.Tests/AGENTS.md`,
`Content.IntegrationTests/AGENTS.md`) encode path → skill routing. If a path
isn't covered, fall back to the core rules; they cover the universal cases.

## Refresh: keeping skills/rules current

The canon is a maintained layer, not a one-off. When you notice a rule/skill is
wrong or missing something:

- **While working a task:** if a rule contradicts reality (the real pattern
  differs from what the rule says), trust the real code for *this* task, and
  leave a note in the PR pointing at the stale rule so a maintainer can update
  the canon. Don't silently work around the canon.
- **Don't rewrite the canon mid-feature.** Fixing a rule is a separate change;
  flag it, don't bundle it.
- **Adding a new subsystem?** Add a `skills/ss14-<subsystem>.md`, register it in
  the root `AGENTS.md` router and the relevant subtree `AGENTS.md`, and mirror
  the pointer into the bridge adapters (`.cursor`, copilot, claude, gemini) per
  `ss14-bridges.md` (kept as part of this rule set). Keep the four-layer
  coverage in sync.

## Stale canon signals

If you find yourself ignoring a rule because "it doesn't fit," that's a signal
to update the rule, not to bypass it silently. Surface it.
