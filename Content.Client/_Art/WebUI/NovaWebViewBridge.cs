using System.IO;
using System.Net;
using System.Text;
using Robust.Client.WebView;
using Robust.Shared.Asynchronous;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Utility;

namespace Content.Client._Art.WebUI;

public sealed class NovaWebViewBridge : IDisposable
{
    private const string WebUiOrigin = "http://webui.local/";
    private const string BridgePrefix = "http://webui.local/__bridge__/";

    private readonly WebViewControl _webView;
    private readonly ITaskManager _taskManager;
    private readonly IResourceManager _resourceManager;
    private readonly IWebViewManager _webViewManager;
    private readonly string _trustedPage;
    private bool _disposed;

    public event Action<NovaBridgeRequest>? RequestReceived;

    public NovaWebViewBridge(
        WebViewControl webView,
        ITaskManager taskManager,
        IResourceManager resourceManager,
        IWebViewManager webViewManager,
        string trustedPage)
    {
        _webView = webView;
        _taskManager = taskManager;
        _resourceManager = resourceManager;
        _webViewManager = webViewManager;
        _trustedPage = trustedPage;
        _webView.AddBeforeBrowseHandler(OnBeforeBrowse);
        _webView.AddResourceRequestHandler(OnResourceRequest);
    }

    public void Complete(string requestId, string json)
    {
        DispatchResponse("nova:complete", requestId, EncodeData(json));
    }

    public void Error(string requestId, string message)
    {
        var json = "\"" + JsonEscape(message) + "\"";
        DispatchResponse("nova:error", requestId, EncodeData(json));
    }

    private void OnResourceRequest(IRequestHandlerContext context)
    {
        var url = context.Url;
        if (!url.StartsWith(WebUiOrigin, StringComparison.Ordinal))
            return;

        // Bridge navigations are handled by OnBeforeBrowse.
        if (url.StartsWith(BridgePrefix, StringComparison.Ordinal))
            return;

        var path = url.Substring(WebUiOrigin.Length);
        if (string.IsNullOrEmpty(path))
            path = "index.html";

        var resPath = new ResPath("/" + path);
        if (!_resourceManager.TryContentFileRead(resPath, out var stream))
        {
            var notFoundStream = new MemoryStream(Encoding.UTF8.GetBytes("Not found"));
            context.DoRespondStream(notFoundStream, "text/plain", HttpStatusCode.NotFound);
            return;
        }

        var mime = GetMimeType(resPath.Extension);
        context.DoRespondStream(stream, mime);
    }

    private void OnBeforeBrowse(IBeforeBrowseContext context)
    {
        var url = context.Url;
        if (!url.StartsWith(BridgePrefix, StringComparison.Ordinal))
            return;

        context.DoCancel();

        if (!IsTrustedPage())
            return;

        try
        {
            var prefixLength = BridgePrefix.Length;
            var queryStart = url.IndexOf('?');
            var eventType = queryStart < 0
                ? url.Substring(prefixLength)
                : url.Substring(prefixLength, queryStart - prefixLength);

            var requestId = "";
            var encodedData = "";

            if (queryStart >= 0)
            {
                var query = url.Substring(queryStart + 1);
                foreach (var part in query.Split('&'))
                {
                    var eq = part.IndexOf('=');
                    if (eq < 0)
                        continue;

                    var key = part.Substring(0, eq);
                    var value = UrlDecode(part.Substring(eq + 1));

                    if (key == "requestId")
                        requestId = value;
                    else if (key == "data")
                        encodedData = value;
                }
            }

            if (string.IsNullOrEmpty(requestId) || string.IsNullOrEmpty(encodedData))
                return;

            _taskManager.RunOnMainThread(() => HandleRequest(eventType, requestId, encodedData));
        }
        catch
        {
            // Ignore malformed bridge URLs after cancelling the navigation.
        }
    }

    private void HandleRequest(string eventType, string requestId, string encodedData)
    {
        if (_disposed)
            return;

        string dataJson;
        try
        {
            dataJson = Encoding.UTF8.GetString(Convert.FromBase64String(encodedData));
        }
        catch
        {
            Error(requestId, "Invalid bridge request.");
            return;
        }

        RequestReceived?.Invoke(new NovaBridgeRequest(eventType, requestId, dataJson));
    }

    private void DispatchResponse(string eventType, string requestId, string data)
    {
        if (_disposed || !_webView.IsInsideTree)
            return;

        var js = new StringBuilder();
        js.Append("window.dispatchEvent(new CustomEvent(\"");
        js.Append(eventType);
        js.Append("\",{detail:{requestId:\"");
        js.Append(requestId);
        js.Append("\",data:\"");
        js.Append(data);
        js.Append("\"}}));");

        _webView.ExecuteJavaScript(js.ToString());
    }

    private string GetMimeType(string extension)
    {
        if (_webViewManager.TryGetResourceMimeType(extension, out var mime))
            return mime;

        return extension.ToLowerInvariant() switch
        {
            "html" or "htm" => "text/html",
            "js" or "mjs" => "text/javascript",
            "css" => "text/css",
            "json" => "application/json",
            "svg" => "image/svg+xml",
            "png" => "image/png",
            "jpg" or "jpeg" => "image/jpeg",
            "webp" => "image/webp",
            "woff2" => "font/woff2",
            _ => "application/octet-stream"
        };
    }

    private static string EncodeData(string json)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    private static string JsonEscape(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static string UrlDecode(string s)
    {
        var sb = new StringBuilder(s.Length);
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c == '%' && i + 2 < s.Length)
            {
                var value = ParseHexByte(s[i + 1], s[i + 2]);
                if (value >= 0)
                {
                    sb.Append((char)value);
                    i += 2;
                    continue;
                }
            }
            else if (c == '+')
            {
                sb.Append(' ');
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    private static int ParseHexByte(char high, char low)
    {
        var h = ParseHexDigit(high);
        var l = ParseHexDigit(low);
        if (h < 0 || l < 0)
            return -1;

        return (h << 4) | l;
    }

    private static int ParseHexDigit(char c)
    {
        if (c >= '0' && c <= '9')
            return c - '0';
        if (c >= 'A' && c <= 'F')
            return c - 'A' + 10;
        if (c >= 'a' && c <= 'f')
            return c - 'a' + 10;
        return -1;
    }

    private bool IsTrustedPage()
    {
        return _webView.Url == _trustedPage;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _webView.RemoveBeforeBrowseHandler(OnBeforeBrowse);
        _webView.RemoveResourceRequestHandler(OnResourceRequest);
        RequestReceived = null;
    }
}

public readonly record struct NovaBridgeRequest(string EventType, string RequestId, string DataJson);
