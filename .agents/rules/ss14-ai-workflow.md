# Rule: how an AI agent works in this repo

This rule governs *process*, not code. Breaking it wastes reviewer time and
your own. Read it before any task.

## Do NOT build, run, or "test" the project to validate your changes

This repo is large and slow to build. **Do not** run `dotnet build`, the game
client/server, tests, `RUN_THIS.py`, `runserver.bat`, or any launch/build
command as part of producing or checking your code — unless the task
**explicitly** asks you to. You do not need to compile to write correct code
here; the rules and skills tell you the patterns, and the reviewer/build CI
catches mechanical errors.

What to do instead:

- Read the existing code you're matching. Confirm the pattern is real
  (`grep`/read the file you're imitating).
- Reason about correctness from the code and these rules.
- Describe the change and how it works in the PR/commit, in detail, so a
  reviewer (or another agent) can follow it without running it.

The one exception: if a human explicitly asks you to build/run/test, do that
specific thing and report the result. Don't generalize it into "I'll just
compile to be safe."

## Narrow diffs

- Touch only what the task needs. See `ss14-fork-editing.md`.
- Upstream edits: minimal + marked with `ss14-art edit`.
- Fork edits: normal scope, but still no unrelated reformatting.
- Don't fix "things you noticed" in the same diff. Note them, separate change.

## Describe the code, don't just drop it

In the PR description / your summary, for each non-trivial change explain:

- **What** the code does.
- **Where** it lives and why (which `_Art` folder / which assembly, and why
  that one — see `ss14-client-server-shared.md`).
- **How** it works: the event flow, what's predicted vs server-authoritative,
  what state is networked and `Dirty`'d, what locale keys were added.
- **What you deliberately did *not* do** (e.g. "did not edit the upstream
  `StorageSystem`; subscribed to `StorageComponent` events from `_Art`
  instead"), and why.
- Any upstream edits, with the `ss14-art edit` marker locations listed.

This narrative is what keeps an agent (or reviewer) from "schizing" — guessing
at intent. The code + the description together are the deliverable.

## Mirroring and path similarity

- Mirror upstream folder structure inside `_Art` (see
  `ss14-fork-editing.md` → preserve path similarity).
- New feature → all four relevant layers (Shared component, Server system,
  Client BUI/XAML, Resources prototype/locale) where the feature needs them.
  Don't put a client menu in Server, don't put a networked component in Client.

## Before you touch a file, check the canon

- Is this file upstream (outside `_Art`) or fork (`_Art`)?
  → `ss14-code-placement.md`.
- If upstream → markers + narrow diff → `ss14-fork-editing.md`.
- Which assembly? → `ss14-client-server-shared.md`.
- Does it network/predict? → `ss14-networking.md`, `ss14-prediction.md`.
- Strings? → `ss14-localization.md`.
- Style/naming? → `ss14-code-style.md`.
- Domain specifics (audio/atmos/ui/etc.)? → the matching `skills/ss14-*.md`.

## Self-check before finishing

- [ ] I did not build/run/test the project (or only did so because explicitly
      asked).
- [ ] Every upstream-file change has a `ss14-art edit` marker.
- [ ] Fork-only code is in `_Art`, not in upstream files.
- [ ] Prediction + networking + localization handled where the feature needs
      them.
- [ ] The PR/summary describes what/where/how for each non-trivial change.
- [ ] No unrelated reformatting or drive-by fixes mixed in.
