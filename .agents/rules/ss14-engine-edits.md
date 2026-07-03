# Rule: engine edits are escalated, not done

The engine is `RobustToolbox/`. It is a separate project (a git submodule) that
we pull from upstream. **Do not edit files under `RobustToolbox/` as part of a
normal feature task.**

## Why

- Engine changes affect everything, not just your feature.
- They break upstream's ability to merge engine updates cleanly.
- They often require engine-side review we can't self-approve.
- 99% of "I need to edit the engine" cases are actually a missing seam in
  *content*, fixable without touching the engine.

## What to do instead

Before considering an engine edit, exhaust these in order:

1. **Subscribe to an existing event** from an `_Art` or content system. Most
   "the engine doesn't expose X" needs are met by events the engine already
   raises.
2. **Use an existing engine API** you overlooked. Search `RobustToolbox` for the
   system/component that already does what you need. The engine is large.
3. **Add the seam in content**, not the engine. If you need new behavior, add a
   content-side system/component in `_Art` that wraps or augments the engine
   one.
4. **Wrap, don't fork.** If an engine type is almost right, derive or compose
   in content rather than editing the engine type.

## When an engine edit is genuinely needed

It's rare but real (engine bug blocking your feature, missing primitive with no
content-side workaround). In that case **escalate**, don't just do it:

- Stop the feature task.
- Note the exact file, the required change, and *why no content-side workaround
  exists*. Be specific — "I tried X, it failed because Y".
- Surface this to a human maintainer in the PR/issue. Do not merge an engine
  edit quietly.
- The engine change, if approved, is a **separate** change from your feature,
  not buried inside it.

## Hard no

- Never edit `RobustToolbox/` to work around a bug you could fix in content.
- Never edit `RobustToolbox/` to add a convenience API nobody else needs yet.
- Never edit `RobustToolbox/` and leave it unmentioned in the PR.

If you're unsure whether something is engine or content: if the file path
starts with `RobustToolbox/`, it's engine. Everything under `Content.*/` (even
outside `_Art`) is content, governed by `ss14-fork-editing.md`, not this rule.
