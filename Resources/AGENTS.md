# Resources — subtree router

`Resources/` is **data**, not C# assemblies: prototypes (YAML), locale
(`.ftl`), textures (RSI), audio (`.ogg`), maps, config presets. No code logic.

## Read first

- Root `AGENTS.md` (router).
- `.agents/rules/ss14-localization.md` — every player-readable string.
- `.agents/skills/ss14-prototype-basics.md` — prototype YAML anatomy.
- `.agents/skills/ss14-examples.md` — worked proto/reagent/visualizer/locale
  examples.

## Fork data here

| Kind | Fork path | Notes |
|---|---|---|
| Prototypes | `Resources/Prototypes/_Art/...` | mirror upstream `Resources/Prototypes/` structure |
| Locale | `Resources/Locale/en-US/_Art/...` | `.ftl`, keys `fork-...` |
| Textures | `Resources/Textures/_Art/...` | RSI folders; reference as `_Art/...` |
| Audio | `Resources/Audio/_Art/...` | `.ogg`; reference as `/Audio/_Art/...` |
| Maps | `Resources/Maps/_Art/...` | fork maps (rare) |
| Config | `Resources/ConfigPresets/_Art/...` | fork config presets (rare) |

## Domain skills that apply in Resources

- `ss14-prototype-basics.md` — entity/item/structure/reagent prototypes.
- `ss14-audio.md` — audio files + `soundCollection` prototypes.
- `ss14-sprite-overlays-shaders.md` — RSI/sprite layers, `GenericVisualizer`
  (prototype-side), `.swsl` shaders.
- `ss14-atmos.md` — reagent/gas reaction prototypes (the YAML side).
- `ss14-ui-bui.md` — the `UserInterface` interfaces map in an entity
  prototype declares a BUI.
- `ss14-npc-ai.md` — HTN behavior prototypes (YAML composition).

## Hard rules specific to Resources

- **`name`/`description` on entity prototypes are locale keys**, not raw text.
  Provide the key in an `_Art` `.ftl`. Wrong: `name: Sell Container`. Right:
  `name: fork-market-sale-container-name`.
- **Fork art on fork paths.** Don't reuse upstream `Resources/Textures/` /
  `Resources/Audio/` paths for fork-only assets; copy/license into `_Art`.
- **`fork-`-prefixed locale keys**, kebab-case, one `.ftl` per feature.
- Prototype `id`s fork-named to avoid collisions with upstream ids.
- Reagent locale keys follow upstream style: `reagent-name-*`, `reagent-desc-*`,
  `reagent-physical-desc-*` (fork id in the suffix).

## Common mistakes

- Raw English `name`/`description` on a prototype.
- Fork prototype outside `_Art` (e.g. dropped into an upstream
  `Resources/Prototypes/Entities/` folder).
- Sprite path pointing at an upstream RSI for fork-only art.
- Missing the `UserInterface` interfaces map on an entity that has a BUI.
- Locale key not defined in any `.ftl` → shows the raw key to players.
