# GEMINI.md

This repo is **hakumai** (ss14-art), a fork of Space Station 14. The canonical
AI instructions live in `.agents/` — this file is a thin pointer. **Do not
duplicate rules here; edit the canon in `.agents/rules/`.**

## Start every task here

1. Read `AGENTS.md` (repo root) — it routes you to rules + skills by path.
2. Read the rules it points at (all in `.agents/rules/`).
3. Read only the matching `.agents/skills/ss14-*.md` for the subsystems you touch.
4. Read the subtree `AGENTS.md` for the area (`Content.Shared`, `Content.Server`,
   `Content.Client`, `Resources`, `Content.Tests`, `Content.IntegrationTests`).

## Universal invariants (the short list)

- **Fork code lives in `_Art` folders.** Everything outside `_Art` is upstream
  (read-mostly). New fork systems/files → `_Art`, never in upstream files.
  See `.agents/rules/ss14-code-placement.md`.
- **Upstream edits must be narrow and marked**: single line
  `// ss14-art edit`, block `// ss14-art edit start` … `// ss14-art edit end`.
  YAML: `# ss14-art edit` / `# ss14-art edit start` … `end`. XAML:
  `<!-- ss14-art edit start -->` … `<!-- ss14-art edit end -->`. Every changed
  upstream line carries a marker. See `.agents/rules/ss14-fork-editing.md`.
- **Don't edit `RobustToolbox/`** (the engine). Escalate instead. See
  `.agents/rules/ss14-engine-edits.md`.
- **Don't build, run, or test the project** to validate your code — no
  `dotnet build`, `RUN_THIS.py`, `runserver.sh/bat`, `runclient.sh/bat`, the
  game, or the test runner — unless a human explicitly asks. Reason from the
  code + rules; describe the change in the PR. See
  `.agents/rules/ss14-ai-workflow.md`.
- **Predict** player interactions (logic in `Content.Shared`), **`Dirty`**
  replicated state, **`NetEntity`** over the wire. See
  `.agents/rules/ss14-prediction.md`, `ss14-networking.md`.
- **Localize** every player-facing string: `fork-`-prefixed keys in
  `Resources/Locale/en-US/_Art/*.ftl`, `{Loc ...}` in XAML. See
  `.agents/rules/ss14-localization.md`.

## Gemini specifics

- Treat `.agents/rules/` and `.agents/skills/` as the source of truth; this
  file is a pointer, not a copy. If a rule here and a file in `.agents/`
  disagree, the `.agents/` file wins — and flag the drift so it can be fixed.
- When summarizing a change, follow `.agents/rules/ss14-ai-workflow.md`:
  describe what/where/how for each non-trivial change, list upstream edits +
  marker locations, state what you deliberately didn't do and why.

## What "done" looks like

- [ ] Read `AGENTS.md` + the rules/skills for the paths touched.
- [ ] Fork code in `_Art`; upstream edits minimal + marked.
- [ ] Prediction/networking/localization handled where needed.
- [ ] No build/run/test (unless explicitly asked); PR describes the change.
- [ ] No edits under `RobustToolbox/`; no drive-by refactors.

The full rules are in `.agents/rules/`. The full skills are in `.agents/skills/`.
