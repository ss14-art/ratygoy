using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Prototypes;
using System.Collections.Generic;

namespace Content.Server.Xenoarchaeology.Artifact.XAE;

/// <summary>
/// Configuration for the artifact effect that replaces nearby items with random safe prototypes.
/// Completely cleaned from Sunrise namespaces.
/// </summary>
[RegisterComponent]
public sealed partial class ArtifactRandomTransformationComponent : Component
{
    /// <summary>
    /// Fraction of eligible entities that will be transformed during one activation.
    /// </summary>
    [DataField]
    public float TransformationPercentRatio = 0.2f;

    /// <summary>
    /// Maximum range used when collecting nearby items.
    /// </summary>
    [DataField]
    public float Radius = 12f;

    /// <summary>
    /// Prototype IDs that should never be selected for this effect.
    /// </summary>
    [DataField]
    public HashSet<EntProtoId>? PrototypeBlacklist;

    /// <summary>
    /// Prototype categories that should never be selected for this effect.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<EntityCategoryPrototype>>? CategoryBlacklist;

    /// <summary>
    /// Component IDs that disqualify a prototype from the candidate pool.
    /// </summary>
    [DataField]
    public HashSet<string>? ComponentBlacklist;

    /// <summary>
    /// Explicit allow-list overrides for prototypes that would otherwise match parent blacklist entries.
    /// </summary>
    [DataField]
    public HashSet<EntProtoId>? PrototypeBlacklistExceptions;

    /// <summary>
    /// Component IDs that must exist on a prototype before it can be considered.
    /// </summary>
    [DataField]
    public HashSet<string>? RequiredComponents;

    /// <summary>
    /// Case-insensitive substrings that disqualify a prototype by ID.
    /// </summary>
    [DataField]
    public HashSet<string>? PrototypeIdBlacklistSubstrings;

    /// <summary>
    /// Case-insensitive substrings that disqualify a prototype by set suffix.
    /// </summary>
    [DataField]
    public HashSet<string>? PrototypeSuffixBlacklistSubstrings;
}