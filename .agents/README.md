# `.agents/` — canonical AI instruction layer for the hakumai SS14 fork

This directory is the **single source of truth** for how AI agents and review
bots must write code in this repository. Everything else — `AGENTS.md`,
`CLAUDE.md`, `GEMINI.md`, `.cursor/rules/`, `.github/copilot-instructions.md`,
`.github/instructions/`, `.coderabbit.yaml` — is an *adapter* that points back
here. Do not duplicate rules in the adapters; edit the canon here.

## Layout

- `rules/` — hard rules. Short, normative, "do this / don't do this". Read first.
- `skills/` — deeper, domain-specific guidance. Loaded on demand by the router
  in `AGENTS.md` when a task touches that subsystem.

## How an agent should use this

1. Read `AGENTS.md` at the repo root (the router). It tells you which
   `rules/*.md` and `skills/*.md` apply to your task.
2. Read every rule it points you at. Rules are mandatory.
3. Read only the skills relevant to the files you're about to touch.
4. Follow `rules/ss14-ai-workflow.md` (also mirrored as a skill) for *how* to
   work: narrow diffs, no rebuilds, describe the code in the PR, etc.

## What this is NOT

- Not documentation for humans (that lives in `docs/` upstream; here `docs/` is
  **not** a source of truth — skills are).
- Not a style guide that you override with "but upstream does it this way".
  Where upstream and these rules disagree on fork code, these rules win.
- Not a place to record one-off facts. Each file is a reusable rule or skill.
