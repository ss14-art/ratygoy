# Skill: NPC / AI systems

**When to read:** you're making a mob behave autonomously — pathfinding,
targets, behavior trees (HTN), queries.

## Where NPC lives

- `Content.Server/NPC/` — the authoritative NPC logic:
  - `HTN/` — Hierarchical Task Network, the primary behavior system.
  - `Pathfinding/` — pathfinding systems + the pathfinder.
  - `Queries/` — HTN primitive queries (find target, etc.).
  - `Components/`, `Events/`, `Systems/`, `UI/`, `Commands/`.
- `Content.Shared/NPC/` (if present) — shared NPC components/state.
- Fork NPC → `Content.Server/_Art/NPC/...` (mirror), `Content.Shared/_Art/NPC/`
  for shared bits.

NPCs are **server-authoritative** — the server runs the behavior, clients see
the result via networked components (position, animation state). Don't run NPC
decision logic on the client.

## HTN (the primary behavior tool)

An HTN is a tree of tasks: **primitive tasks** (do a thing) and **compound
tasks** (pick a subtree based on the world state via a `NPCBlackboard`). The
engine's HTN system ticks each NPC's active plan.

- A `NPCTaskSystem` / primitive task is a system method decorated/registered
  as a task. It reads/writes the blackboard.
- Compound tasks defined in YAML (or code) choose among subtrees by conditions.
- The blackboard (`NPCBlackboard.cs`) is the per-NPC world-state key-value store
  tasks read from / write to (target, position, has-path, etc.).

To add a fork behavior:

1. Prefer composing existing primitive tasks (move-to, melee, pick-up) into a
   fork HTN defined in a fork YAML prototype, before writing new primitives.
2. If you need a new primitive, mirror an existing one in
   `Content.Server/_Art/NPC/HTN/...`, register it, read/write the blackboard.
3. Keep it server-side; network the *result* (e.g. the mob moving) not the
   decision.

## Pathfinding

- `PathfindingSystem` / the pathfinder compute paths on the server's grid
  representation.
- Request a path via the API; the NPC's movement system follows it.
- Fork pathfinding tweaks → `_Art`; don't edit the upstream pathfinder without
  marking (`ss14-fork-editing.md`).

## Components

- `NPCComponent` — marks an entity as an NPC + holds the active HTN plan.
- `HTNComponent`/config — which HTN prototype the NPC runs.
- Targeting components, perceiver (senses) components.

## Pitfalls

- **Running NPC logic client-side** — it's server-authoritative. The client
  only renders.
- **Heavy per-tick work** — NPCs are many; a per-tick O(n) scan over all
  entities per NPC is O(n²). Use the queries/spatial systems (`EntityLookup`,
  the NPC query primitives) and the pathfinder's batching.
- **Writing a full bespoke behavior tree** when HTN + existing primitives
  suffice. Compose first.
- **Mutating the blackboard from the wrong scope** — tasks own their reads/
  writes; don't poke it from unrelated systems.
- **Forgetting to network the visible result** — a mob that "decided" but
  doesn't visibly move/attack means the networked component wasn't `Dirty`'d
  or the movement wasn't predicted/replicated.

## Where to look

- `Content.Server/NPC/HTN/`, `Content.Server/NPC/Pathfinding/`,
  `Content.Server/NPC/Queries/`, `Content.Server/NPC/Systems/`.
- `NPCBlackboard.cs` for the world-state store.
- Existing HTN YAML prototypes under `Resources/Prototypes/...` (search for
  HTN prototype definitions) for the composition style.
- Rules: `ss14-client-server-shared.md` (NPC logic server-side),
  `ss14-prediction.md` (movement result replicated).
