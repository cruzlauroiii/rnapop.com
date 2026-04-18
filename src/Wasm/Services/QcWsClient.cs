using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Models;

namespace Wasm.Services;

/// <summary>WebSocket client for ESP32 #4 — QC & Imaging.</summary>
public sealed class QcWsClient : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;

    public QcStatus Status { get; private set; } = new();
    public bool Connected => _ws?.State == WebSocketState.Open;
    public string? Error { get; private set; }
    public byte[]? LastImage { get; private set; }
    public event Action? OnStatusChanged;
    public event Action<byte[]>? OnImageReceived;

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
            try { await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None); } catch { }
        _ws?.Dispose(); _ws = null;
        _cts?.Dispose(); _cts = null;
        Status.Connected = false;
    }

    public async Task SendCommandAsync(int command, int value = 0)
    {
        if (_ws is not { State: WebSocketState.Open }) return;
        var json = JsonSerializer.Serialize(new { command, value }, JsonOpts);
        await _ws.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, _cts?.Token ?? CancellationToken.None);
    }

    private async Task ReceiveLoop(CancellationToken ct)
    {
        var buf = new byte[65536]; // Larger buffer for JPEG images
        try
        {
            while (_ws is { State: WebSocketState.Open } && !ct.IsCancellationRequested)
            {
                var result = await _ws.ReceiveAsync(buf, ct);
                if (result.MessageType == WebSocketMessageType.Close) break;

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    // JPEG image from gel imager
                    LastImage = new byte[result.Count];
                    Array.Copy(buf, LastImage, result.Count);
                    OnImageReceived?.Invoke(LastImage);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buf, 0, result.Count);
                    var incoming = JsonSerializer.Deserialize<QcStatus>(json, JsonOpts);
                    if (incoming is not null) { incoming.Connected = true; Status = incoming; OnStatusChanged?.Invoke(); }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
        Status.Connected = false;
        OnStatusChanged?.Invoke();
    }

    public async ValueTask DisposeAsync() => await DisconnectAsync();
}
