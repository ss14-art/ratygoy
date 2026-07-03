# Skill: sprite / overlays / shaders

**When to read:** you're doing entity visuals (sprites, layers, animation,
`GenericVisualizer`), screen-space overlays (health bars, vignettes, HUD
markers), or shaders (`.swsl`).

## Sprites — `SpriteComponent` + RSI

- `SpriteComponent` is the component; `type: Sprite` in a prototype configures
  it. `sprite:` points at an RSI directory (`/Textures/...`), `layers:` lists
  sprite layers with `state:`, optional `map:` (layer keys) and `visible:`.
- Fork sprites → `Resources/Textures/_Art/...`. Reference as
  `sprite: _Art/...` (no leading `Resources/`).
- **RSI**: a folder with a `.rsi.json` meta + `.png` states. States have
  directions and delays. Add fork RSIs under `_Art`; don't reuse upstream
  sprite paths for fork-only art.

### Layer-keyed visuals — `GenericVisualizer`

When an entity's sprite should react to component state (on/off, open/closed),
map an enum's states to layer changes in the prototype:

```yaml
- type: Sprite
  layers:
  - state: base
  - state: on
    map: [ "enum.ForkConsoleVisuals.Layer" ]
    visible: false
- type: GenericVisualizer
  visuals:
    enum.ForkConsoleVisuals.Powered:
      enum.ForkConsoleVisuals.Layer:
        True: { visible: true }
        False: { visible: false }
```

System toggles: `_appearance.SetData(uid, ForkConsoleVisuals.Powered, bool)`.
See `ss14-examples.md` Example 4.

### `AppearanceComponent`

Required for `GenericVisualizer`/`VisualizerSystem`. Add `- type: Appearance`
to the prototype. Systems set appearance data keys; visualizers read them. Keep
visual state in appearance data, not by directly poking the sprite from logic
(unless you have a specific reason).

### Sprite system APIs

- `Content.Client/Sprite/` — `ContentSpriteSystem`, `RandomSpriteSystem`,
  `ScaleVisualsSystem`, `SpriteFadeSystem`. Use these / their components rather
  than hand-rolling.
- For programmatic layer changes, prefer appearance-data + a visualizer over
  direct `sprite.LayerSetVisible` calls scattered in logic.

## Overlays — screen-space, client-only

Overlays draw on top of the world view (health bars over mobs, vignettes, HUD
markers, noir filter). They're **client** systems in
`Content.Client/Overlays/`:

- A `Overlay` subclass draws in `Draw`/`DrawSpace`. An `OverlaySystem` adds it
  conditionally (`_overlay.AddOverlay(...)` / manages lifecycle).
- Examples: `EntityHealthBarOverlay`, `NoirOverlay`, `BlackAndWhiteOverlay`,
  `ShowCrewIconsSystem`, `ShowHealthBarsSystem`, `EquipmentHudSystem`.
- Fork overlays → `Content.Client/_Art/Overlays/...`.

When to use an overlay vs a sprite layer: overlay = screen-space / many
entities / global effect. Sprite layer = the entity's own look. Health bars
over many mobs → overlay. A single machine's on-light → sprite layer +
visualizer.

## Shaders — `.swsl`

Robust shaders are `.swsl` files in `Resources/Textures/Shaders/`. Applied via
`Shader` prototype / `SpriteComponent` shader field / overlay shaders.

- Fork shaders → `Resources/Textures/Shaders/_Art/...` (or a fork subfolder).
  Reference by prototype id.
- Apply to a sprite: `- type: Sprite ...` with a `shader:`/`postShader:` field
  per the upstream patterns. Apply to an overlay in the overlay's draw.
- `.swsl` is Robust's shader language; copy an existing shader as a starting
  point (`blurryx.swsl`, `camera_static.swsl` are in-repo examples).

## Pitfalls

- **Directly poking sprite layers in logic** instead of appearance data +
  visualizer — works but doesn't network/replicate cleanly and scatters
  visual logic. Prefer the data-driven path.
- **Fork art on upstream sprite paths** — fork art under `_Art`, referenced
  as `_Art/...`.
- **Overlay doing per-entity logic that should be PVs/server-aware** — overlays
  are client-side; only draw for entities the client knows about.
- **Shader applied without the prototype** — a `.swsl` needs a `type: shader`
  prototype to be referenced by id.
- **Missing `- type: Appearance`** when using a visualizer → no data flows.

## Where to look

- `GenericVisualizer`: `Resources/Prototypes/Body/species_base.yml` (real
  example), `ss14-examples.md` Example 4.
- Overlays: `Content.Client/Overlays/*.cs`.
- Sprite systems: `Content.Client/Sprite/*.cs`.
- Shaders: `Resources/Textures/Shaders/*.swsl` + their `type: shader` protos.
