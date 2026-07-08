using Content.Shared.Construction.Prototypes;
using Content.Shared.Preferences;
using Content.Shared._Art.Preferences;
using Robust.Client;
using Robust.Client.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Client.Lobby
{
    /// <summary>
    ///     Receives <see cref="PlayerPreferences" /> and <see cref="GameSettings" /> from the server during the initial
    ///     connection.
    ///     Stores preferences on the server through <see cref="SelectCharacter" /> and <see cref="UpdateCharacter" />.
    /// </summary>
    public sealed class ClientPreferencesManager : IClientPreferencesManager
    {
        [Dependency] private readonly IClientNetManager _netManager = default!;
        [Dependency] private readonly IBaseClient _baseClient = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;

        public event Action? OnServerDataLoaded;

        // SS14-Art-Edit start
        public event Action<MsgPersistentAppearance>? PersistentAppearanceReceived;
        // SS14-Art-Edit end

        public GameSettings Settings { get; private set; } = default!;
        public PlayerPreferences Preferences { get; private set; } = default!;

        public void Initialize()
        {
            _netManager.RegisterNetMessage<MsgPreferencesAndSettings>(HandlePreferencesAndSettings);
            _netManager.RegisterNetMessage<MsgUpdateCharacter>();
            _netManager.RegisterNetMessage<MsgSelectCharacter>();
            _netManager.RegisterNetMessage<MsgDeleteCharacter>();
            _netManager.RegisterNetMessage<MsgFinalizeCharacter>();
            // SS14-Art-Edit start
            _netManager.RegisterNetMessage<MsgRequestPersistentAppearance>();
            _netManager.RegisterNetMessage<MsgPersistentAppearance>(HandlePersistentAppearance);
            // SS14-Art-Edit end
            _baseClient.RunLevelChanged += BaseClientOnRunLevelChanged;
        }

        private void BaseClientOnRunLevelChanged(object? sender, RunLevelChangedEventArgs e)
        {
            if (e.NewLevel == ClientRunLevel.Initialize)
            {
                Settings = default!;
                Preferences = default!;
            }
        }

        public void SelectCharacter(HumanoidCharacterProfile profile)
        {
            SelectCharacter(Preferences.IndexOfCharacter(profile));
        }

        public void SelectCharacter(int slot)
        {
            Preferences = new PlayerPreferences(Preferences.Characters, slot, Preferences.AdminOOCColor, Preferences.ConstructionFavorites);
            var msg = new MsgSelectCharacter
            {
                SelectedCharacterIndex = slot
            };
            _netManager.ClientSendMessage(msg);
        }

        public void UpdateCharacter(HumanoidCharacterProfile profile, int slot)
        {
            var collection = IoCManager.Instance!;
            profile.EnsureValid(_playerManager.LocalSession!, collection);
            var characters = new Dictionary<int, HumanoidCharacterProfile>(Preferences.Characters) { [slot] = profile };
            Preferences = new PlayerPreferences(characters, Preferences.SelectedCharacterIndex, Preferences.AdminOOCColor, Preferences.ConstructionFavorites);
            var msg = new MsgUpdateCharacter
            {
                Profile = profile,
                Slot = slot
            };
            _netManager.ClientSendMessage(msg);
        }

        public void CreateCharacter(HumanoidCharacterProfile profile)
        {
            var characters = new Dictionary<int, HumanoidCharacterProfile>(Preferences.Characters);
            var lowest = Enumerable.Range(0, Settings.MaxCharacterSlots)
                .Except(characters.Keys)
                .FirstOrNull();

            if (lowest == null)
            {
                throw new InvalidOperationException("Out of character slots!");
            }

            var l = lowest.Value;
            characters.Add(l, profile);
            Preferences = new PlayerPreferences(characters, Preferences.SelectedCharacterIndex, Preferences.AdminOOCColor, Preferences.ConstructionFavorites);

            UpdateCharacter(profile, l);
        }

        public void DeleteCharacter(HumanoidCharacterProfile profile)
        {
            DeleteCharacter(Preferences.IndexOfCharacter(profile));
        }

        public void DeleteCharacter(int slot)
        {
            var characters = Preferences.Characters.Where(p => p.Key != slot);
            Preferences = new PlayerPreferences(characters, Preferences.SelectedCharacterIndex, Preferences.AdminOOCColor, Preferences.ConstructionFavorites);
            var msg = new MsgDeleteCharacter
            {
                Slot = slot
            };
            _netManager.ClientSendMessage(msg);
        }

        public void DeleteCharacter(string name)
        {

        }
        public void UpdateConstructionFavorites(List<ProtoId<ConstructionPrototype>> favorites)
        {
            Preferences = new PlayerPreferences(Preferences.Characters, Preferences.SelectedCharacterIndex, Preferences.AdminOOCColor, favorites);
            var msg = new MsgUpdateConstructionFavorites
            {
                Favorites = favorites
            };
            _netManager.ClientSendMessage(msg);
        }

        private void HandlePreferencesAndSettings(MsgPreferencesAndSettings message)
        {
            Preferences = message.Preferences;
            Settings = message.Settings;

            OnServerDataLoaded?.Invoke();
        }
        public void FinalizeCharacter(HumanoidCharacterProfile profile, int slot)
        {
            var msg = new MsgFinalizeCharacter
            {
                Slot = slot,
                Profile = profile
            };
            _netManager.ClientSendMessage(msg);
        }
        public void JoinAsCharacter(int slot)
        {
            var msg = new MsgJoinAsCharacter
            {
                Slot = slot,
            };
            _netManager.ClientSendMessage(msg);
        }

        // SS14-Art-Edit start
        public void RequestPersistentAppearance(int slot)
        {
            _netManager.ClientSendMessage(new MsgRequestPersistentAppearance { Slot = slot });
        }

        private void HandlePersistentAppearance(MsgPersistentAppearance message)
        {
            PersistentAppearanceReceived?.Invoke(message);
        }
        // SS14-Art-Edit end
    }
}
