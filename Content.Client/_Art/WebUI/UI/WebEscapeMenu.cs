using System.Numerics;
using System.Text;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.WebView;
using Robust.Shared.Asynchronous;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Utility;

namespace Content.Client._Art.WebUI.UI;

public sealed class WebEscapeMenu : DefaultWindow
{
    private const string PageUrl = "http://webui.local/dist/src/interfaces/escape-menu/index.html";

    [Dependency] private readonly ITaskManager _taskManager = default!;
    [Dependency] private readonly IResourceManager _resourceManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IWebViewManager _webViewManager = default!;

    private readonly NovaWebViewBridge _bridge;
    private readonly ISawmill _sawmill;
    private bool _showWiki;
    private bool _showDiscord;

    public event Action? FeedbackPressed;
    public event Action? ChangelogPressed;
    public event Action? RulesPressed;
    public event Action? DisconnectPressed;
    public event Action? OptionsPressed;
    public event Action? QuitPressed;
    public event Action? WikiPressed;
    public event Action? GuidebookPressed;
    public event Action? DiscordPressed;

    protected override Vector2 ContentsMinimumSize => new(480, 560);

    public WebEscapeMenu()
    {
        IoCManager.InjectDependencies(this);

        _sawmill = _logManager.GetSawmill("webui.escape");

        Title = Loc.GetString("fork-web-ui-escape-title");

        if (!_resourceManager.TryContentFileRead(new ResPath("/dist/src/interfaces/escape-menu/index.html"), out _))
        {
            _sawmill.Error("Escape menu HTML is missing: build the WebUI with 'npm run build' in the WebUI/ directory.");
        }

        var webView = new WebViewControl
        {
            Url = PageUrl,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        _sawmill.Debug("WebEscapeMenu WebView created for {0}", PageUrl);

        Contents.AddChild(webView);
        _bridge = new NovaWebViewBridge(webView, _taskManager, _resourceManager, _webViewManager, PageUrl);
        _bridge.RequestReceived += OnBridgeRequest;
    }


    public void SetLinkVisibility(bool showWiki, bool showDiscord)
    {
        _showWiki = showWiki;
        _showDiscord = showDiscord;
    }

    private void OnBridgeRequest(NovaBridgeRequest request)
    {
        _sawmill.Debug("Bridge event received: {0}", request.EventType);

        switch (request.EventType)
        {
            case "page-ready":
                _bridge.Complete(request.RequestId, "{\"ready\":true}");
                break;
            case "escape-menu-get-state":
                _bridge.Complete(request.RequestId, BuildStateJson());
                break;
            case "escape-menu-action":
                HandleAction(request);
                break;
            case "chunk-data":
                _bridge.Error(request.RequestId, "Chunked bridge requests are not supported.");
                break;
            default:
                _bridge.Error(request.RequestId, "Unknown bridge request.");
                break;
        }
    }

    private string BuildStateJson()
    {
        var sb = new StringBuilder();
        sb.Append("{\"title\":\"");
        sb.Append(JsonEscape(Loc.GetString("fork-web-ui-escape-title")));
        sb.Append("\",\"status\":\"");
        sb.Append(JsonEscape(Loc.GetString("fork-web-ui-escape-connected")));
        sb.Append("\",\"actions\":[");

        AppendAction(sb, "rules", "fork-web-ui-escape-rules");
        sb.Append(',');
        AppendAction(sb, "guidebook", "fork-web-ui-escape-guidebook");
        sb.Append(',');
        AppendAction(sb, "changelog", "fork-web-ui-escape-changelog");
        sb.Append(',');
        AppendAction(sb, "feedback", "fork-web-ui-escape-feedback");
        sb.Append(',');
        AppendAction(sb, "options", "fork-web-ui-escape-options");
        sb.Append(',');
        AppendAction(sb, "wiki", "fork-web-ui-escape-wiki", _showWiki);
        sb.Append(',');
        AppendAction(sb, "discord", "fork-web-ui-escape-discord", _showDiscord);
        sb.Append(',');
        AppendAction(sb, "disconnect", "fork-web-ui-escape-disconnect", true, true);
        sb.Append(',');
        AppendAction(sb, "quit", "fork-web-ui-escape-quit", true, true);

        sb.Append("]}");
        return sb.ToString();
    }

    private static void AppendAction(StringBuilder sb, string id, string localeKey, bool visible = true, bool danger = false)
    {
        sb.Append("{\"id\":\"");
        sb.Append(id);
        sb.Append("\",\"label\":\"");
        sb.Append(JsonEscape(Loc.GetString(localeKey)));
        sb.Append("\",\"visible\":");
        sb.Append(visible ? "true" : "false");
        sb.Append(",\"danger\":");
        sb.Append(danger ? "true" : "false");
        sb.Append("}");
    }

    private static string JsonEscape(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private void HandleAction(NovaBridgeRequest request)
    {
        var action = ExtractActionId(request.DataJson);
        if (string.IsNullOrEmpty(action))
        {
            _bridge.Error(request.RequestId, Loc.GetString("fork-web-ui-escape-invalid-action"));
            return;
        }

        var callback = action switch
        {
            "feedback" => FeedbackPressed,
            "changelog" => ChangelogPressed,
            "rules" => RulesPressed,
            "disconnect" => DisconnectPressed,
            "options" => OptionsPressed,
            "quit" => QuitPressed,
            "wiki" when _showWiki => WikiPressed,
            "guidebook" => GuidebookPressed,
            "discord" when _showDiscord => DiscordPressed,
            _ => null,
        };

        if (callback == null)
        {
            _bridge.Error(request.RequestId, Loc.GetString("fork-web-ui-escape-invalid-action"));
            return;
        }

        _bridge.Complete(request.RequestId, "{\"accepted\":true}");
        callback.Invoke();
    }

    private static string? ExtractActionId(string json)
    {
        var key = "\"action\"";
        var idx = json.IndexOf(key, StringComparison.Ordinal);
        if (idx < 0)
            return null;

        idx += key.Length;
        while (idx < json.Length && (json[idx] == ':' || char.IsWhiteSpace(json[idx])))
            idx++;

        if (idx >= json.Length || json[idx] != '"')
            return null;

        idx++;
        var end = json.IndexOf('"', idx);
        if (end < 0)
            return null;

        return json.Substring(idx, end - idx);
    }

    protected override void Dispose(bool disposing)
    {
        _bridge.Dispose();
        base.Dispose(disposing);
    }
}
