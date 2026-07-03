# Skill: worked examples

**When to read:** you want a concrete, end-to-end pattern to copy. Each example
is the minimal correct shape; adapt names/keys to your feature. All fork
examples use `_Art` and `fork-` locale prefixes.

The reference feature actually in this repo is the **Market** — read
`Content.Server/_Art/Market/`, `Content.Shared/_Art/Market/`,
`Content.Client/_Art/Market/`, `Resources/Prototypes/_Art/market.yml` alongside
these.

---

## Example 1 — simple Component + System (fork)

`Content.Shared/_Art/Widget/Components/WidgetComponent.cs`:

```csharp
namespace Content.Shared._Art.Widget.Components;

[RegisterComponent]
public sealed partial class WidgetComponent : Component
{
    [DataField]
    public int Charges = 3;

    [DataField]
    public ProtoId<SoundCollectionPrototype> UseSound = "GenericPop";
}
```

`Content.Server/_Art/Widget/WidgetSystem.cs`:

```csharp
using Content.Shared._Art.Widget.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Art.Widget;

public sealed class WidgetSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WidgetComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnInteractHand(EntityUid uid, WidgetComponent comp, InteractHandEvent args)
    {
        if (!TryUseWidget(uid, comp, args.User))
            return;
    }

    private bool TryUseWidget(EntityUid uid, WidgetComponent comp, EntityUid user)
    {
        if (!CanUseWidget(uid, comp))
            return false;
        DoUseWidget(uid, comp, user);
        return true;
    }

    private bool CanUseWidget(EntityUid uid, WidgetComponent comp)
        => comp.Charges > 0;

    private void DoUseWidget(EntityUid uid, WidgetComponent comp, EntityUid user)
    {
        comp.Charges--;
        Dirty(uid, comp);
        _audio.PlayPvs(comp.UseSound, uid);
        _popup.PopupEntity(Loc.GetString("fork-widget-used"), uid, user);
    }
}
```

Notes: `On → Try → Can → Do`; `Dirty` after mutating replicated state; localized
popup; typed `ProtoId<SoundCollectionPrototype>`; interaction in Shared would be
needed for prediction (here shown server-side for brevity — for a real
hand-interaction, put `OnInteractHand` logic in a Shared system; see
`ss14-prediction.md`).

---

## Example 2 — item/structure prototype (fork)

`Resources/Prototypes/_Art/widget.yml`:

```yaml
- type: entity
  id: ForkWidget
  name: fork-widget-name
  description: fork-widget-desc
  parent: BaseItem
  components:
  - type: Widget
    charges: 5
  - type: Sprite
    sprite: _Art/Objects/widget.rsi
    state: icon
  - type: Item
    size: Small
```

`Resources/Locale/en-US/_Art/widget.ftl`:

```ftl
fork-widget-name = widget
fork-widget-desc = A small widget with limited charges.
fork-widget-used = You use the widget.
```

- `name`/`description` are locale keys, not raw text.
- Fork sprite path under `_Art`.
- Parent a `Base*` to inherit standard components.

---

## Example 3 — reagent in YAML (fork)

Reagents are `type: reagent` prototypes. `name`/`desc` are locale keys.

`Resources/Prototypes/_Art/Reagents/fork_reagents.yml`:

```yaml
- type: reagent
  id: ForkTonic
  name: reagent-name-fork-tonic
  group: Medical
  desc: reagent-desc-fork-tonic
  flavor: forktonic
  color: "#4CA3DD"
  physicalDesc: reagent-physical-desc-fork-tonic
  metabolisms:
    Medicine:
      effects:
      - !type:HealthChange
        damage:
          groups:
            Toxin: -2
```

`Resources/Locale/en-US/_Art/reagents.ftl`:

```ftl
reagent-name-fork-tonic = fork tonic
reagent-desc-fork-tonic = A bracing tonic of fork origin.
reagent-physical-desc-fork-tonic = faintly glowing and blue
```

(Match the upstream reagent locale key style: `reagent-name-*`,
`reagent-desc-*`, `reagent-physical-desc-*`. Fork ids clearly named.)

---

## Example 4 — `GenericVisualizer`

Visuals keyed off an enum's states. The component raises `VisualizeEvent` /
sets a `AppearanceComponent` data key; the prototype maps states to layer
changes.

`Resources/Prototypes/_Art/Entities/Objects/fork_console.yml`:

```yaml
- type: entity
  id: ForkConsole
  parent: BaseStructureDynamic
  components:
  - type: Sprite
    noRot: true
    sprite: _Art/Structures/fork_console.rsi
    layers:
    - state: base
    - state: on
      map: [ "enum.ForkConsoleVisuals.Layer" ]
      visible: false
  - type: GenericVisualizer
    visuals:
      enum.ForkConsoleVisuals.Powered:
        enum.ForkConsoleVisuals.Layer:
          True: { visible: true }
          False: { visible: false }
```

The shared enum (mirrors upstream `CreamPiedVisuals` style):

```csharp
public enum ForkConsoleVisuals : byte { Powered, Layer }
```

System toggles appearance data: `_appearance.SetData(uid,
ForkConsoleVisuals.Powered, isPowered);`.

---

## Example 5 — localization (string + prototype + marking)

String in code:

```csharp
_popup.PopupEntity(
    Loc.GetString("fork-market-listing-created",
        ("lotName", lotName), ("price", price)),
    uid, player);
```

```ftl
fork-market-listing-created = Listing "{$lotName}" created for {$price} sp.
```

Prototype name/description keys: shown in Example 2.

Marking prototype (cosmetic body marking), fork:

```yaml
# Resources/Prototypes/_Art/Markings/fork_markings.yml
- type: marking
  id: ForkMarkingStripe
  bodyPart: Head
  markingCategory: Head
  speciesRestriction: [ Human ]
  sprites:
  - sprite: _Art/Mobs/Customization/stripe.rsi
    state: stripe
```

with locale `marking-fork-marking-stripe = fork stripe`.

---

## Example 6 — test anchors

`Content.Tests` uses NUnit. A unit test:

```csharp
using NUnit.Framework;

namespace Content.Tests._Art.Widget;

[TestFixture]
public sealed class WidgetSystemTest : ContentUnitTest
{
    [Test]
    public void TestUseDecrementsCharges()
    {
        // arrange + act + assert against the system under test
        Assert.That(1 + 1, Is.EqualTo(2));
    }
}
```

Integration tests live in `Content.IntegrationTests` and spin a real
server+client pair. Use `[Test]`/`[TestFixture]`; prefer the existing
integration test harness (`PoolManager` / `TestMapData`) for
server+client scenarios. Fork integration tests go under
`Content.IntegrationTests/_Art/...`.

For test fixtures that need entities, use the integration pool to get a
server/client pair — don't hand-roll `EntityManager` in a plain unit test
unless the system is pure logic. See the subtree `AGENTS.md` in
`Content.Tests` / `Content.IntegrationTests`.

---

## The non-example: what an upstream edit looks like

If you genuinely must touch an upstream file (last resort after subscribing
from `_Art` fails):

```csharp
// in Content.Server/Cargo/Systems/CargoSystem.cs (UPSTREAM FILE)
        if (comp.Account != null)
        {
            // ss14-art edit start
            RaiseLocalEvent(uid, new ForkCargoAccountUsedEvent(comp.Account, amount));
            // ss14-art edit end
            _bankSystem.TryAdjustBalance(comp.Account.Value, amount);
        }
```

One block, marked, minimal — it only creates a seam (raises an event) that
`_Art` code subscribes to. The bulk of the fork feature lives in `_Art`.

See `ss14-fork-editing.md` for the full marker rules.
