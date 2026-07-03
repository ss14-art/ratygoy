# Skill: porting and licensing

**When to read:** you're bringing code/content in from another SS14 fork
(`_NF`, `_RMC14`, `_Funkystation`, upstream, etc.), or you're about to copy a
chunk of upstream code into the fork.

## The short version

This repo (hakumai / ss14-art) is SS14-licensed (MIT/Apache depending on the
piece — see `LICENSE.TXT` and upstream licensing). When you bring code in:

1. **Fork-only code goes in `_Art`**, with `Content.<Assembly>._Art` namespaces
   (see `ss14-code-placement.md`).
2. **Keep authorship/attribution honest.** If you port a system from another
   fork or upstream, note the origin in the PR. Don't strip copyright headers
   that exist on the source file.
3. **Don't copy upstream code into `_Art` when you could subscribe to it.**
   Copying creates a maintenance fork-of-a-fork that rots on merges. Prefer
   subscribing to upstream events from `_Art` (see `ss14-fork-editing.md` →
   maximize fork-only logic out of upstream).
4. **If you must copy** (upstream class is sealed / no seam), copy the minimum,
   mark it as fork code, and note *why* you couldn't subscribe, so a future
   maintainer knows when to re-evaluate.

## Porting from another fork (e.g. `_NF`)

This repo already depends on `_NF` (e.g. `Content.Server._NF.Bank.BankSystem`).
That means some fork code is a *dependency*, not something to re-port.

- If `_NF` already provides it and we depend on it → use it directly. Don't
  re-implement.
- If you're porting a feature from a fork we *don't* depend on → bring only the
  fork-only logic, adapt namespaces to `_Art`, adapt locale keys to
  `fork-...`, and follow our conventions (`ss14-code-style.md`,
  `ss14-localization.md`). Strip the other fork's prefix from namespaces and
  replace with `_Art`.
- Other forks' prefixes in this repo (`_RMC14`, `_Funkystation`,
  `_Persistence14`) are **their** code, present as dependencies/submodules.
  Don't edit them like they're ours; treat them like upstream (read-mostly).

## Adapting ported code to our conventions

When porting, rewrite to match us, don't keep the source's style:

- Namespace → `Content.<Assembly>._Art.<Feature>[.<Sub>]`.
- Locale keys → `fork-<feature>-...` in `Resources/Locale/en-US/_Art/`.
- Dependency field names → `_systemName` (see `ss14-code-style.md`).
- BUI → `Subs.BuiEvents<T>` helper, `BoundUserInterface` + `CreateWindow`,
  `FancyWindow` XAML (see `skills/ss14-ui-bui.md`, `ss14-ui-xaml.md`).
- `ProtoId<T>` not raw strings.
- `Entity<T>` where the API offers it.

A verbatim port that keeps the source fork's naming/conventions will get
review pushback. Adapt it.

## Licensing specifics

- Engine (`RobustToolbox`) and SS14 content are under their respective licenses
  (see `LICENSE.TXT`). Fork code we add is under the same project license —
  don't introduce incompatible licenses.
- Don't copy code from a fork with a stricter license without checking
  compatibility. If unsure, surface it in the PR for a maintainer.
- Assets (sprites, audio) have their own licensing/attribution requirements.
  Fork assets under `Resources/Textures/_Art`, `Resources/Audio/_Art`; don't
  mix upstream asset paths with fork assets.

## What not to do

- Don't `git mv` an upstream file into `_Art` to "claim" it — that's not a port,
  that's a divergence bomb. If you need fork behavior, add fork code in `_Art`
  and subscribe.
- Don't port a whole subsystem verbatim and leave the other fork's prefix
  (`_NF`, `_RMC14`) in our `_Art` namespaces.
- Don't strip attribution/copyright headers from ported files.
- Don't bring a dependency we don't already have without surfacing it.

## Where to look

- Rules: `ss14-code-placement.md`, `ss14-fork-editing.md`.
- The fork already using a cross-fork dependency: `Content.Server/_Art/Market`
  uses `Content.Server._NF.Bank.BankSystem`.
