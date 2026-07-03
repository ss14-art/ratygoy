---
applyTo: 'Content.Client/**'
description: Client assembly guidance — XAML menus, BUI subclasses, overlays, sprites/shaders. No networked components here.
---

# Content.Client

Runs **client only**. Presentation: XAML menus, client-only visuals, input
wiring, client-side BUIs, overlays, sprites/shaders. Does not run on the server.

Read: `AGENTS.md` (root), `.agents/rules/ss14-client-server-shared.md`,
`.agents/rules/ss14-localization.md`, `.agents/skills/ss14-ui-xaml.md`,
`.agents/skills/ss14-ui-bui.md`.

- Fork code: `Content.Client/_Art/<Feature>/...`, namespace
  `Content.Client._Art.<Feature>[.<Sub>]` (BUI in `BUI/`, menus in `UI/`).
- Don't put networked components here — they must be in Shared so the server
  knows the type.
- Don't send network messages from XAML menus — the menu raises `Action`s; the
  `BoundUserInterface` translates to `SendMessage`.
- XAML: `FancyWindow`, `[GenerateTypedNameReferences]`, `RobustXamlLoader.Load(this)`,
  `Name=` for typed refs (no `FindChild`), `{Loc fork-...}` for all text.
- May `using Content.Shared.*`, `Robust.Client.*`. Cannot `using
  Content.Server.*`.
- Overlays are client-side; only draw for entities the client knows (PVs).
