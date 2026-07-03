# Rule: editing upstream code (narrow diffs + `ss14-art edit` markers)

Upstream code = everything outside `_Art` folders (see `ss14-code-placement.md`).
You may edit it, but every edit must be **narrow** and **marked**. Reviewers and
merge tooling rely on the markers to find our changes under thousands of
upstream lines.

## Marker syntax

Two forms. Use the form that matches the size of the change.

### Multi-line block — `// ss14-art edit start` / `// ss14-art edit end`

When you insert or modify a **block** of 2+ lines in an upstream `.cs` file,
wrap it:

```csharp
        // ss14-art edit start
        var foo = ComputeFoo(uid);
        RaiseLocalEvent(uid, new FooCalculatedEvent(foo));
        // ss14-art edit end
```

- Put the `start` marker on its own line, *before* the changed code, at the
  same indentation as the surrounding code.
- Put the `end` marker on its own line, *after* the changed code.
- If you replace existing upstream lines, the replaced upstream lines go away
  and only your block + markers remain. Do **not** leave the original lines
  commented out.
- One `start`/`end` pair per contiguous block. Don't nest them.

### Single-line change — `// ss14-art edit`

When you change exactly **one line** (a tweak, a renamed arg, a flipped
condition, one added call), append the marker to that line:

```csharp
        if (comp.MaxListings > 0 && comp.AllowForkSales) // ss14-art edit
```

```csharp
        _popup.PopupEntity(Loc.GetString("..."), uid, player); // ss14-art edit
```

Do **not** wrap a single-line change in `start`/`end` — the inline marker is
enough and less noisy.

### In YAML / prototypes

Use the YAML comment form:

```yaml
  - type: ForkMarketTag   # ss14-art edit
```

```yaml
    # ss14-art edit start
    - type: ForkSomething
      foo: 1
    # ss14-art edit end
```

### In XAML

```xml
        <!-- ss14-art edit start -->
        <Button Name="ForkButton" Text="{Loc fork-market-button}" />
        <!-- ss14-art edit end -->
```

## What counts as "one upstream file edit"

The markers exist so a reviewer can `grep "ss14-art edit"` and see *all* of our
divergence from upstream at a glance, and so upstream merges don't silently
conflict-smear our changes. Therefore:

- **Every** line you add or change in an upstream file carries a marker. No
  exceptions, including "trivial" changes. An unmarked edit is a review
  blocker.
- Markers go in **upstream files only**. Do **not** marker `_Art` code — that's
  already ours and the noise hides real divergences.
- Do not marker changes inside `RobustToolbox/` — you shouldn't be editing the
  engine at all (see `ss14-engine-edits.md`).

## Narrow diffs (what to change and how much)

- Change the **smallest** thing that achieves the goal. One line beats five.
- Reuse upstream events, components, and systems where possible instead of
  patching their internals. Subscribe from `_Art` code rather than editing
  upstream systems. See `ss14-code-placement.md`.
- Don't reformat, reorder `using`s, or "tidy" upstream code around your edit.
  Touch only what your feature needs.
- Don't rename upstream identifiers. If you need a different name, add a
  fork-only alias/wrapper in `_Art`.
- Don't delete upstream lines unless removing them is the literal point of the
  change (and even then, prefer disabling via data/component over deleting
  logic).

## Maximally extract fork-only logic out of upstream files

If a feature needs non-trivial logic, the upstream file should contain only the
thinnest possible hook — ideally nothing. Patterns, best first:

1. **No upstream edit at all.** Subscribe to an existing upstream event from an
   `_Art` system. This is the goal. Most features can do this.
2. **Raise an event / add a component field.** If upstream doesn't expose the
   seam you need, the smallest upstream edit is one that *creates* the seam:
   raise a new event, or add a `[DataField]` that fork code reads. Wrap that one
   line/block in markers.
3. **`partial` extension.** If you must extend an upstream class, do it via a
   `partial class` in an `_Art` file rather than stuffing methods into the
   upstream file. The upstream file keeps its body intact.
4. **Last resort: inline logic in the upstream file**, marked. This is the
   worst option because it merges worst. Only when 1–3 are impossible.

The bulk of the feature — the system, the data, the rules — lives in `_Art`.

## Preserve path similarity

When you mirror upstream structure inside `_Art`, keep the path parallel:

| Upstream | Fork |
|---|---|
| `Content.Server/Cargo/Systems/CargoSystem.cs` | `Content.Server/_Art/Cargo/Systems/ForkCargoThing.cs` |
| `Content.Shared/Cargo/Components/FooComponent.cs` | `Content.Shared/_Art/Cargo/Components/ForkFooComponent.cs` |
| `Resources/Prototypes/Entities/Objects/foo.yml` | `Resources/Prototypes/_Art/Entities/Objects/foo.yml` |

Parallel paths make review and upstream merges tractable.

## Boundaries: engine / content / fork

- Engine (`RobustToolbox/`) → no edits. Escalate (see `ss14-engine-edits.md`).
- Upstream content (outside `_Art`) → narrow + marked, as above.
- Fork (`_Art`) → edit normally, no markers, normal SS14 conventions.

## Self-check before you finish

- [ ] Did I edit an upstream file? Every changed line has a `ss14-art edit`
      marker (single-line or `start`/`end` block).
- [ ] Did I put fork-only code in `_Art` instead of the upstream file?
- [ ] Is the upstream diff the smallest it can be? Could it be a subscription
      instead?
- [ ] Did I avoid renaming/reformatting upstream code?
- [ ] No edits inside `RobustToolbox/`?
