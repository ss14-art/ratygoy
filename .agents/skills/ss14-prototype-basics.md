# Skill: SS14 prototype basics

**When to read:** you're adding or editing a prototype (entity, item, structure,
reagent, decal, etc.) in YAML under `Resources/Prototypes/`.

## What a prototype is

A prototype is a YAML-defined data definition the game instantiates. Entities,
items, structures, reagents, decals, sound collections, etc. are all
prototypes. They live under `Resources/Prototypes/...` as `.yml`.

- `type: entity` — the most common. Defines a spawnable thing with components.
- `type: reagent` — a chemical reagent.
- `type: decal`, `type: entityTemplate`, `type: soundCollection`, etc.

## Fork prototypes

Fork prototypes go in `Resources/Prototypes/_Art/...`, mirroring upstream path
structure (see `ss14-fork-editing.md` → preserve path similarity). Example the
fork already uses (`Resources/Prototypes/_Art/market.yml`):

```yaml
- type: entity
  id: MarketSaleContainer
  name: fork-market-sale-container-name
  description: fork-market-sale-container-desc
  parent: BaseStructureDynamic
  components:
  - type: Transform
    anchored: true
  - type: MarketContainer
  - type: Storage
    maxItemSize: Huge
    grid:
    - 0,0,9,7
  - type: UserInterface
    interfaces:
      enum.StorageUiKey.Key:
        type: StorageBoundUserInterface
  - type: ContainerContainer
    containers:
      storagebase: !type:Container
        ents: []
  - type: InteractionOutline
  - type: Sprite
    noRot: true
    sprite: /Textures/Structures/Storage/orebox.rsi
    layers:
    - state: orebox
    - state: orebox-top
      map: [ top ]
      visible: true
```

Read this carefully — it demonstrates nearly every convention below.

## Anatomy

- `id` — the prototype's unique ID. PascalCase. Fork IDs should be clearly
  fork-named (e.g. `MarketSaleContainer`) so they don't collide with upstream.
- `parent` — inherit from a base prototype to get its components, then override
  / add. Prefer parenting to a `Base*` over redefining everything.
- `abstract: true` — a base prototype that isn't spawnable, only inherited
  from.
- `noSpawn: true` — defined but not spawnable directly (e.g. an internal
  container entity).
- `name` / `description` — **localization keys**, not raw text. Provide the
  key in an `_Art` `.ftl` (see `ss14-localization.md`). Wrong: `name: Sell
  Container`. Right: `name: fork-market-sale-container-name`.
- `components:` — list of component instances. Each `- type: <ComponentType>`
  then its `[DataField]`s as YAML keys.
- `suffix` — an editor-only label (optional).

## Component fields in YAML

A component's C# `[DataField]` names map to YAML keys. From
`MarketSaleConsoleComponent`:

```csharp
[DataField] public int MaxListingsPerPlayer = 10;
[DataField] public float ContainerSearchRadius = 2.0f;
```

→ in YAML:

```yaml
  - type: MarketSaleConsole
    maxListingsPerPlayer: 5
    containerSearchRadius: 3.0
```

- YAML key = the C# field name, **camelCase** (first letter lowercased).
- Omit a key → the C# initializer default applies.
- Use the typed form where the engine expects it: `!type:Container` for
  container entries, `!type:All`/`!type:AllWhitelist` etc. for whitelists.

## Sprite / RSI

- `sprite:` points at an RSI directory under `Resources/Textures/...`. Fork
  sprites live under `Resources/Textures/_Art/...` (mirror the convention).
- `layers:` with `state:` and optional `map:` (for `GenericVisualizer` /
  layer-keyed visuals) and `visible:`.
- Don't hardcode upstream sprite paths for fork-only visuals; put fork sprites
  in `_Art` and reference them.

## Common pitfalls

- **Raw English `name`/`description`** → must be locale keys.
- **Wrong parent** → inheriting a non-`Base` entity drags in spawn behavior you
  don't want. Use the right `Base*` or set `noSpawn`/`abstract`.
- **Missing `ContainerContainer`** when the entity has storage/containers →
  the container won't initialize correctly.
- **Colliding `id`** with upstream → prefix fork IDs clearly.
- **Forgetting `UserInterface` interfaces map** when the entity has a BUI → the
  UI key must map to a `BoundUserInterface` type (see
  `skills/ss14-ui-bui.md`).

## Where to look

- Fork example: `Resources/Prototypes/_Art/market.yml`.
- Upstream bases: `Resources/Prototypes/Entities/BaseStructure*.yml`,
  `BaseItem.yml`, etc. Read the parent before parenting to it.
- Full worked example: `skills/ss14-examples.md`.
