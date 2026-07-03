# Skill: transform / physics

**When to read:** you're moving entities, reading/changing position, parenting,
collision, joints, or movement speed.

## Transform — `TransformSystem` + `TransformComponent`

`[Dependency] private readonly TransformSystem _transform = default!;`.
`Transform(uid)` gets the component.

- **Read position**: `_transform.GetWorldPosition(uid)` → `Vector2`.
  `GetWorldPositionRotation(uid)` → `(Vector2, Angle)`.
- **Coordinates**: `Transform(uid).Coordinates` — an `EntityCoordinates`
  relative to its parent. Use this for spawn locations, relative moves.
- **Map**: `Transform(uid).MapUid`, `Transform(uid).MapId`.
- **Move**: `_transform.SetWorldPosition(uid, vec)`,
  `_transform.SetCoordinates(uid, coords)`.
- **Parent**: `_transform.SetParent(uid, newParent)`. Detach to map:
  `_transform.AttachToGridOrMap(uid)`.
- **Map entity**: `_map.GetMapOrInvalid(mapId)` (the fork Market uses this to
  find the default round map).

Prefer the `TransformSystem` API over mutating `TransformComponent` fields
directly — the system keeps the parent/child/coordinate invariants consistent.

## Physics — `SharedPhysicsSystem` / `PhysicsComponent`

- `FixturesComponent` for collision shapes. `PhysicsComponent` for body type
  (Dynamic/Static/Animated), velocity.
- `CollisionGroup` (`Content.Shared/Physics/CollisionGroup.cs`) — bitmask of
  which groups collide with which. Set on a fixture.
- `JointComponent` / `SharedJointSystem` for constraints (cuffs, chains).
- `SharedBroadphaseSystem` underlies spatial queries.

## Movement

- `MovementSpeedModifierSystem` (`Content.Shared/Movement/Systems/`) — the
  canonical way to modify a mob's speed (walk/run) via weighty modifiers rather
  than writing to `MovementSpeedModifierComponent` directly.
- `MoverController` / input movement is client-predicted; speed modifiers are
  shared so both sides agree.
- `FrictionContactsSystem`, `MovementIgnoreGravitySystem` for special movement
  behaviors.

## Spatial lookup

`[Dependency] private readonly EntityLookupSystem _lookup = default!;` (the
fork Market uses it):

```csharp
_lookup.GetEntitiesInRange<StorageComponent>(uid, radius, set);
```

- `GetEntitiesInRange<T>(uid, radius, HashSet<Entity<T>>)` — typed, spatial.
- `GetEntitiesInRange<T>(coords, radius, ...)`.
- Prefer `_lookup` over manual distance checks against all entities.

## Prediction note

Position/movement is predicted. Mutating transform on the client predicts; the
server reconciles. For server-authoritative teleports (admin, shuttles), the
engine handles the correction. Don't fight it: use the system API, let
prediction do its job. See `ss14-prediction.md`.

## Pitfalls

- **Mutating `TransformComponent` fields directly** — use `TransformSystem`;
  direct writes break parent/coordinate invariants.
- **`GetWorldPosition` in a hot loop without an `EntityQuery`** — there's a
  query-accepting overload; use it in bulk paths.
- **Wrong `CollisionGroup`** — a fixture with no matching mask won't collide
  with what you expect. Check the bitmask.
- **Setting velocity on a Static body** — no-op. Set `BodyType = BodyType.Dynamic`.
- **Teleporting without detaching from a moved parent** — the entity rides the
  parent. `_transform.SetParent(uid, map)` or `AttachToGridOrMap` first when
  needed.

## Where to look

- `RobustToolbox/Robust.Shared/GameObjects/Systems/SharedTransformSystem*.cs`
  for the full API (read-only reference; don't edit — `ss14-engine-edits.md`).
- `Content.Shared/Movement/Systems/` for movement modifiers.
- `Content.Shared/Physics/` for collision/joints.
- Fork user of lookup + transform: `Content.Server/_Art/Market/MarketManager.cs`.
