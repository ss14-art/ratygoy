# Rule: localization is mandatory

**Every human-readable string the game shows must go through `Loc.GetString`
and have a key in an `.ftl` file.** No hardcoded English in `.cs`/`.xaml`/`.yml`
that the player can see. This is non-negotiable; a PR with a raw string literal
shown to players is a review blocker.

## The pattern

1. Add the string to an `.ftl` file under
   `Resources/Locale/en-US/_Art/<feature>.ftl`.
2. Reference it via `Loc.GetString("key", ("arg", value))` in code, or
   `{Loc key}` / `{Loc "key" args...}` in XAML.

### `.ftl`

```ftl
fork-market-listing-created = Listing "{$lotName}" created for {$price} sp.
fork-market-listing-cancelled = Listing "{$lotName}" cancelled.
fork-market-name-prefix = market {$thing}
```

- Keys are kebab-case, prefixed with the fork feature (e.g. `fork-market-`).
  The prefix keeps fork strings separate from upstream and other forks
  (`_NF`, `_RMC14`, etc.).
- Arguments use `{$name}`. Use Fluent features (`{CAPITALIZE(THE($x))}`,
  plurals, etc.) — see existing upstream `.ftl` for the grammar.

### In C#

```csharp
_popup.PopupEntity(
    Loc.GetString("fork-market-listing-created", ("lotName", lotName), ("price", price)),
    uid, player);
```

### In XAML

```xml
<Label Text="{Loc fork-market-sale-title}" />
<Button Name="ListButton" Text="{Loc fork-market-list-button}" Access="Public" />
```

### In YAML (entity name/description)

```yaml
- type: entity
  id: MarketSaleContainer
  name: fork-market-sale-container-name
  description: fork-market-sale-container-desc
```

Then provide `fork-market-sale-container-name = Market sale container` in the
`.ftl`. Entity `name`/`description` are localization keys, not raw text.

## Naming convention

- Fork locale keys: `fork-<feature>-<thing>[-<variant>]`, all kebab-case.
- One `.ftl` per feature under `Resources/Locale/en-US/_Art/`.
- If you mirror upstream structure, mirror the locale filename too.

## What must be localized

- Popups, chat messages, UI labels, entity names/descriptions, alert text,
  verb text, examiner text — anything a player reads.
- Examining/interaction feedback.

## What is NOT localized

- Internal log strings (`Log.Error("MarketStorageComponent not found...")`) —
  these are for developers, keep them in English in code.
- Identifier keys, prototype IDs, component field names — these are data, not
  display text.
- Comments.

## Self-check

- [ ] No raw English string literal reaches a player-facing surface.
- [ ] Every new key exists in an `_Art` `.ftl` file.
- [ ] Keys are `fork-`-prefixed and kebab-case.
- [ ] XAML uses `{Loc ...}`, not literal `Text="Sell"`.
- [ ] Entity prototypes use locale keys for `name`/`description`.

See `skills/ss14-examples.md` for a full locale + prototype example.
