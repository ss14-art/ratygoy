namespace Content.Shared._Persistence14.PersistentIdentifier;

/// <summary>
/// An event signifying a change to a the ID stored within a <see cref="PersistentIdentifierComponent"/> 
/// </summary>
[ByRefEvent]
public record struct PersistentIdChangedEvent(EntityUid Uid, string OldId, string NewId, PersistentIdChangeBehaviour Behaviour);

public enum PersistentIdChangeBehaviour
{
    Sever, // Delete any references to the ID.
    Alter, // Change any references to match the new ID.
}