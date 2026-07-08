using System;
using System.Collections.Generic;
using Content.Shared.Item;
using Content.Shared.Xenoarchaeology.Artifact;
using Content.Shared.Xenoarchaeology.Artifact.XAE;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Xenoarchaeology.Artifact.XAE;

/// <summary>
/// Handles the artifact effect that replaces nearby items with safe random prototypes.
/// Completely cleaned from Sunrise dependencies, custom helpers, and player inventory scanning.
/// </summary>
public sealed class ArtifactRandomTransformationSystem : BaseXAESystem<ArtifactRandomTransformationComponent>
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly HashSet<Entity<ItemComponent>> _items = [];
    private readonly List<EntityUid> _worldItems = [];
    private readonly List<EntityPrototype> _baseCandidatePool = [];
    private readonly Dictionary<EntityUid, List<EntityPrototype>> _candidateCache = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArtifactRandomTransformationComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        RebuildCandidateCaches();
    }

    protected override void OnActivated(Entity<ArtifactRandomTransformationComponent> ent, ref XenoArtifactNodeActivatedEvent args)
    {
        TryActivateTransformation(ent);
    }

    private bool TryActivateTransformation(Entity<ArtifactRandomTransformationComponent> ent)
    {
        if (!TryGetTransformCandidates(ent, out var candidates))
            return false;

        var coords = _transform.GetMoverCoordinates(ent);

        _items.Clear();
        _lookup.GetEntitiesInRange(coords, ent.Comp.Radius, _items);
        CopyNearbyItems();

        // Transforms only items lying in the world, ignores player slots and inventories completely
        TryTransformItems(_worldItems, ent.Comp.TransformationPercentRatio, candidates);
        return true;
    }

    private void OnStartup(Entity<ArtifactRandomTransformationComponent> ent, ref ComponentStartup args)
    {
        TryGetTransformCandidates(ent, out _);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        RebuildCandidateCaches();
    }

    private void RebuildCandidateCaches()
    {
        _candidateCache.Clear();
        _baseCandidatePool.Clear();

        foreach (var prototype in _prototype.EnumeratePrototypes<EntityPrototype>())
        {
            if (!CanEverTransformInto(prototype))
                continue;

            _baseCandidatePool.Add(prototype);
        }
    }

    private static bool CanEverTransformInto(EntityPrototype proto)
    {
        if (proto.Abstract)
            return false;

        if (!proto.MapSavable)
            return false;

        return true;
    }

    private bool TryGetTransformCandidates(Entity<ArtifactRandomTransformationComponent> ent, out List<EntityPrototype> candidates)
    {
        if (_candidateCache.TryGetValue(ent, out var cached))
        {
            candidates = cached;
            return candidates.Count > 0;
        }

        candidates = new List<EntityPrototype>();
        foreach (var proto in _baseCandidatePool)
        {
            if (IsBlacklisted(proto, ent.Comp))
                continue;

            candidates.Add(proto);
        }

        _candidateCache[ent] = candidates;
        return candidates.Count > 0;
    }

    private bool IsBlacklisted(EntityPrototype proto, ArtifactRandomTransformationComponent comp)
    {
        if (comp.PrototypeBlacklistExceptions != null && comp.PrototypeBlacklistExceptions.Contains(proto.ID))
            return false;

        if (comp.PrototypeBlacklist != null && (comp.PrototypeBlacklist.Contains(proto.ID) || HasBlacklistedParent(proto.ID, comp.PrototypeBlacklist)))
            return true;

        if (comp.CategoryBlacklist != null && HasBlacklistedCategory(proto, comp.CategoryBlacklist))
            return true;

        if (comp.ComponentBlacklist != null && HasBlacklistedComponent(proto, comp.ComponentBlacklist))
            return true;

        if (ContainsBlacklistedSubstring(proto.ID, comp.PrototypeIdBlacklistSubstrings))
            return true;

        if (ContainsBlacklistedSubstring(proto.EditorSuffix, comp.PrototypeSuffixBlacklistSubstrings))
            return true;

        if (comp.RequiredComponents != null && !HasRequiredComponents(proto, comp.RequiredComponents))
            return true;

        return false;
    }

    private static bool HasRequiredComponents(EntityPrototype proto, HashSet<string> requiredComponents)
    {
        foreach (var required in requiredComponents)
        {
            if (proto.Components.ContainsKey(required))
                return true;
        }
        return false;
    }

    private bool HasBlacklistedParent(EntProtoId prototypeId, HashSet<EntProtoId> prototypeBlacklist)
    {
        foreach (var parent in _prototype.EnumerateAllParents<EntityPrototype>(prototypeId))
        {
            if (prototypeBlacklist.Contains(parent.id))
                return true;
        }
        return false;
    }

    private static bool HasBlacklistedComponent(EntityPrototype proto, HashSet<string> componentBlacklist)
    {
        foreach (var componentId in proto.Components.Keys)
        {
            if (componentBlacklist.Contains(componentId))
                return true;
        }
        return false;
    }

    private static bool HasBlacklistedCategory(EntityPrototype proto, HashSet<ProtoId<EntityCategoryPrototype>> categoryBlacklist)
    {
        foreach (var category in proto.Categories)
        {
            if (categoryBlacklist.Contains(category.ID))
                return true;
        }
        return false;
    }

    private static bool ContainsBlacklistedSubstring(string? value, IReadOnlyCollection<string>? blacklist)
    {
        if (string.IsNullOrWhiteSpace(value) || blacklist == null)
            return false;

        foreach (var substring in blacklist)
        {
            if (value.Contains(substring, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private void CopyNearbyItems()
    {
        _worldItems.Clear();
        foreach (var item in _items)
        {
            _worldItems.Add(item.Owner);
        }
    }

    private void TryTransformItems(List<EntityUid> entities, float transformationRatio, IReadOnlyList<EntityPrototype> candidates)
    {
        var countToTransform = GetTransformCount(entities.Count, transformationRatio);
        if (countToTransform <= 0)
            return;

        if (entities.Count > 1)
            _random.Shuffle(entities);

        for (var i = 0; i < countToTransform; i++)
        {
            var item = entities[i];
            if (Deleted(item))
                continue;

            var prototype = _random.Pick(candidates);
            Spawn(prototype.ID, _transform.GetMapCoordinates(item));
            QueueDel(item);
        }
    }

    private static int GetTransformCount(int sourceCount, float transformationRatio)
    {
        return Math.Max(1, (int) (sourceCount * transformationRatio));
    }
}
