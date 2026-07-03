# ast-grep rules for SS14 anti-patterns

Mechanical lints CodeRabbit runs on C#/XAML diffs. Each flags a common SS14
fork mistake. They complement `.agents/rules/` (prose catches intent, ast-grep
catches syntax).

## Rules

- `ss14-raw-string-popup.yml` — `_popup.PopupEntity("literal", ...)` (and
  PopupClient/Server/Cursor/Coordinates) with a raw string instead of
  `Loc.GetString(...)`.
- `ss14-xaml-literal-text.yml` — `Text="..."` / `Title="..."` literal in XAML
  (should be `{Loc ...}`).

## What ast-grep does NOT cover (handled in `.coderabbit.yaml` path_instructions)

These are better as prose path-instructions than ast-grep, because they're about
*file location* / *presence of a marker* rather than a syntax shape:

- **Upstream edit without `// ss14-art edit` marker** — CodeRabbit's
  `path_instructions` for `**/*.cs` flags this. ast-grep doesn't know a file's
  path (upstream vs `_Art`), so it can't distinguish "needs a marker" from
  "already fork code, no marker needed".
- **Fork-only code outside `_Art`** — same reason (path-based).
- **Engine edits (`RobustToolbox/`)** — path-filtered out of review entirely.
- **Missing `Dirty`** — semantic, not syntactic; flagged via prose.

## Tuning

After a few live PRs, review noise from these rules and refine:
- tighten the popup rule's method list if it misses/over-matches,
- the XAML literal-text rule is broad (will flag intentionally-non-localized
  debug labels) — add exceptions or narrow it,
- add rules for new recurring anti-patterns found in review.

See `.coderabbit.yaml` → `reviews.tools.ast-grep` and the project plan's
"CodeRabbit hardening" section.
