# Skill: PVs (entity visibility / culling)

**When to read:** you're dealing with what entities a client "sees," sending
updates only to relevant players, or something isn't appearing/disappearing
right for a distant player.

## What PVs is

"PVS" (Potentially Visible Set) is the engine's per-client culling: a client
only receives state updates (and entity spawns) for entities within its visible
range/viewport. This is why a far player doesn't see a machine's state change —
the server doesn't send it to them. It's a **bandwidth** optimization, not a
security one (don't rely on PVs to hide secrets; a client could be modified).

## Implications for your code

- **State updates are auto-filtered.** `Dirty(uid, comp)` queues a resend, but
  only clients that currently PVs-see the entity get it. That's usually what
  you want.
- **Sounds:** `PlayPvs` respects this — only seeing players hear it. For
  everyone-everywhere sounds use `PlayGlobal` / the global sound system (see
  `ss14-audio.md`).
- **UI/BUI state:** a BUI is sent to the player who opened it (and anyone the
  server chooses), independent of PVs.
- **Newly-PVs-visible entities:** the server sends their full state when they
  enter a client's view. You don't do anything special.

## Forcing visibility (PVs overrides)

Sometimes an entity must always be sent to a client regardless of distance
(e.g. a global HUD element, a ghost-follow target). There are component-driven
mechanisms for this. Search the codebase for `PvsOverride`/`RequirePvs`-style
components before reaching for a manual send. Use sparingly — overriding PVs
defeats its purpose and costs bandwidth.

## "The client doesn't see my change"

Most "distant player doesn't see it" bugs are one of:

1. **They're outside PVs range.** Expected behavior. If they *should* see it
   globally, use a global mechanism (global sound, admin chat, a HUD overlay),
   not a positional `Dirty`.
2. **Missing `Dirty`.** They're in range but never got the update — see
   `ss14-networking.md`.
3. **The component isn't networked** (not `[RegisterComponent]` / state not on
   a replicated component).

Distinguish 1 from 2 by checking whether a *near* player sees it.

## Predicted/ephemeral entities

Spawned entities that are predicted exist on the client before the server
confirms; PVs still governs which clients track them. Don't try to bypass PVs
for prediction — the engine handles it.

## Pitfalls

- **Treating PVs as security.** It's bandwidth culling. Secrets must be
  server-side (don't put them on a networked component the client could read).
- **Global spam via `PlayGlobal`/PVs-override** for things that are really
  positional. Match the audience to the event.
- **Forgetting that a far admin/ghost won't get positional `Dirty` updates.**
  If staff tools need global visibility, use the staff-overlays/global
  mechanisms, not positional ones.

## Where to look

- `RobustToolbox/.../Pvs*` (engine — read-only; `ss14-engine-edits.md`).
- `Content.Shared/Audio/SharedGlobalSoundSystem.cs` for the "everyone hears
  this" pattern.
- Overlays in `Content.Client/Overlays/` (e.g. health bars, crew icons) for
  always-on client visuals that don't depend on PVs of a single entity.
