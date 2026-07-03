# Content.Shared — subtree router

`Content.Shared` runs on **both client and server**. This is where prediction
happens, where components/BUI state/messages live, where shared interaction
logic goes. **Default to here for logic.**

## Read first

- Root `AGENTS.md` (router).
- `.agents/rules/ss14-client-server-shared.md` — why Shared.
- `.agents/rules/ss14-prediction.md` — shared logic is predicted logic.
- `.agents/rules/ss14-networking.md` — components here get replicated.
- `.agents/rules/ss14-ecs-basics.md`, `ss14-code-style.md`.

## Fork code here

- `Content.Shared/_Art/<Feature>/...`, namespace `Content.Shared._Art.<Feature>[.<Sub>]`.
- Components (networked), BUI messages/state (`[Serializable, NetSerializable]`),
  shared systems, shared events, shared helpers.

## Domain skills that apply in Shared

- `ss14-ui-bui.md` — the **Shared** half of a BUI (UI key, messages, state).
- `ss14-atmos.md` — shared atmos components/`GasMixture` (server systems are
  authoritative, but shared types live here).
- `ss14-transform-physics.md` — `TransformSystem`/movement speed modifiers are
  shared; both sides need them for prediction.
- `ss14-audio.md` — `SharedAudioSystem` is shared.
- `ss14-sprite-overlays-shaders.md` — `AppearanceComponent`/visuals enums are
  shared (visualizers live client-side but read shared data).
- `ss14-pvs.md` — relevant when reasoning about what replicates.
- `ss14-npc-ai.md` — shared NPC components if any (logic is server-side).

## Hard rules specific to Shared

- **Don't `using Content.Server.*` or `Content.Client.*`** here — Shared can't
  see them; it compiles into both. A compile error here means you pulled a
  server/client-only dependency up. Move that dependency down instead, or raise
  an event.
- BUI messages/state **must** be `[Serializable, NetSerializable]` and live
  here, or the other side can't deserialize.
- Networked components live here (both sides need the type).
- `EntityUid` in logic is fine; `NetEntity` on the wire (in messages/state).

## Common mistake

Putting predicted interaction logic in `Content.Server` instead of here →
laggy, non-predicted action. If the client must react, it belongs in Shared.
