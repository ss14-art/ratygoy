# Content.Client — subtree router

`Content.Client` runs **client only**. Presentation: XAML menus, client-only
visuals, input wiring, client-side BUIs, overlays, sprites/shaders. Does not
run on the server.

## Read first

- Root `AGENTS.md` (router).
- `.agents/rules/ss14-client-server-shared.md` — when Client vs Shared.
- `.agents/rules/ss14-localization.md` — XAML uses `{Loc ...}`.
- `.agents/rules/ss14-prediction.md` — client predicts via shared systems.
- `.agents/rules/ss14-ecs-basics.md`, `ss14-code-style.md`.

## Fork code here

- `Content.Client/_Art/<Feature>/...`, namespace `Content.Client._Art.<Feature>[.<Sub>]`.
- BUI subclasses + XAML menus (`BUI/` + `UI/`), client overlays, client visual
  systems, input handlers.

## Domain skills that apply in Client

- `ss14-ui-bui.md` — the **Client** half of a BUI (`BoundUserInterface` +
  `CreateWindow`, `UpdateState`).
- `ss14-ui-xaml.md` — XAML menus, `FancyWindow`, `[GenerateTypedNameReferences]`.
- `ss14-ui-eui.md` — admin/player panels (`BaseEui`), client side.
- `ss14-sprite-overlays-shaders.md` — overlays + sprite rendering + shaders
  are client-side.
- `ss14-pvs.md` — client receives only PVs-visible entities.
- `ss14-audio.md` — `PlayLocal` for client-only sounds.

## Hard rules specific to Client

- May `using Content.Shared.*` and `Robust.Client.*`. **Cannot** `using
  Content.Server.*`.
- **Don't put networked components here.** Components the server replicates
  must be in Shared so the server knows the type. Client-only visual scratch
  can be a client component.
- **Don't send network messages from XAML menus.** The menu raises `Action`s;
  the `BoundUserInterface` translates them to `SendMessage`. See
  `ss14-ui-xaml.md`.
- **No raw English in XAML** — `Text="{Loc fork-...}"` always.
- Overlays are client-side; only draw for entities the client knows about
  (PVs).

## Common mistakes

- Putting a BUI message/state class here instead of Shared → server can't
  serialize it.
- `FindChild` in XAML code-behind instead of `[GenerateTypedNameReferences]` +
  `Name=`.
- Literal `Text="Sell"` instead of `{Loc ...}`.
- Predicted logic written client-only instead of shared → diverges from server.
- Networked component defined here → server build doesn't know the type.
