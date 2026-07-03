# GitHub Copilot instructions — hakumai (ss14-art) SS14 fork

This repo is a fork of Space Station 14. Canonical AI instructions live in
`.agents/`. This file is a thin pointer — **do not duplicate rules here; edit
the canon in `.agents/rules/`.**

## Start every task

1. Read `AGENTS.md` (repo root) — routes you to rules + skills by path.
2. Read the rules in `.agents/rules/` it points at.
3. Read only the matching `.agents/skills/ss14-*.md` for the subsystems you
   touch (path → skill mapping in `.github/instructions/`).
4. Read the subtree `AGENTS.md` for the area.

## Universal invariants

- **Fork code lives in `_Art` folders** (`Content.{Shared,Server,Client}/_Art`,
  `Resources/Prototypes/_Art`, `Resources/Locale/en-US/_Art`,
  `Resources/Textures/_Art`, `Resources/Audio/_Art`). Everything outside `_Art`
  is upstream (read-mostly). New fork systems/files → `_Art`, never in upstream
  files. See `.agents/rules/ss14-code-placement.md`.
- **Upstream edits must be narrow and marked**:
  - single line: `// ss14-art edit`
  - block: `// ss14-art edit start` … `// ss14-art edit end`
  - YAML: `# ss14-art edit` / `# ss14-art edit start` … `# ss14-art edit end`
  - XAML: `<!-- ss14-art edit start -->` … `<!-- ss14-art edit end -->`
  Every changed upstream line carries a marker. See
  `.agents/rules/ss14-fork-editing.md`.
- **Don't edit `RobustToolbox/`** (engine). Escalate. See
  `.agents/rules/ss14-engine-edits.md`.
- **Don't build, run, or test** to validate your code (no `dotnet build`,
  `RUN_THIS.py`, `runserver.sh/bat`, `runclient.sh/bat`, the game, the test
  runner) unless a human explicitly asks. Reason from code + rules; describe
  the change in the PR. See `.agents/rules/ss14-ai-workflow.md`.
- **Predict** player interactions (logic in `Content.Shared`), **`Dirty`**
  replicated state, **`NetEntity`** over the wire. See
  `.agents/rules/ss14-prediction.md`, `ss14-networking.md`.
- **Localize** every player-facing string: `fork-`-prefixed keys in
  `Resources/Locale/en-US/_Art/*.ftl`, `{Loc ...}` in XAML. See
  `.agents/rules/ss14-localization.md`.

## Path-specific instructions

See `.github/instructions/*.instructions.md` — each applies `applyTo` a path
glob and routes to the matching `.agents/skills/ss14-*.md`.

## Done

- [ ] Read `AGENTS.md` + rules/skills for the paths touched.
- [ ] Fork code in `_Art`; upstream edits minimal + marked.
- [ ] Prediction/networking/localization handled where needed.
- [ ] No build/run/test (unless explicitly asked); PR describes the change.
- [ ] No edits under `RobustToolbox/`; no drive-by refactors.

Full rules: `.agents/rules/`. Full skills: `.agents/skills/`.
