# Rule: ECS basics

Space Station 14 is **Entity-Component-System**. If you write OOP-style "god
objects" or put behavior on components, your code will be wrong and rejected.

## The three parts

- **Entity** — an ID (`EntityUid`) with no behavior of its own. Entities are
  bags of components.
- **Component** — pure data. A `[RegisterComponent] sealed partial class`
  extending `Component`. Fields are `[DataField]`s for serialized/map-defined
  data. **No logic** beyond maybe a trivial constructor-default. No methods that
  do work.
- **System** — an `EntitySystem` that holds the logic. Systems subscribe to
  events, query components, and mutate component data. Logic lives here.

Data on the component, behavior in the system. Repeat it.

## Minimal correct component (fork)

```csharp
namespace Content.Shared._Art.Market.Components;

[RegisterComponent]
public sealed partial class MarketSaleConsoleComponent : Component
{
    [DataField]
    public int MaxListingsPerPlayer = 10;

    [DataField]
    public float ContainerSearchRadius = 2.0f;
}
```

- `[RegisterComponent]` so the component type is known.
- `sealed partial` always (the `partial` is for engine codegen).
- `[DataField]` on every field that should serialize (map/proto/yaml-defined
  state). Non-serialized runtime scratch fields get `[DataField]` removed only
  if they must not persist — prefer keeping them serialized by default and only
  opt out with `[DataField(customRecordRepl: true)]` or by not marking when
  they're purely runtime. When unsure, mark it; serialization is the safe
  default.
- Field initializers give the default value used when the field isn't specified
  in YAML.

## Minimal correct system (fork)

```csharp
namespace Content.Server._Art.Market;

public sealed class MarketSystem : EntitySystem
{
    [Dependency] private readonly BankSystem _bank = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        // subscribe to events here
    }
}
```

- `[Dependency] private readonly SomeSystem _name = default!;` for dependencies.
  See `ss14-code-style.md` for the `_name` naming rule.
- Subscribe in `Initialize()`. Use `SubscribeLocalEvent<TComp, TEvent>(handler)`.
- Systems are `sealed` (engine handles instantiation). `partial` only if you
  split the system across files.

## Events are the glue

Components don't call each other. Systems raise and handle events.

```csharp
SubscribeLocalEvent<MarketSaleConsoleComponent, BoundUIOpenedEvent>(OnSaleUIOpened);

private void OnSaleUIOpened(EntityUid uid, MarketSaleConsoleComponent comp, BoundUIOpenedEvent args) { ... }
```

- Handler signature: `(EntityUid uid, TComponent comp, ref TEvent args)` — note
  the `ref` on the event for events that carry mutable data back.
- Use `Entity<TComponent>` for the typed entity tuple where the API offers it
  (see `ss14-code-style.md`).

## What not to do

- Don't put methods that do work on a component. Components are data.
- Don't give a component a reference to a system. Systems hold systems.
- Don't create a component to "carry" behavior that should be an event.
- Don't query `EntityManager.GetEntities` in a hot loop; use queries/lookups.
- Don't store per-tick mutable state on a component that should be local system
  state (and vice versa — state that must serialize goes on the component).

## Where to read more

- `skills/ss14-ecs-basics.md` — extended walkthrough.
- `skills/ss14-common-api-patterns.md` — EntitySystem / PrototypeManager / Audio
  / Popup patterns.
- `skills/ss14-examples.md` — full Component + System example.
