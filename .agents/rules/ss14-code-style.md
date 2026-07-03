# Rule: code style for fork C#

Follow upstream SS14 style. The `.editorconfig` enforces the mechanical parts
(4-space indent, 120 col, UTF-8, trim trailing whitespace, final newline). This
rule covers the conventions `.editorconfig` can't.

## Naming

### Subscription handlers

`On<Event>` — prefix `On`, then the event name (minus the `Event` suffix is
fine when unambiguous). Group by UI/event.

```csharp
SubscribeLocalEvent<FooComponent, BoundUIOpenedEvent>(OnFooUIOpened);
private void OnFooUIOpened(EntityUid uid, FooComponent comp, BoundUIOpenedEvent args) { }
```

For BUI event wiring, the fork uses the `Subs.BuiEvents<T>` helper (see
`MarketSystem`):

```csharp
Subs.BuiEvents<MarketSaleConsoleComponent>(MarketConsoleUiKey.Sale, subs =>
{
    subs.Event<BoundUIOpenedEvent>(OnSaleUIOpened);
    subs.Event<MarketCreateListingMessage>(OnCreateListing);
});
```

### Dependency fields

`[Dependency] private readonly SomeSystem _some = default!;` — the field name
is the system name minus `System`, camelCase with leading underscore.

```csharp
[Dependency] private readonly BankSystem _bank = default!;
[Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
[Dependency] private readonly SharedContainerSystem _container = default!;
```

- `_bank` for `BankSystem`.
- `_uiSystem` for `UserInterfaceSystem` (keeps `System` when dropping it would
  be ambiguous, e.g. `UserInterface` is also a type).
- `_container` for `SharedContainerSystem` (the `Shared` prefix can be dropped
  in the field name when there's no ambiguity; keep it if a server-only
  `ContainerSystem` exists alongside).
- Always `private readonly`, always `= default!`.

## `Entity<T>` (typed entity tuple)

Prefer `Entity<TComponent>` over raw `(EntityUid, TComponent)` where the API
takes/returns it. It's the canonical way to carry an entity + one component.

```csharp
private readonly HashSet<Entity<MarketContainerComponent>> _containerSet = new();

private void OnMapInit(Entity<StationBankAccountComponent> ent, ref MapInitEvent args) { }
```

- Use it in handler signatures when the engine offers the `Entity<T>` overload.
- Use it for collections of entity+component pairs.
- Access `.Owner`/`.Comp`/`.Comp1` as the tuple provides; prefer the named
  properties the engine defines.

## Method ordering: `On → Try → Can → Do`

Structure a system's methods so the call chain reads top-down:

1. **`On<Event>`** — the event handler. Validates, then delegates. No deep logic.
2. **`Try<Action>`** — orchestrates the action: checks preconditions, gathers
   data, calls `Can` then `Do`. Returns bool or early-returns.
3. **`Can<Action>`** — pure precondition checks. Returns bool, no side effects.
4. **`Do<Action>`** — the actual mutation/effect. Assumes preconditions hold.

```csharp
private void OnCreateListing(EntityUid uid, MarketSaleConsoleComponent comp, MarketCreateListingMessage args)
{
    if (!TryCreateListing(uid, comp, args))
        return;
}

private bool TryCreateListing(EntityUid uid, MarketSaleConsoleComponent comp, MarketCreateListingMessage args)
{
    if (!CanCreateListing(uid, comp, args))
        return false;
    DoCreateListing(uid, comp, args);
    return true;
}

private bool CanCreateListing(EntityUid uid, MarketSaleConsoleComponent comp, MarketCreateListingMessage args)
{
    if (string.IsNullOrWhiteSpace(args.LotName) || args.LotName.Length > 25) return false;
    if (_market.GetListingsBySeller(Name(args.Actor)).Count >= comp.MaxListingsPerPlayer) return false;
    return true;
}

private void DoCreateListing(EntityUid uid, MarketSaleConsoleComponent comp, MarketCreateListingMessage args)
{
    // ...mutate state, Dirty, popup...
}
```

- `On` is thin. `Can` is pure. `Do` is the only place with side effects.
- This makes preconditions testable and the diff reviewable.
- Not every trivial handler needs the full pyramid — use judgment. A one-line
  handler is fine as just `On`. Reach for `Try/Can/Do` when there's real
  precondition logic.

## `[DataField]`, `ProtoId<T>`, `[Prototype]`

- `[DataField]` on serialized component/prototype fields. Field initializers
  are the defaults.
- Prefer `ProtoId<TPrototype>` over `string` for prototype references — it's
  typed and validated.

  ```csharp
  [DataField]
  public ProtoId<StackPrototype> StackTypeId = default!;
  ```

  Not `public string StackType = "";`.
- For entity prototype references, `EntProtoId<TComponent>` is the typed form.
- `[Prototype]` on prototype classes that have a backing C# type (rare in fork
  data; most fork prototypes are pure YAML `type: entity`/`type: reagent`).
- Don't use the legacy `string`-id + manual lookup style. `PrototypeManager`
  indexing via `ProtoId<T>` is the modern path (see
  `skills/ss14-common-api-patterns.md`).
- `[DataField]` defaults: if a value must always be present, keep the
  initializer non-null/non-empty (`= string.Empty;`, `= new();`).

## General C# / SS14 conventions

- `sealed partial` on components, `sealed` (or `sealed partial` if split) on
  systems. `sealed` is the default; open only with reason.
- `using` directives: modern SS14 uses file-scoped-ish grouping; match the
  surrounding file. Don't reorder usings in upstream files (see
  `ss14-fork-editing.md`).
- `var` for locals where the type is obvious; explicit type where it aids
  reading.
- No `this.` (`.editorconfig` already discourages it; `dotnet_style_qualification_*`
  = false).
- Brace style: Allman (braces on own line), 4-space indent.
- `default!` for `[Dependency]` fields; never null-deref a dependency.
- Prefer `RaiseLocalEvent` / `SubscribeLocalEvent` over direct cross-system
  calls when the seam should be decoupled; direct calls are fine for tight,
  same-assembly helpers.
- Don't catch `Exception` broadly. Catch specific exceptions or let it crash.

## Namespaces

- Fork: `Content.<Assembly>._Art.<Feature>[.<Sub>]`.
  - `Content.Shared._Art.Market.Components`
  - `Content.Server._Art.Market`
  - `Content.Client._Art.Market.UI`
- Folder path matches namespace, as upstream does.
- One namespace per feature; sub-folders get a sub-namespace.

## What not to do

- Don't introduce a `public` field where `[DataField] private` would do; expose
  via the system, not the component's public surface.
- Don't drop `System` from a dependency field name when it creates ambiguity.
- Don't put logic in `Can` or `On`; they stay thin/pure.
- Don't use raw `string` ids where `ProtoId<T>` exists.
