# Skill: audio

**When to read:** you're playing sounds, adding ambient audio, jukebox content,
or sound-collection prototypes.

## System

`SharedAudioSystem` (injected as `[Dependency] private readonly SharedAudioSystem
_audio = default!;`). It's shared so the same call works client/server; the
engine routes the actual playback.

```csharp
_audio.PlayPvs(new SoundPathSpecifier("/Audio/_Art/market/cash.ogg"), uid);
```

## Where fork audio lives

`Resources/Audio/_Art/...`. Mirror upstream's `Resources/Audio/` structure.
Reference fork audio by absolute path `/Audio/_Art/...`. Don't reuse upstream
audio paths for fork-only sounds; copy/license the asset into `_Art` if needed
(see `ss14-porting-and-licensing.md`).

## Play variants (pick by who hears it)

- `PlayPvs(spec, uid)` — heard by players who can currently see the entity
  (PVs-filtered). **The default choice** for positional entity sounds.
- `PlayPvs(spec, uid, audioParams)` — with params (volume, variation).
- `PlayLocal(...)` — client-local only (e.g. a UI click). Not networked.
- `PlayGlobal(...)` — everyone, no position (announcements, round events).
  Usually paired with a global-sound system so it's not spammed.
- `PlayEntity(...)` — at a specific entity, with explicit filtering.
- `PlayCoordinates(...)` — at coordinates.

For predicted client-side sounds (e.g. a UI button press), `PlayLocal` is
correct. For "this machine did a thing everyone nearby hears", `PlayPvs`.

## Sound specifiers

- `SoundPathSpecifier("/Audio/...")` — a single file.
- `SoundCollectionSpecifier("SomeCollection")` — picks a random sound from a
  `type: soundCollection` prototype. Good for varied impacts/footsteps.
- A `SoundPathSpecifier` can carry `Params` (volume, pitch variation, max
  distance).

## Component-driven audio

Prefer data-driven over one-off `PlayPvs` calls:

- `AmbientSoundComponent` (`Content.Shared/Audio/AmbientSoundComponent.cs`) —
  looping positional ambience, range/volume via fields. The fork can add fork
  ambient sounds by adding the component in a prototype.
- `SoundWhileAliveComponent`, jukebox components, etc. — read the existing ones
  before inventing a new audio component.

## `soundCollection` prototype

```yaml
# Resources/Prototypes/_Art/Audio/fork_sounds.yml
- type: soundCollection
  id: ForkWidgetPop
  files:
  - /Audio/_Art/Widget/pop1.ogg
  - /Audio/_Art/Widget/pop2.ogg
```

Reference via `new SoundCollectionSpecifier("ForkWidgetPop")` or as a
`ProtoId<SoundCollectionPrototype>` field.

## Pitfalls

- **Wrong filter** — `PlayGlobal` for a positional sound spams everyone; `PlayPvs`
  for a global announcement means far players miss it. Match audience to intent.
- **Predicted vs authoritative** — a server-only `PlayPvs` for a predicted
  action plays late. Predict the sound client-side where it makes sense (or use
  the engine's predicted audio paths).
- **Raw path string** — use `SoundPathSpecifier` / `SoundCollectionSpecifier` /
  `ProtoId<SoundCollectionPrototype>`, not `string`.
- **Missing asset** — the `.ogg` must exist at the path or you get silence +
  a warning. Fork audio under `_Art`.
- **Volume** — SS14 audio is loud by default; tune with `AudioParams`.

## Where to look

- API: `Content.Shared/Audio/SharedContentAudioSystem.cs`,
  `SharedAudioSystem` (Robust).
- Components: `Content.Shared/Audio/*.cs`.
- Fork example reference: a prototype with `- type: AmbientSound` or a system
  calling `_audio.PlayPvs`.
