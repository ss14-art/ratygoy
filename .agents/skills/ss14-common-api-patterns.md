# Skill: common SS14 API patterns

**When to read:** you need the idiomatic way to do a common thing — popups,
audio, prototype lookup, entity queries, container manipulation, transforms.
These are the patterns the fork and upstream use; match them.

## EntitySystem essentials

- `[Dependency]` to pull systems: `[Dependency] private readonly PopupSystem
  _popup = default!;`
- `EntityManager` / `_entityManager` for entity ops (spawn, delete, GetComp).
- `Transform(uid)` / `_transform` (`TransformSystem`) for coordinates, parent,
  world position.
- `MetaData(uid)` for name, description, prototype id.

## Popups

Player-visible feedback. Always localized.

```csharp
[Dependency] private readonly SharedPopupSystem _popup = default!;

_popup.PopupEntity(
    Loc.GetString("fork-market-listing-created", ("lotName", lotName)),
    uid, player);
```

- `PopupEntity(msg, uid, actor)` — popup at the entity, seen by the actor.
- Variants: `PopupClient` (client-predicted), `PopupServer` (server-only),
  `PopupCursor`, `PopupCoordinates`. Pick by who-should-see-it and
  predicted-vs-authoritative.
- String arg via `Loc.GetString` with `("name", value)` tuples.

## Audio

```csharp
[Dependency] private readonly SharedAudioSystem _audio = default!;

_audio.PlayPvs(new SoundPathSpecifier("/Audio/_Art/market/cash.ogg"), uid);
_audio.PlayLocal(audioId, uid); // client-local
```

- Fork audio under `Resources/Audio/_Art/...`. Reference via
  `SoundPathSpecifier` or a `SoundCollectionSpecifier`.
- `PlayPvs` plays for players who can see the entity (PVs-filtered) — the usual
  choice. `PlayLocal` for client-only. See `skills/ss14-audio.md`.

## Prototype lookup (typed)

Prefer `ProtoId<T>` over raw `string`. Resolve via the prototype manager
injected as a system dependency or through a system that wraps it.

```csharp
[Dependency] private readonly IPrototypeManager _proto = default!;

if (_proto.TryIndex(protoId, out var proto)) { ... }
```

- Store references as `ProtoId<StackPrototype>`, `EntProtoId<TComponent>`.
- For spawning from an entity prototype id: `_entityManager.SpawnEntity(entProtoId, coords)`
  or `Spawn` helpers on `EntityManager`/`TransformSystem`.
- See `skills/ss14-prototype-basics.md`.

## Entity queries / spatial lookup

For "all entities with component X" or "all near a point":

```csharp
// typed query enumerator (preferred for bulk)
var query = EntityQueryEnumerator<MarketContainerComponent>();
while (query.MoveNext(out var uid, out var comp)) { ... }

// spatial: entities within radius
_lookup.GetEntitiesInRange<StorageComponent>(uid, radius, _containerSet);
```

The fork Market uses `_lookup` + a `HashSet<Entity<...>>` for the
nearby-container search. Don't iterate `EntityManager` naively in a hot path.

## Containers

```csharp
[Dependency] private readonly SharedContainerSystem _container = default!;

var c = _container.EnsureContainer<Container>(uid, "lot_items");
_container.TryRemoveFromContainer(ent);
_container.Insert(ent, c);
```

- `EnsureContainer<T>` get-or-make.
- `TryRemoveFromContainer` / `Insert` / `CanInsert`.
- The fork Market pulls items out of a `StorageComponent`'s container to move
  them into a listing — match that pattern for item relocation.

## Transform / coordinates

```csharp
[Dependency] private readonly TransformSystem _transform = default!;
[Dependency] private readonly SharedMapSystem _map = default!;

var coords = Transform(uid).Coordinates;
var mapEnt = _map.GetMapOrInvalid(_ticker.DefaultMap);
```

- `Transform(uid).Coordinates` for position.
- `Transform(uid).MapUid` for the map entity.
- `_transform` for set-world-position, attach, detach.

## Raising / handling events (the glue)

```csharp
// raise
RaiseLocalEvent(uid, new FooEvent { Bar = 1 });

// subscribe
SubscribeLocalEvent<FooComponent, FooEvent>(OnFoo);
private void OnFoo(EntityUid uid, FooComponent comp, ref FooEvent args) { }
```

- `ref` on the event when handlers mutate it / set a result field.
- Prefer events over direct cross-system calls for cross-feature seams.

## BUI wiring (server side)

```csharp
Subs.BuiEvents<MarketSaleConsoleComponent>(MarketConsoleUiKey.Sale, subs =>
{
    subs.Event<BoundUIOpenedEvent>(OnSaleUIOpened);
    subs.Event<MarketCreateListingMessage>(OnCreateListing);
});

_uiSystem.SetUiState(uid, MarketConsoleUiKey.Sale,
    new MarketSaleConsoleInterfaceState(containerContents, activeListings, max, balance));
```

See `skills/ss14-ui-bui.md` for the full BUI flow.

## Name / MetaData

```csharp
var name = Name(uid);              // entity display name (localized)
var meta = MetaData(uid);          // MetaDataComponent
var proto = meta.EntityPrototype;  // the prototype, if any
```

The fork Market keys listings by `Name(player)` (the seller's display name).
`Name()` returns the localized name.

## What not to do

- Don't reach into a component's private fields from another system; use the
  system API or an event.
- Don't `EntityManager.GetEntities()` in a loop; use a query enumerator.
- Don't play audio with a raw path string when a `SoundPathSpecifier` /
  `ProtoId` is the typed form.
- Don't `Popup` a raw English string; `Loc.GetString` it.

## Where to look

- Fork: `Content.Server/_Art/Market/MarketSystem.cs`,
  `Content.Server/_Art/Market/MarketManager.cs` — exercises popups (implicitly),
  containers, transform, lookup, prototype manager, BUI.
- Domain skills for the deeper subsystems (`skills/ss14-audio.md`, etc.).
