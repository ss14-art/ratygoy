# Rule: client / server / shared

SS14 content is split across three assemblies. Picking the wrong one is a top
cause of broken prediction and review churn. Default to **Shared** for logic.

## The three assemblies

- **`Content.Shared`** — runs on **both** client and server. This is where
  prediction happens, where components and BUI state/messages live, where shared
  interaction logic goes. **Default to here.**
- **`Content.Server`** — server only. Authority: persistence, DB, spawning
  authority, admin, round logic, anything irreversible. Does **not** run on the
  client.
- **`Content.Client`** — client only. Presentation: XAML menus, client-only
  visuals, input wiring, prediction-side rendering. Does **not** run on the
  server.

Fork code mirrors this: `Content.Shared/_Art`, `Content.Server/_Art`,
`Content.Client/_Art`.

## The decision

Ask, in order:

1. **Does it need to run on both client and server?** → `Content.Shared`.
   This includes: components, networked state, BUI messages/state, events both
   sides handle, predicted interaction logic, shared helpers.
2. **Is it server authority only** (DB, persistence, spawning-truth, admin,
   round flow, irreversible side effects)? → `Content.Server`.
3. **Is it pure client presentation** (a menu's XAML, client-only visual
   updates, input → message wiring)? → `Content.Client`.

When torn between Shared and Server: **if the client must react to it or
predict it, it's Shared.** Only demote to Server when the client genuinely must
not know.

## Components

- `[RegisterComponent]` components that hold replicated state → **Shared**.
  Both sides need the type. (See `ss14-networking.md`.)
- Server-only scratch data that the client never sees can be a Shared component
  that's only `Dirty`'d server-side, or a Server-only component. Prefer Shared
  for the networked part, Server-only component for secret scratch.

## BUI (Bound User Interface)

The BUI pattern straddles all three:

- **Shared**: the `Enum` UI key, the `[Serializable, NetSerializable]` message
  classes, and the `[Serializable, NetSerializable, DataDefinition]` state
  classes. (See `MarketMessages.cs`, `MarketSaleConsoleInterfaceState.cs`.)
- **Server**: the `EntitySystem` that owns the UI, handles messages, sends
  state via `UserInterfaceSystem.SetUiState`. (See `MarketSystem.cs`.)
- **Client**: the `BoundUserInterface` subclass + the XAML menu. (See
  `MarketSaleConsoleBoundUserInterface.cs`, `MarketSaleMenu.xaml`.)

Get the split right or the BUI won't compile / won't replicate.

## Events

- Events both sides predict → defined/handled in Shared.
- Server-authoritative events (e.g. round start, DB writes) → Server.
- Client input events → often raised client-side, handled in Shared for
  prediction, with the server confirming.

## Hard rules

- A system that does prediction **must not** be Server-only. Move it to Shared.
- A `[Serializable, NetSerializable]` message/state **must** be in Shared, or
  the other side can't deserialize it.
- Don't `using Content.Server.*` from `Content.Shared` or `Content.Client` —
  Shared/Client can't see Server. Server can see Shared.
- Don't `using Content.Client.*` from Shared or Server.
- Client may `using Content.Shared.*`. Server may `using Content.Shared.*` and
  `Robust.Server.*`.

## Quick reference

| Thing | Assembly |
|---|---|
| Networked component | Shared |
| BUI message/state classes | Shared (`NetSerializable`) |
| BUI UI key enum | Shared |
| Predicted interaction logic | Shared |
| Server authority / DB / spawn truth | Server |
| XAML menu + client BUI | Client |
| Locale strings | Resources (not an assembly) |
| Prototypes | Resources (YAML) |

See `skills/ss14-client-server-shared.md` for the extended walkthrough.
