using Robust.Client.UserInterface;
using Robust.Shared.Console;
using Robust.Shared.IoC;

namespace Content.Client._Art.WebUI;

internal sealed class TestWebUICommand : LocalizedCommands
{
    public override string Command => "test_webui_escape";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var uiManager = IoCManager.Resolve<IUserInterfaceManager>();
        var window = uiManager.CreateWindow<UI.WebEscapeMenu>();
        window.OpenCentered();
    }
}
