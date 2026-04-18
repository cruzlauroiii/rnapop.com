using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Models;

namespace Wasm.Services;

/// <summary>
/// Client-side WebSocket service running entirely in Blazor WASM.
/// Connects to ESP32 WSS server at wss://0.0.0.0:{port}.
/// Sends LabCommandMessage, receives LabStatus stream.
/// </summary>
public sealed class LabWsClient : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;

    public LabStatus Status { get; private set; } = new();
    public bool Connected => _ws?.State == WebSocketState.Open;
    public string? Error { get; private set; }

    public event Action? OnStatusChanged;

    public async Task ConnectAsync(string wssUrl)
    {
        await DisconnectAsync();
        Error = null;

        try
        {
            _cts = new CancellationTokenSource();
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(wssUrl), _cts.Token);
            Status.Connected = true;
            OnStatusChanged?.Invoke();

            // Start background receive loop
            _ = ReceiveLoop(_cts.Token);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            Status.Connected = false;
            OnStatusChanged?.Invoke();
        }
    }

    public async Task DisconnectAsync()
    {
        _cts?.Cancel();
        if (_ws is { State: WebSocketState.Open })
        {
            try { await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None); }
            catch { }
        }
        _ws?.Dispose();
        _ws = null;
        _cts?.Dispose();
        _cts = null;
        Status.Connected = false;
    }

    public async Task SendCommandAsync(LabCommand command, int deviceIndex = 0, double value = 0, double value2 = 0)
    {
        if (_ws is not { State: WebSocketState.Open }) return;

        var msg = new LabCommandMessage
        {
            Command = command,
            DeviceIndex = deviceIndex,
            Value = value,
            Value2 = value2,
        };

        var json = JsonSerializer.Serialize(msg, JsonOpts);
        var bytes = Encoding.UTF8.GetBytes(json);
        await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, _cts?.Token ?? CancellationToken.None);
    }

    private async Task ReceiveLoop(CancellationToken ct)
    {
        var buf = new byte[8192];
        try
        {
            while (_ws is { State: WebSocketState.Open } && !ct.IsCancellationRequested)
            {
                var result = await _ws.ReceiveAsync(buf, ct);
                if (result.MessageType == WebSocketMessageType.Close) break;
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buf, 0, result.Count);
                    var incoming = JsonSerializer.Deserialize<LabStatus>(json, JsonOpts);
                    if (incoming is not null)
                    {
                        incoming.Connected = true;
                        incoming.LastUpdate = DateTime.Now;
                        Status = incoming;
                        OnStatusChanged?.Invoke();
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }

        Status.Connected = false;
        OnStatusChanged?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
