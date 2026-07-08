using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._Art.Preferences
{
    /// <summary>
    /// The client sends this to ask the server what the persistent (saved) body for a
    /// given character slot is currently wearing, so the character editor preview doll
    /// can show the real outfit instead of the default job clothes.
    /// </summary>
    public sealed class MsgRequestPersistentAppearance : NetMessage
    {
        public override MsgGroups MsgGroup => MsgGroups.Command;

        public int Slot;

        public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
        {
            Slot = buffer.ReadInt32();
        }

        public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
        {
            buffer.Write(Slot);
        }
    }
}