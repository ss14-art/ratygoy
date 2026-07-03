---
applyTo: 'Content.Shared/**'
description: Shared assembly guidance — prediction, networked components, BUI messages/state live here.
---

# Content.Shared

Runs on **both client and server**. Prediction, components, BUI messages/state,
shared interaction logic. **Default to here for logic.**

Read: `AGENTS.md` (root), `.agents/rules/ss14-client-server-shared.md`,
`.agents/rules/ss14-prediction.md`, `.agents/rules/ss14-networking.md`.

- Fork code: `Content.Shared/_Art/<Feature>/...`, namespace
  `Content.Shared._Art.<Feature>[.<Sub>]`.
- Don't `using Content.Server.*` or `Content.Client.*` here — Shared can't see
  them.
- BUI messages/state must be `[Serializable, NetSerializable]` and live here.
- Networked components live here (both sides need the type).
- `EntityUid` in logic, `NetEntity` on the wire.
- Predicted player-interaction logic belongs here, not in `Content.Server`.
