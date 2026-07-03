# hakumai fork — pointer rule

This repo is the **hakumai** (ss14-art) SS14 fork. The canon lives in `.agents/`.
This file is a pointer — do not duplicate rules here.

## Read first, every task

1. `AGENTS.md` (repo root) — the router.
2. `.agents/rules/` — the rules it points at (mandatory, short).
3. `.agents/skills/ss14-*.md` — only the ones for the subsystems you touch.
4. The subtree `AGENTS.md` for the area.

## Universal invariants

- **Fork code → `_Art` folders** (`Content.{Shared,Server,Client}/_Art`,
  `Resources/Prototypes/_Art`, `Resources/Locale/en-US/_Art`,
  `Resources/Textures/_Art`, `Resources/Audio/_Art`). Everything outside `_Art`
  is upstream (read-mostly). New fork systems/files → `_Art`, never in upstream
  files. (`.agents/rules/ss14-code-placement.md`)
- **Upstream edits → narrow + marked.** Single line: `// ss14-art edit`.
  Block: `// ss14-art edit start` … `// ss14-art edit end`. YAML:
  `# ss14-art edit` / `# ss14-art edit start` … `end`. XAML:
  `<!-- ss14-art edit start -->` … `<!-- ss14-art edit end -->`. Every changed
  upstream line carries a marker. (`.agents/rules/ss14-fork-editing.md`)
- **Don't edit `RobustToolbox/`** (engine). Escalate.
  (`.agents/rules/ss14-engine-edits.md`)
- **Don't build/run/test** to validate (no `dotnet build`, `RUN_THIS.py`,
  `runserver.sh/bat`, `runclient.sh/bat`, game, test runner) unless a human
  explicitly asks. Reason from code + rules; describe the change in the PR.
  (`.agents/rules/ss14-ai-workflow.md`)
- **Predict** (logic in `Content.Shared`), **`Dirty`** replicated state,
  **`NetEntity`** over the wire. (`.agents/rules/ss14-prediction.md`,
  `ss14-networking.md`)
- **Localize** every player-facing string: `fork-`-prefixed keys in
  `Resources/Locale/en-US/_Art/*.ftl`, `{Loc ...}` in XAML.
  (`.agents/rules/ss14-localization.md`)

## Claude Code note

- `.claude/skills/` has runtime skills (e.g. `run-hakumai` to run the
  server/client). Don't invoke build/run skills unless explicitly told to.
- Canon is `.agents/`; this `.claude/rules/` only mirrors pointers.

Full rules: `.agents/rules/`. Full skills: `.agents/skills/`.
