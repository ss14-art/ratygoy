using Content.Shared.Construction.Prototypes;
using Content.Shared.Preferences;
using Content.Shared._Art.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Client.Lobby
{
    public interface IClientPreferencesManager
    {
        event Action OnServerDataLoaded;

        bool ServerDataLoaded => Settings != null;

        GameSettings? Settings { get; }
        PlayerPreferences? Preferences { get; }
        void Initialize();
        void SelectCharacter(HumanoidCharacterProfile profile);
        void SelectCharacter(int slot);
        void UpdateCharacter(HumanoidCharacterProfile profile, int slot);
        void CreateCharacter(HumanoidCharacterProfile profile);
        void DeleteCharacter(HumanoidCharacterProfile profile);
        void DeleteCharacter(int slot);
        void DeleteCharacter(string name);
        void UpdateConstructionFavorites(List<ProtoId<ConstructionPrototype>> favorites);
        void FinalizeCharacter(HumanoidCharacterProfile profile, int slot);
        void JoinAsCharacter(int slot);
        // SS14-Art-Edit start
        /// <summary>
        /// Raised when the server responds to <see cref="RequestPersistentAppearance"/>.
        /// </summary>
        event Action<MsgPersistentAppearance> PersistentAppearanceReceived;

        /// <summary>
        /// Asks the server what the persistent (saved) body for the given character slot
        /// is currently wearing, e.g. so the character editor can preview it.
        /// </summary>
        void RequestPersistentAppearance(int slot);
        // SS14-Art-Edit end
    }
}
