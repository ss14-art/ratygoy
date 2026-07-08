using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._Art.Preferences
{
    /// <summary>
    /// The server sends this in response to <see cref="MsgRequestPersistentAppearance"/>.
    /// Contains a snapshot of what the player's persistent (saved) body is currently
    /// equipped with, keyed by inventory slot name, so the client can render it on the
    /// character editor preview doll.
    /// </summary>
    public sealed class MsgPersistentAppearance : NetMessage
    {
        public override MsgGroups MsgGroup => MsgGroups.Command;

        public int Slot;

        /// <summary>
        /// Whether a persistent (saved) body was found for this character slot at all.
        /// If false, the character has never entered cryosleep / has no saved body yet,
        /// and the client should fall back to the default job-clothes preview.
        /// </summary>
        public bool Found;

        /// <summary>
        /// Inventory slot name (e.g. "jumpsuit", "head") -> the EntProtoId of the item
        /// currently equipped there on the persistent body.
        /// </summary>
        public Dictionary<string, string> EquippedItems = new();

        public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
        {
            Slot = buffer.ReadInt32();
            Found = buffer.ReadBoolean();

            var count = buffer.ReadVariableInt32();
            EquippedItems = new Dictionary<string, string>(count);
            for (var i = 0; i < count; i++)
            {
                var key = buffer.ReadString();
                var value = buffer.ReadString();
                EquippedItems[key] = value;
            }
        }

        public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
        {
            buffer.Write(Slot);
            buffer.Write(Found);

            buffer.WriteVariableInt32(EquippedItems.Count);
            foreach (var (key, value) in EquippedItems)
            {
                buffer.Write(key);
                buffer.Write(value);
            }
        }
    }
}