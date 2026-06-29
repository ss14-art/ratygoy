namespace Content.Shared._Persistence14.PersistentIdentifier;

public sealed partial class PersistentIdentifierSystem : EntitySystem
{
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private ILogManager _log = default!;

    /// <summary>
    /// The Sawmill key for all ID related log messages.
    /// </summary>
    public const string Sawmill = "Persistent ID";

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PersistentIdRegisterComponent, PersistentIdChangedEvent>(OnRegisterIdChange);
    }

    /// <summary>
    /// Gets the ID from a <see cref="PersistentIdentifierComponent"/>. Generates a new ID if one is not present.
    /// </summary>
    public string EnsureId(Entity<PersistentIdentifierComponent> ent)
    {
        if (ent.Comp.IdInit) return ent.Comp.Id;

        ResetId(ent, out var id, PersistentIdChangeBehaviour.Sever);
        return id;
    }

    public string EnsureId(EntityUid uid)
    {
        EnsureComp<PersistentIdentifierComponent>(uid, out var idComp);
        return EnsureId((uid, idComp));
    }

    /// <summary>
    /// Attempts to retrieve the ID from a <see cref="PersistentIdentifierComponent"/>.<br/><br/>
    /// Return true if the component has an ID, otherwise false.
    /// </summary>
    public bool TryGetId(Entity<PersistentIdentifierComponent> ent, out string id)
    {
        if (ent.Comp.IdInit)
        {
            id = ent.Comp.Id;
            return true;
        }

        id = Guid.Empty.ToString();
        return false;
    }

    /// <summary>
    /// Resets the ID of a <see cref="PersistentIdentifierComponent"/> to a new valid GUID.
    /// </summary>
    public void ResetId(Entity<PersistentIdentifierComponent> ent, out string id, PersistentIdChangeBehaviour behaviour = PersistentIdChangeBehaviour.Sever)
    {
        var oldId = ent.Comp.Id;
        ent.Comp.Id = Guid.NewGuid().ToString();
        id = ent.Comp.Id;
        Dirty(ent);

        var ev = new PersistentIdChangedEvent(ent, oldId, ent.Comp.Id, behaviour);
        RaiseLocalEvent(ref ev);
    }

    public void ResetId(Entity<PersistentIdentifierComponent> ent, PersistentIdChangeBehaviour behaviour = PersistentIdChangeBehaviour.Sever)
        => ResetId(ent, out _, behaviour);

    /// <summary>
    /// Empties the ID of a <see cref="PersistentIdentifierComponent"/>.
    /// </summary>
    public void ClearId(Entity<PersistentIdentifierComponent> ent, PersistentIdChangeBehaviour behaviour = PersistentIdChangeBehaviour.Sever)
    {
        if (!ent.Comp.IdInit) return;

        var oldId = ent.Comp.Id;
        ent.Comp.Id = Guid.Empty.ToString();
        Dirty(ent);

        var ev = new PersistentIdChangedEvent(ent, oldId, ent.Comp.Id, behaviour);
        RaiseLocalEvent(ref ev);
    }

    public bool OverrideId(Entity<PersistentIdentifierComponent> ent, string id, PersistentIdChangeBehaviour behaviour = PersistentIdChangeBehaviour.Sever)
    {
        if (id == ent.Comp.Id) return false;

        if (id == Guid.Empty.ToString())
        {
            _log.GetSawmill(Sawmill).Warning("Unable to override ID to empty. Use ClearId instead.");
            return false;
        }

        var oldId = ent.Comp.Id;
        ent.Comp.Id = id;
        Dirty(ent);

        var ev = new PersistentIdChangedEvent(ent, oldId, ent.Comp.Id, behaviour);
        RaiseLocalEvent(ref ev);
        return true;
    }

    /// <summary>
    /// Attempts to resolve a given id on a source entity. Will prioritize an existing <see cref="PersistentIdRegisterComponent"/> and may add one if none are available.
    /// </summary>
    /// <param name="sourceUid">The Uid of the entity to look into.</param>
    /// <param name="id">The desired id.</param>
    /// <param name="ent">The output variable storing the retrieved entity.</param>
    /// <param name="conditional">A conditional function applied to the search.</param>
    /// <param name="useFetchIfFalse">If true, the resolve method will attempt to fetch the entity using <see cref="TryFetchId"/> if unable to resolve using the source registry.</param>
    /// <param name="ensureRegistry">If true, the resolve method will ensure the existence of a <see cref="PersistentIdRegisterComponent"/> on the source Uid.</param> 
    /// <returns>True if able to successfully resolve the id, otherwise false.</returns>
    public bool TryResolveId(
        EntityUid sourceUid,
        string id,
        out Entity<PersistentIdentifierComponent> ent,
        Func<Entity<PersistentIdentifierComponent>, bool>? conditional = null,
        bool useFetchIfFalse = true, bool ensureRegistry = true)
    {
        ent = default!;
        conditional ??= _ => true;

        PersistentIdRegisterComponent? registry;
        if (ensureRegistry)
        {
            EnsureComp<PersistentIdRegisterComponent>(sourceUid, out registry);
            if (registry.TryGet(id, out ent, _entMan) && conditional(ent))
                return true;
        }
        else if (
            TryComp<PersistentIdRegisterComponent>(sourceUid, out registry) &&
            registry.TryGet(id, out ent, _entMan) &&
            conditional(ent))
        {
            return true;
        }

        if (useFetchIfFalse)
            return TryFetchId(id, out ent, conditional, registry);
        return false;
    }

    /// <summary>
    /// Fetches an id from all existing <see cref="PersistentIdentifierComponent"/>. Attempts to add valid ids to an existing <see cref="PersistentIdRegisterComponent"/> 
    /// </summary>
    /// <param name="id">The desired id.</param>
    /// <param name="ent">The output variable storing the retrieved entity.</param>
    /// <param name="conditional">A conditional function applied to the search.</param>
    /// <param name="registry">An optional registry to register valid ids to when found. Improves speed of future searches.</param>
    /// <returns></returns>
    public bool TryFetchId(
        string id,
        out Entity<PersistentIdentifierComponent> ent,
        Func<Entity<PersistentIdentifierComponent>, bool>? conditional = null,
        PersistentIdRegisterComponent? registry = null)
    {
        ent = default!;
        conditional ??= _ => true;

        _log.GetSawmill(Sawmill).Info($"Attempting to fetch persistent id: {id}");

        var lookup = EntityQueryEnumerator<PersistentIdentifierComponent>();

        while (lookup.MoveNext(out var uid, out var idComp))
        {
            if (idComp.Id == id && conditional((uid, idComp)))
            {
                ent = (uid, idComp);
                if (registry is not null) registry.TryRegister(ent, _entMan);
                _log.GetSawmill(Sawmill).Info($"Entity found: {uid}");
                return true;
            }
        }

        _log.GetSawmill(Sawmill).Warning($"Unable to find entity with matching pid.");
        return false;
    }

    private void OnRegisterIdChange(EntityUid uid, PersistentIdRegisterComponent register, ref PersistentIdChangedEvent args)
    {
        // Culling will remove the existing reference as the new ID will not match that stored in the key.
        register.CullStaleEntities(_entMan);

        if (args.NewId == Guid.Empty.ToString() || args.NewId == args.OldId || args.Behaviour == PersistentIdChangeBehaviour.Sever)
            return; // Nothing more to do.

        if (!TryComp<PersistentIdentifierComponent>(args.Uid, out var idComp))
            return; // What would this even mean...?

        register.TryRegister((args.Uid, idComp), _entMan);
    }
}