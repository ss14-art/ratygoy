# Rule: bridge adapters point at the canon, they don't duplicate it

The canon lives in `.agents/rules/` and `.agents/skills/`. The bridge files are
**adapters** that get each tool to read the canon:

| Tool | Bridge file(s) |
|---|---|
| Codex / generic `AGENTS` | `AGENTS.md` (root) + subtree `AGENTS.md` |
| Cursor | `.cursor/rules/*.mdc` |
| GitHub Copilot | `.github/copilot-instructions.md` + `.github/instructions/*.md` |
| Claude Code | `CLAUDE.md` + `.claude/rules/*.md` |
| Gemini | `GEMINI.md` |
| CodeRabbit | `.coderabbit.yaml` |

## The rule

- **Don't copy rule/skill text into the bridges.** Bridges are thin: they state
  the universal invariants (fork-vs-upstream, the `ss14-art edit` markers, "read
  `.agents`", "don't build") and point at `.agents/` for the rest.
- When a rule changes, you edit `.agents/rules/*.md` once. The bridges keep
  working because they reference the canon, not a stale copy.
- If a tool needs a tool-specific nudge (e.g. "Cursor: respect
  `.cursor/rules/`" or "Claude: load `.claude/skills`"), put *that* in the
  bridge, not the rule content.
- Keep the set of bridges in sync with this table. Adding a tool = add a bridge
  that points at `.agents`, plus a row here.

## What every bridge must contain

At minimum, every bridge states:

1. This is the hakumai SS14 fork. Fork code lives in `_Art` folders; everything
   else is upstream. (see `.agents/rules/ss14-code-placement.md`)
2. Upstream edits must be narrow and marked `// ss14-art edit` (single line) or
   `// ss14-art edit start`/`end` (block). (see
   `.agents/rules/ss14-fork-editing.md`)
3. New code goes in `_Art`, not upstream files.
4. Don't build/run/test to validate; describe the code in the PR instead. (see
   `.agents/rules/ss14-ai-workflow.md`)
5. Read `AGENTS.md` (root) and `.agents/rules/` for the full rules; load the
   matching `skills/ss14-*.md` for the subsystem you're touching.

The rest is in the canon. Bridges are pointers.
