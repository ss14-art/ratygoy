# Skill: client / server / shared (extended)

**When to read:** you're deciding which assembly a piece of code belongs in, or
wiring a feature that spans client and server. The rule
`ss14-client-server-shared.md` is the short form.

## The three assemblies, again

- **`Content.Shared`** — compiled into both client and server. Prediction,
  components, networked state, BUI messages/state, shared events. **Default.**
- **`Content.Server`** — server only. Authority: DB, persistence, spawn truth,
  admin, round logic, irreversible effects.
- **`Content.Client`** — client only. Presentation: XAML, client visuals,
  input wiring, client-side BUI.

Fork mirrors: `Content.{Shared,Server,Client}/_Art/...`.

## The decision tree

```
Does the client need to react to or predict this?
├─ Yes → Content.Shared (shared system + networked component)
└─ No
   └─ Is it server authority (DB, spawn truth, admin, round)?
      ├─ Yes → Content.Server
      └─ No (pure client presentation) → Content.Client
```

When unsure between Shared and Server, pick Shared. A server-only system can't
be predicted and will feel laggy; a shared system that does something
server-only is easy to split (raise a server event for the irreversible part).

## Why prediction forces Shared

The client runs the *same* shared handler to predict, then the server runs it
to confirm. If the handler is in `Content.Server`, the client can't run it, so
there's no prediction — the action waits for the round trip. That's the bug
`ss14-prediction.md` is about. So: **interaction logic that should feel
responsive lives in `Content.Shared`**, with irreversible server-only effects
split out.

## The BUI split (the canonical cross-assembly feature)

A Bound User Interface naturally spans all three. Mirror the fork Market:

**Shared** (`Content.Shared/_Art/Market/BUI/`):
- UI key enum: `MarketConsoleUiKey` (`Sale`, `Request`).
- Messages: `MarketCreateListingMessage`, `MarketCancelListingMessage`,
  `MarketBuyListingMessage` — each `[Serializable, NetSerializable,
  BoundUserInterfaceMessage]`.
- State: `MarketSaleConsoleInterfaceState`,
  `MarketRequestConsoleInterfaceState` — each `[Serializable, NetSerializable,
  DataDefinition]` extending `BoundUserInterfaceState`.
- The data carried by state: `MarketListingData` — `[Serializable,
  NetSerializable, DataDefinition]` with `[DataField]`s.

**Server** (`Content.Server/_Art/Market/`):
- `MarketSystem : EntitySystem` — owns the UI. `Subs.BuiEvents<TComp>(uiKey,
  subs => { ... })` to wire message handlers. Computes state and calls
  `_uiSystem.SetUiState(uid, uiKey, new ...InterfaceState(...))`.

**Client** (`Content.Client/_Art/Market/BUI/` + `UI/`):
- `MarketSaleConsoleBoundUserInterface : BoundUserInterface` — `Open()` creates
  the menu via `this.CreateWindow<MarketSaleMenu>()`, wires menu callbacks to
  `SendMessage(new MarketCreateListingMessage(...))`. `UpdateState(state)`
  populates the menu from the state.
- `MarketSaleMenu.xaml` + `.xaml.cs` — the XAML UI (`FancyWindow`,
  `[GenerateTypedNameReferences]`). See `skills/ss14-ui-xaml.md`.

This split is the template for any console/menu feature. Get the assembly of
each piece right or it won't compile (Shared can't reference Server/Client) or
won't replicate (messages/state must be `NetSerializable` in Shared).

## References across assemblies

- Shared ← referenced by Server and Client. Shared can't reference either.
- Server ← references Shared + `Robust.Server`. Can't reference Client.
- Client ← references Shared + `Robust.Client`. Can't reference Server.

So:
- `using Content.Server.*` from Shared or Client → **compile error**.
- `using Content.Client.*` from Shared or Server → **compile error**.
- A shared system can't call a server-only system directly. Raise an event the
  server system handles, or split the server-only part out.

## Fork namespace mirror

| Layer | Folder | Namespace |
|---|---|---|
| Shared component | `Content.Shared/_Art/<F>/Components/` | `Content.Shared._Art.<F>.Components` |
| Shared BUI msg/state | `Content.Shared/_Art/<F>/BUI/` | `Content.Shared._Art.<F>.BUI` |
| Server system | `Content.Server/_Art/<F>/` | `Content.Server._Art.<F>` |
| Client BUI | `Content.Client/_Art/<F>/BUI/` | `Content.Client._Art.<F>.BUI` |
| Client menu | `Content.Client/_Art/<F>/UI/` | `Content.Client._Art.<F>.UI` |

Match the fork Market layout exactly for new console/menu features.

## Common mistakes

- **BUI message in Server** → client can't deserialize → UI broken. Move to
  Shared.
- **Networked component in Client** → server doesn't know the type → won't
  replicate. Move to Shared.
- **Shared system calling a Server system** → won't compile in the Client
  build. Raise an event instead.
- **Predicted logic in Server** → laggy. Move the shared part to Shared.
- **Client reading server-only component** → type missing on client. The
  component must be Shared if the client reads it.

## Where to look

- Fork reference feature: the Market (all three assemblies).
- Rules: `ss14-client-server-shared.md`, `ss14-prediction.md`,
  `ss14-networking.md`.
- BUI skill: `skills/ss14-ui-bui.md`.
