using Content.Client._Art.WebUI.UI; // ss14-art edit
using Content.Client.FeedbackPopup;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Guidebook;
using Content.Client.UserInterface.Systems.Info;
using Content.Shared.CCVar;
using JetBrains.Annotations;
using Robust.Client.Console;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.UserInterface.Systems.EscapeMenu;

[UsedImplicitly]
public sealed class EscapeUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IClientConsoleHost _console = default!;
    [Dependency] private readonly IUriOpener _uri = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ChangelogUIController _changelog = default!;
    [Dependency] private readonly InfoUIController _info = default!;
    [Dependency] private readonly OptionsUIController _options = default!;
    [Dependency] private readonly GuidebookUIController _guidebook = default!;
    [Dependency] private readonly FeedbackPopupUIController _feedback = null!;

    private WebEscapeMenu? _escapeWindow; // ss14-art edit

    private MenuButton? EscapeButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.EscapeButton;

    public void UnloadButton()
    {
        if (EscapeButton == null)
        {
            return;
        }

        EscapeButton.Pressed = false;
        EscapeButton.OnPressed -= EscapeButtonOnOnPressed;
    }

    public void LoadButton()
    {
        if (EscapeButton == null)
        {
            return;
        }

        EscapeButton.OnPressed += EscapeButtonOnOnPressed;
    }

    private void ActivateButton() => EscapeButton!.SetClickPressed(true);
    private void DeactivateButton() => EscapeButton!.SetClickPressed(false);

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_escapeWindow == null);

        // ss14-art edit start
        _escapeWindow = UIManager.CreateWindow<WebEscapeMenu>();

        _escapeWindow.OnClose += DeactivateButton;
        _escapeWindow.OnOpen += ActivateButton;

        _escapeWindow.FeedbackPressed += () =>
        {
            CloseEscapeWindow();
            _feedback.ToggleWindow();
        };

        _escapeWindow.ChangelogPressed += () =>
        {
            CloseEscapeWindow();
            _changelog.ToggleWindow();
        };

        _escapeWindow.RulesPressed += () =>
        {
            CloseEscapeWindow();
            _info.OpenWindow();
        };

        _escapeWindow.DisconnectPressed += () =>
        {
            CloseEscapeWindow();
            _console.ExecuteCommand("disconnect");
        };

        _escapeWindow.OptionsPressed += () =>
        {
            CloseEscapeWindow();
            _options.OpenWindow();
        };

        _escapeWindow.QuitPressed += () =>
        {
            CloseEscapeWindow();
            _console.ExecuteCommand("quit");
        };

        _escapeWindow.WikiPressed += () =>
        {
            _uri.OpenUri(_cfg.GetCVar(CCVars.InfoLinksWiki));
        };

        _escapeWindow.GuidebookPressed += () =>
        {
            _guidebook.ToggleGuidebook();
        };

        _escapeWindow.DiscordPressed += () =>
        {
            _uri.OpenUri(_cfg.GetCVar(CCVars.InfoLinksDiscord));
        };

        _escapeWindow.SetLinkVisibility(
            _cfg.GetCVar(CCVars.InfoLinksWiki) != "",
            _cfg.GetCVar(CCVars.InfoLinksDiscord) != "");
        // ss14-art edit end

        CommandBinds.Builder
            .Bind(EngineKeyFunctions.EscapeMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<EscapeUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        if (_escapeWindow != null)
        {
            _escapeWindow.Dispose();
            _escapeWindow = null;
        }

        CommandBinds.Unregister<EscapeUIController>();
    }

    private void EscapeButtonOnOnPressed(ButtonEventArgs obj)
    {
        ToggleWindow();
    }

    private void CloseEscapeWindow()
    {
        _escapeWindow?.Close();
    }

    /// <summary>
    /// Toggles the game menu.
    /// </summary>
    public void ToggleWindow()
    {
        if (_escapeWindow == null)
            return;

        if (_escapeWindow.IsOpen)
        {
            CloseEscapeWindow();
            EscapeButton!.Pressed = false;
        }
        else
        {
            _escapeWindow.OpenCentered();
            EscapeButton!.Pressed = true;
        }
    }
}
