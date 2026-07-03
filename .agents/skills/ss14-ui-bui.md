# Skill: BUI (Bound User Interface)

**When to read:** you're making a machine/console/machine-opened menu that a
player interacts with in-world (vend, sale console, fabricator, cargo console).
This skill is **BUI-specific**. For the XAML menu itself see
`skills/ss14-ui-xaml.md`; for admin/player-panel EUIs see
`skills/ss14-ui-eui.md`.

## The BUI flow

A BUI is an in-world entity UI: a player opens it (click + verb / interaction),
the server computes state, the client renders a window, the client sends
messages (button clicks), the server acts. It spans three assemblies — get the
split right (`ss14-client-server-shared.md`).

The fork **Market** is the reference. Mirror its layout exactly for a new
console feature.

## Shared layer (`Content.Shared/_Art/<F>/BUI/`)

1. **UI key enum** — identifies which UI on an entity that may have several:

   ```csharp
   public enum MarketConsoleUiKey : byte { Sale, Request }
   ```

2. **Messages** — client → server, one per action. `[Serializable,
   NetSerializable]`, extend `BoundUserInterfaceMessage`:

   ```csharp
   [Serializable, NetSerializable]
   public sealed partial class MarketCreateListingMessage : BoundUserInterfaceMessage
   {
       public string LotName = string.Empty;
       public int Price;
       public MarketCreateListingMessage(string lotName, int price) { LotName = lotName; Price = price; }
   }
   ```

3. **State** — server → client, the data to render. `[Serializable,
   NetSerializable, DataDefinition]`, extend `BoundUserInterfaceState`. Fields
   are `[DataField]`. Entities over the wire are `NetEntity` (see
   `ss14-networking.md`):

   ```csharp
   [Serializable, NetSerializable, DataDefinition]
   public sealed partial class MarketSaleConsoleInterfaceState : BoundUserInterfaceState
   {
       [DataField] public Dictionary<string, int> ContainerContents = new();
       [DataField] public List<MarketListingData> ActiveListings = new();
       [DataField] public int MaxListings;
       [DataField] public int Balance;
       // constructor...
   }
   ```

## Server layer (`Content.Server/_Art/<F>/`)

The owning `EntitySystem` wires handlers with the `Subs.BuiEvents<TComp>`
helper and pushes state with `UserInterfaceSystem`:

```csharp
[Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

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

private void OnSaleUIOpened(EntityUid uid, MarketSaleConsoleComponent comp, BoundUIOpenedEvent args)
    => UpdateSaleState(uid, comp, args.Actor);

private void UpdateSaleState(EntityUid uid, MarketSaleConsoleComponent comp, EntityUid player)
{
    // ...gather data...
    _uiSystem.SetUiState(uid, MarketConsoleUiKey.Sale,
        new MarketSaleConsoleInterfaceState(containerContents, activeListings, comp.MaxListingsPerPlayer, balance));
}
```

- `args.Actor` is the player who triggered the message (verify perms/conditions
  before acting — see `On → Try → Can → Do` in `ss14-code-style.md`).
- Re-send state whenever the underlying data changes (after a create/cancel,
  re-open, or a periodic tick if live).
- The prototype must declare the UI:

  ```yaml
  - type: UserInterface
    interfaces:
      enum.MarketConsoleUiKey.Sale:
        type: MarketSaleConsoleBoundUserInterface   # the CLIENT BUI type
  ```

## Client layer (`Content.Client/_Art/<F>/BUI/` + `UI/`)

The `BoundUserInterface` subclass binds the UI key to the menu:

```csharp
public sealed class MarketSaleConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables] private MarketSaleMenu? _menu;

    public MarketSaleConsoleBoundUserInterface(EntityUid owner, Enum uiKey)
        : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<MarketSaleMenu>();
        _menu.OnCreateListing += (name, price) => SendMessage(new MarketCreateListingMessage(name, price));
        _menu.OnCancelListing += id => SendMessage(new MarketCancelListingMessage(id));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not MarketSaleConsoleInterfaceState s) return;
        _menu?.Populate(s.ContainerContents, s.ActiveListings, s.MaxListings);
    }
}
```

- `this.CreateWindow<T>()` creates and tracks the menu; it auto-closes with the
  BUI. Don't hand-manage window lifetime.
- Menu callbacks → `SendMessage(new ...Message(...))` (client→server).
- `UpdateState` → cast the state and call a `Populate(...)` method on the menu.
- The menu itself is XAML — see `skills/ss14-ui-xaml.md`.

## Pitfalls

- **Message/state not `[Serializable, NetSerializable]`** → silently broken.
- **Message/state in the wrong assembly** → must be Shared.
- **Entity in state/message as `EntityUid`** → use `NetEntity` + convert at the
  boundary.
- **Forgetting to re-push state after an action** → stale UI.
- **Not declaring the UI in the prototype's `UserInterface` map** → UI won't
  open / wrong type.
- **`Open()` not calling `base.Open()`** → lifecycle breaks.
- **Acting on a message without checking `args.Actor` permissions** → anyone
  can trigger server effects.

## Where to look

- Fork reference: `Content.Shared/_Art/Market/BUI/`,
  `Content.Server/_Art/Market/MarketSystem.cs`,
  `Content.Client/_Art/Market/BUI/`.
- Helper: `Subs.BuiEvents` (see `Content.Shared/Inventory/RelaySubscriptionHelpers.cs`).
- Rules: `ss14-client-server-shared.md`, `ss14-networking.md`,
  `ss14-prediction.md`.
