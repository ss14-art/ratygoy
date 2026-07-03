# Skill: EUI (Eui — player/admin panels not bound to an entity)

**When to read:** you're making a UI that's **not** tied to an in-world entity —
admin panels, the character editor, ghost roles, round-end summary, a global
player menu. For entity-bound consoles see `skills/ss14-ui-bui.md`.

## BUI vs EUI — when to use which

- **BUI** (Bound UserInterface): opened on an **entity** (a machine, console,
  vend). State is per-entity. See `skills/ss14-ui-bui.md`.
- **EUI**: opened on a **player/session**, no entity required. Used for admin
  tools, the character/lobby editor, round-end screens, ahelp, ghost role
  picking. State is per-session.

If your UI is "the player opens a panel that isn't a world object," it's an EUI.

## Where EUI lives

- Server: `Content.Server/EUI/` (`BaseEui.cs`, `EuiManager.cs`).
- Client: `Content.Client/EUI/` (`BaseEui.cs`, `EuiManager.cs`).
- Fork EUIs → `Content.{Server,Client}/_Art/EUI/...`, mirroring.

## The shape

A server `Eui` subclass (`Content.Server/EUI/BaseEui.cs`):

```csharp
public sealed class ForkPanelEui : BaseEui
{
    public ForkPanelEui() { }

    public override void Opened()
    {
        base.Opened();
        StateDirty(); // push initial state
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        if (msg is ForkPanelAction action)
        {
            // act, then StateDirty() to refresh
        }
    }

    public override EuiStateBase GetState() => new ForkPanelState { /* ... */ };
}
```

- `Opened()` / `Closed()` lifecycle hooks.
- `HandleMessage` for client→server messages.
- `GetState()` returns the current state; `StateDirty()` flags it to be resent.
- Open it for a player session via `EuiManager` (server side): create the eui,
  `OpenEui(playerSession)`.

The client side (`Content.Client/EUI/BaseEui.cs`):

```csharp
public sealed class ForkPanelEui : BaseEui
{
    private ForkPanelWindow? _window;

    public override void Open()
    {
        base.Open();
        _window = new ForkPanelWindow();
        _window.OnAction += a => SendMessage(new ForkPanelAction(...));
        _window.OpenCentered();
    }

    public override void UpdateState(EuiStateBase state)
    {
        if (state is not ForkPanelState s) return;
        _window?.UpdateState(s);
    }
}
```

The window itself is a normal XAML control (see `skills/ss14-ui-xaml.md`),
opened with `OpenCentered()` / managed manually (no `CreateWindow` here —
that's a BUI helper).

## Messages & state

EUI messages/state cross the network like BUI: `[Serializable, NetSerializable]`.
State extends `EuiStateBase`; messages extend `EuiMessageBase`. They live in
**Shared** (`Content.Shared/_Art/EUI/`) so both sides share the types — same
rule as BUI (`ss14-client-server-shared.md`, `ss14-networking.md`). `NetEntity`
for any entity refs over the wire.

## Pitfalls

- **Using a BUI for a non-entity panel.** Use EUI. A BUI needs an owning entity.
- **EUI state/messages not in Shared / not `[Serializable, NetSerializable]`.**
- **Not calling `StateDirty()` after server-side changes** → stale panel.
- **Managing window lifetime wrong** — EUI windows aren't auto-tracked like
  `CreateWindow`; close them in `Closed()` / handle dispose.
- **Forgetting the player session** — EUIs are per-session; don't assume a
  single global instance.

## Where to look

- `Content.Server/EUI/BaseEui.cs`, `Content.Client/EUI/BaseEui.cs`.
- Existing EUIs: search for `: BaseEui` in `Content.Server` and
  `Content.Client` for concrete examples (admin panels, round-end, ghost
  roles).
- Rules: `ss14-client-server-shared.md`, `ss14-networking.md`.
