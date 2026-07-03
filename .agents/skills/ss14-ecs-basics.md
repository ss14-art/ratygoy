# Skill: SS14 ECS basics (extended)

**When to read:** you're new to the codebase or unsure how to structure a
component/system/event. The rule `ss14-ecs-basics.md` is the short form; this
walks further.

## The model

- **Entity** = `EntityUid` (an integer id) + the components attached to it. No
  behavior, no data of its own beyond metadata (name, transform).
- **Component** = `[RegisterComponent] sealed partial class : Component`. Pure
  data via `[DataField]`. No logic. Serialized/replicated by the engine.
- **System** = `sealed class : EntitySystem`. Holds logic. `[Dependency]`s on
  other systems. Subscribes to events in `Initialize()`.

Data on components, behavior in systems. This is the whole game.

## Component, fully

```csharp
using Robust.Shared.GameObjects; // for Component, DataField, RegisterComponent

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

- `[RegisterComponent]` — registers the type so YAML `type: MarketSaleConsole`
  and code can find it.
- `sealed partial` — always. `sealed` because we don't subclass components;
  `partial` because the engine's source-gen extends it.
- `[DataField]` — serialized. The YAML key is the field name camelCased
  (`maxListingsPerPlayer`). The initializer is the default.
- For inspection: `[ViewVariables]` on fields you want to debug live (see
  `ss14-debugging-workflow.md`).
- A component can be empty (marker): `MarketContainerComponent` is just
  `[RegisterComponent] public sealed partial class MarketContainerComponent :
  Component {}` — used to tag entities the market logic cares about.

## System, fully

```csharp
using System.Linq;
using Content.Server._Art.Market.Components;
using Content.Shared._Art.Market;
using Content.Shared._Art.Market.BUI;
using Content.Shared._Art.Market.Components;
using Robust.Server.GameObjects;

namespace Content.Server._Art.Market;

public sealed class MarketSystem : EntitySystem
{
    [Dependency] private readonly BankSystem _bank = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly MarketManager _market = default!;

    public override void Initialize()
    {
        base.Initialize();
        Subs.BuiEvents<MarketSaleConsoleComponent>(MarketConsoleUiKey.Sale, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnSaleUIOpened);
            subs.Event<MarketCreateListingMessage>(OnCreateListing);
            subs.Event<MarketCancelListingMessage>(OnCancelListing);
        });
    }
    // ... handlers ...
}
```

- `[Dependency] private readonly XSystem _x = default!;` — see
  `ss14-code-style.md` for naming.
- `Initialize()` — subscribe here. `base.Initialize()` first.
- `Subs.BuiEvents<TComp>(uiKey, subs => { ... })` — the modern BUI event
  subscription helper (defined in
  `Content.Shared/Inventory/RelaySubscriptionHelpers.cs` and friends). Prefer
  it over raw `SubscribeLocalEvent` for BUI messages.
- For non-BUI events: `SubscribeLocalEvent<TComp, TEvent>(Handler)`.

## Event handler shape

```csharp
private void OnCreateListing(EntityUid uid, MarketSaleConsoleComponent comp, MarketCreateListingMessage args)
{
    var player = args.Actor;
    // validate, then act
}
```

- `(EntityUid uid, TComponent comp, ref TEvent args)` when the event carries
  mutable data back (note `ref`).
- `(EntityUid uid, TComponent comp, TEvent args)` when it doesn't (BUI
  messages are reference types, no `ref`).
- The handler is thin: validate + delegate to `Try/Can/Do` (see
  `ss14-code-style.md`).

## Querying components

- `TryComp<T>(uid, out var comp)` — get if present.
- `Comp<T>(uid)` — get, throws if absent (use only when you know it's there).
- `HasComp<T>(uid)`.
- `EnsureComp<T>(uid)` — get-or-add.
- For bulk: entity queries (`EntityQueryEnumerator<T>`), `EntityLookupSystem`
  for spatial queries (the fork Market uses `_lookup` + a radius). Don't iterate
  all entities in a hot path.

## Raising events

```csharp
RaiseLocalEvent(uid, new MyEvent { Foo = bar });
RaiseLocalEvent(new MyGlobalEvent());
```

- Per-entity events: `RaiseLocalEvent(uid, evt)`.
- Broadcast: `RaiseLocalEvent(evt)` (no uid).
- Events let systems talk without hard references. Prefer events over direct
  cross-system calls for cross-feature seams.

## Common mistakes

- **Logic on a component.** Move it to a system.
- **Component referencing a system.** Reverse it: the system holds the
  component.
- **Storing runtime scratch on a component that should be system-local** (it
  serializes needlessly / replicates). Or the inverse: **storing state that
  must replicate as a system-local field** (clients never see it). State that
  must sync → component `[DataField]` + `Dirty`. Ephemeral scratch → system
  field or a non-replicated component field.
- **`Comp<T>` on an entity you didn't verify has T** → NRE. `TryComp` first.
- **Subscribing in a constructor or field initializer** — subscribe in
  `Initialize()` only.

## Where to look

- Fork: `Content.Server/_Art/Market/MarketSystem.cs`,
  `Content.Shared/_Art/Market/Component/*.cs`.
- The rule: `.agents/rules/ss14-ecs-basics.md`.
- API patterns: `skills/ss14-common-api-patterns.md`.
- Full example: `skills/ss14-examples.md`.
