# Rule: networking — `NetEntity`, `Dirty`, networked components

SS14 replicates component state from server to client. You must use the right
primitives or state silently won't sync, and prediction will break.

## Networked components

A component is replicated (server → client) when it's `[RegisterComponent]` and
its fields are `[DataField]` (or otherwise serialized). The engine handles the
wire format. For a field to be *networked* (sent to clients in real time, not
just at map load), it must be on a networked component and you must `Dirty` it
after changing it.

- Components that carry player-visible state live in `Content.Shared` so both
  sides share the type (see `ss14-client-server-shared.md`).
- Fields the client shouldn't see: don't put them on a replicated component, or
  mark them non-replicated. Server-only scratch belongs in a Server-only
  component or a non-networked field.

## `Dirty`

After you mutate a replicated component's state, call:

```csharp
Dirty(uid, comp);      // EntityUid + component
Dirty(ent);            // Entity<TComponent>
```

This marks the component dirty so the engine re-serializes and sends the update
to clients. **Forgetting `Dirty` is the #1 networking bug**: the server's state
is correct, clients never learn. If a player can't see a change that definitely
happened, check for a missing `Dirty`.

- `Dirty` after *every* mutation of replicated state you want clients to see.
- Don't `Dirty` in a tight loop needlessly (it queues a resend), but correctness
  first — a missing `Dirty` is worse than an extra one.

## `NetEntity`

Networking uses `NetEntity`, not `EntityUid`, over the wire. The two are not
interchangeable.

- In **BUI messages and state** that cross the network, reference entities as
  `NetEntity`, not `EntityUid`. See `MarketListingData` for the data shape (it
  avoids entity refs entirely by storing names/counts; when you must send an
  entity, use `NetEntity`).
- Convert at the boundary:
  - `GetEntity(netEntity)` → `EntityUid` (client/server side, when you receive).
  - `GetNetEntity(entityUid)` → `NetEntity` (when you put an entity into a
    message/state).
- On the client, `EntityUid`s from the server may not be valid directly; always
  go through `GetEntity` on the `NetEntity`. The `EntityManager`/`_entityManager`
  has these helpers.

Rule of thumb: **`EntityUid` in logic, `NetEntity` on the wire.** Convert at the
seam.

## `[Serializable, NetSerializable]`

Anything sent over the network (BUI messages, BUI state) must be:

```csharp
[Serializable, NetSerializable, DataDefinition]
public sealed partial class MarketSaleConsoleInterfaceState : BoundUserInterfaceState
{
    [DataField] public Dictionary<string, int> ContainerContents = new();
    [DataField] public List<MarketListingData> ActiveListings = new();
    [DataField] public int MaxListings;
    [DataField] public int Balance;
}
```

- `Serializable` + `NetSerializable` → engine can serialize it over the wire.
- `DataDefinition` + `[DataField]` → the engine's typed serializer handles the
  fields (preferred over manual `[NetSerialize]` for simple data).
- These classes live in **Shared** (see `ss14-client-server-shared.md`).
- Messages extend `BoundUserInterfaceMessage`; state extends
  `BoundUserInterfaceState`.

## Self-check

- [ ] Replicated state mutated → `Dirty` called.
- [ ] Entities in network messages/state are `NetEntity`, converted at the
      boundary with `GetNetEntity`/`GetEntity`.
- [ ] BUI messages/state are `[Serializable, NetSerializable]`, in Shared.
- [ ] Networked components are in Shared (both sides need the type).
- [ ] No `EntityUid` in a `[NetSerializable]` field.
