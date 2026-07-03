---
applyTo: 'Resources/**'
description: Resources guidance — prototypes/locale/textures/audio. name/description are locale keys, fork data under _Art.
---

# Resources

Data, not code: prototypes (YAML), locale (`.ftl`), textures (RSI), audio
(`.ogg`), maps, config presets.

Read: `AGENTS.md` (root), `.agents/rules/ss14-localization.md`,
`.agents/skills/ss14-prototype-basics.md`, `.agents/skills/ss14-examples.md`.

- Fork data paths: `Resources/Prototypes/_Art/...`,
  `Resources/Locale/en-US/_Art/...`, `Resources/Textures/_Art/...`,
  `Resources/Audio/_Art/...`.
- Entity prototype `name`/`description` are **locale keys**, not raw text.
  Provide the key in an `_Art` `.ftl`.
- Locale keys: `fork-`-prefixed, kebab-case, one `.ftl` per feature.
- Fork art on fork paths; reference sprites as `_Art/...`, audio as
  `/Audio/_Art/...`. Don't reuse upstream asset paths for fork-only art.
- Prototype `id`s fork-named to avoid collisions.
- Entity with a BUI needs a `type: UserInterface` `interfaces:` map in its
  prototype.
