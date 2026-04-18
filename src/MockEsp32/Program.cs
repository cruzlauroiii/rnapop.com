using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Models;

int port = Random.Shared.Next(9100, 9999);

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"https://0.0.0.0:{port}");
builder.Services.AddCors();

var app = builder.Build();
app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseWebSockets();

var sim = new LabSimulator();

// Background tick
_ = Task.Run(async () =>
{
    while (true) { sim.Tick(); await Task.Delay(500); }
});

// WebSocket on root path — wss://0.0.0.0:{port} (no /ws)
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var ws = await context.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine($"[+] Client connected");
        await HandleClient(ws, sim, context.RequestAborted);
    }
    else
    {
        await next();
    }
});

app.MapGet("/", () => Results.Json(new { wss = $"wss://0.0.0.0:{port}", status = "running" }));

Console.WriteLine("=======================================");
Console.WriteLine(" Mock ESP32 Lab — All 4 Controllers");
Console.WriteLine("=======================================");
Console.WriteLine();
Console.WriteLine($"  wss://0.0.0.0:{port}");
Console.WriteLine();
Console.WriteLine("  Simulates all 17 lab instruments.");
Console.WriteLine("  Paste the URL into the Blazor WASM");
Console.WriteLine("  Lab / Environment / QC dashboards.");
Console.WriteLine();
Console.WriteLine("  Press Ctrl+C to stop.");
Console.WriteLine("=======================================");

app.Run();

// ===== WebSocket Handler =====

static async Task HandleClient(WebSocket ws, LabSimulator sim, CancellationToken ct)
{
    var opts = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    var buf = new byte[4096];

    using var statusCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    var statusTask = Task.Run(async () =>
    {
        while (!statusCts.Token.IsCancellationRequested && ws.State == WebSocketState.Open)
        {
            await Task.Delay(500, statusCts.Token).ConfigureAwait(false);
            try
            {
                var json = JsonSerializer.Serialize(sim.GetFullStatus(), opts);
                await ws.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, statusCts.Token);
            }
            catch { break; }
        }
    }, statusCts.Token);

    try
    {
        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            var result = await ws.ReceiveAsync(buf, ct);
            if (result.MessageType == WebSocketMessageType.Close) break;
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var json = Encoding.UTF8.GetString(buf, 0, result.Count);
                try
                {
                    var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    // Robust parsing — handle both int and string enum values
                    int cmd = 0;
                    if (root.TryGetProperty("command", out var cmdEl))
                    {
                        if (cmdEl.ValueKind == JsonValueKind.Number) cmd = cmdEl.GetInt32();
                        else if (cmdEl.ValueKind == JsonValueKind.String)
                        {
                            // Handle string enum names from C# JsonStringEnumConverter
                            cmd = cmdEl.GetString() switch
                            {
                                "HeatBlockSetTemp" or "heatBlockSetTemp" => 0,
                                "HeatBlockStart" or "heatBlockStart" => 1,
                                "HeatBlockStop" or "heatBlockStop" => 2,
                                "PumpSetFlow" or "pumpSetFlow" => 3,
                                "PumpStart" or "pumpStart" => 4,
                                "PumpStop" or "pumpStop" => 5,
                                "SpectrometerMeasure" or "spectrometerMeasure" => 6,
                                "CentrifugeStart" or "centrifugeStart" => 7,
                                "CentrifugeStop" or "centrifugeStop" => 8,
                                "GelStart" or "gelStart" => 9,
                                "GelStop" or "gelStop" => 10,
                                _ when int.TryParse(cmdEl.GetString(), out var n) => n,
                                _ => -1,
                            };
                        }
                    }

                    double val = 0, val2 = 0;
                    int idx = 0;
                    if (root.TryGetProperty("value", out var vEl))
                    {
                        if (vEl.ValueKind == JsonValueKind.Number) val = vEl.GetDouble();
                    }
                    if (root.TryGetProperty("value2", out var v2El))
                    {
                        if (v2El.ValueKind == JsonValueKind.Number) val2 = v2El.GetDouble();
                    }
                    if (root.TryGetProperty("deviceIndex", out var diEl))
                    {
                        if (diEl.ValueKind == JsonValueKind.Number) idx = diEl.GetInt32();
                    }

                    Console.WriteLine($"  CMD: {cmd} idx={idx} val={val} val2={val2}");
                    sim.HandleCommand(cmd, idx, val, val2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Parse error: {ex.Message} — raw: {json[..Math.Min(json.Length, 200)]}");
                }

                // Always send status back, even if parse failed
                try
                {
                    var status = JsonSerializer.Serialize(sim.GetFullStatus(), opts);
                    if (ws.State == WebSocketState.Open)
                        await ws.SendAsync(Encoding.UTF8.GetBytes(status), WebSocketMessageType.Text, true, ct);
                }
                catch { }
            }
        }
    }
    catch (WebSocketException ex) { Console.WriteLine($"  WS error: {ex.Message}"); }
    catch (OperationCanceledException) { }
    finally
    {
        statusCts.Cancel();
        try { await statusTask; } catch { }
        if (ws.State == WebSocketState.Open)
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
        Console.WriteLine("[-] Client disconnected");
    }
}

// ===== Simulator =====

class LabSimulator
{
    // ESP32 #1
    double heatTemp = 22.0, heatTarget = 37.0;
    bool heaterOn = false;
    double[] pumpFlow = [0, 0], pumpVol = [0, 0], pumpDispensed = [0, 0];
    bool[] pumpRunning = [false, false];
    double a260 = 0, a280 = 0;
    bool spectroReady = false;
    int centRpm = 0, centTarget = 0, centRemain = 0;
    bool centRunning = false;
    double gelVoltage = 0, gelCurrent = 0;
    int gelRemain = 0;
    bool gelRunning = false;

    // ESP32 #3
    double roomTemp = 22.5, roomHumidity = 45.0;
    double freezerTemp = -80.0, fridgeTemp = 4.0;
    double phValue = 7.0, phTemp = 22.0;
    bool phReady = false;
    int stirrerRpm = 0, stirrerTarget = 0;
    bool stirrerRunning = false;
    int vortexSpeed = 0;
    bool vortexRunning = false;
    bool uvcOn = false;
    int uvcRemain = 0;
    bool fumeHoodOn = false;
    double airflow = 0;
    bool estop = false, doorOpen = false, gasAlarm = false;
    double gasLevel = 150;

    // ESP32 #4
    bool transOn = false;
    int transBrightness = 200;
    bool filterEngaged = false;
    double dlsSize = 0, dlsPdi = 0;
    bool dlsReady = false, dlsRunning = false;
    double turbOd = 0, turbTrans = 0;
    bool turbReady = false;

    readonly Random rng = new();

    public void Tick()
    {
        if (heaterOn)
        {
            double delta = heatTarget - heatTemp;
            heatTemp += delta * 0.08 + (rng.NextDouble() - 0.5) * 0.1;
        }
        else { heatTemp += (22.0 - heatTemp) * 0.02; }

        for (int i = 0; i < 2; i++)
        {
            if (!pumpRunning[i]) continue;
            pumpDispensed[i] += pumpFlow[i] / 60.0 * 0.5;
            if (pumpDispensed[i] >= pumpVol[i])
            { pumpDispensed[i] = pumpVol[i]; pumpRunning[i] = false; Console.WriteLine($"  Pump {(char)('A' + i)}: done ({pumpVol[i]:F1} mL)"); }
        }

        if (centRunning)
        { centRpm = centTarget; centRemain = Math.Max(0, centRemain - 1); if (centRemain <= 0) { centRunning = false; centRpm = 0; Console.WriteLine("  Centrifuge: done"); } }

        if (gelRunning)
        { gelCurrent = gelVoltage * 0.3 + rng.NextDouble() * 2; gelRemain = Math.Max(0, gelRemain - 1); if (gelRemain <= 0) { gelRunning = false; gelVoltage = 0; gelCurrent = 0; Console.WriteLine("  Gel: done"); } }

        roomTemp = 22.5 + (rng.NextDouble() - 0.5) * 0.3;
        roomHumidity = 45.0 + (rng.NextDouble() - 0.5) * 2;
        freezerTemp = -80.0 + (rng.NextDouble() - 0.5) * 0.5;
        fridgeTemp = 4.0 + (rng.NextDouble() - 0.5) * 0.3;
        gasLevel = 150 + rng.NextDouble() * 20;
        if (fumeHoodOn) airflow = 75 + rng.NextDouble() * 10;

        if (uvcOn) { uvcRemain = Math.Max(0, uvcRemain - 1); if (uvcRemain <= 0) { uvcOn = false; Console.WriteLine("  UV-C: done"); } }

        if (dlsRunning)
        { dlsSize = 72 + rng.NextDouble() * 20; dlsPdi = 0.08 + rng.NextDouble() * 0.12; dlsReady = true; dlsRunning = false; Console.WriteLine($"  DLS: {dlsSize:F1} nm, PDI {dlsPdi:F3}"); }
    }

    public void HandleCommand(int cmd, int idx, double val, double val2)
    {
        switch (cmd)
        {
            case 0: heatTarget = Math.Clamp(val, 20, 100); Console.WriteLine($"  Heat → {heatTarget:F1}°C"); break;
            case 1: heaterOn = true; Console.WriteLine("  Heater ON"); break;
            case 2: heaterOn = false; Console.WriteLine("  Heater OFF"); break;
            case 3: idx = Math.Clamp(idx, 0, 1); pumpFlow[idx] = val; pumpVol[idx] = val2; pumpDispensed[idx] = 0; pumpRunning[idx] = true; Console.WriteLine($"  Pump {(char)('A' + idx)}: {val:F1} mL/min, {val2:F1} mL"); break;
            case 4: pumpRunning[Math.Clamp(idx, 0, 1)] = true; break;
            case 5: pumpRunning[Math.Clamp(idx, 0, 1)] = false; Console.WriteLine($"  Pump {(char)('A' + idx)} stopped"); break;
            case 6: a260 = 0.8 + rng.NextDouble() * 0.4; a280 = a260 / (1.95 + rng.NextDouble() * 0.1); spectroReady = true; Console.WriteLine($"  Spectro: A260={a260:F3} ratio={a260 / a280:F2}"); break;
            case 7: centTarget = (int)val; centRemain = (int)val2; centRunning = true; Console.WriteLine($"  Centrifuge: {centTarget} RPM, {centRemain}s"); break;
            case 8: centRunning = false; centRpm = 0; break;
            case 9: gelVoltage = val; gelRemain = (int)val2; gelRunning = true; Console.WriteLine($"  Gel: {val:F0}V, {val2:F0} min"); break;
            case 10: gelRunning = false; gelVoltage = 0; gelCurrent = 0; break;
            case 20: phValue = 3.8 + rng.NextDouble() * 0.4; phTemp = roomTemp; phReady = true; Console.WriteLine($"  pH: {phValue:F2}"); break;
            case 21: stirrerTarget = (int)val; stirrerRpm = stirrerTarget; stirrerRunning = true; Console.WriteLine($"  Stirrer: {stirrerTarget} RPM"); break;
            case 22: stirrerRunning = false; stirrerRpm = 0; break;
            case 23: vortexSpeed = (int)val; vortexRunning = true; Console.WriteLine($"  Vortex: {vortexSpeed}%"); break;
            case 24: vortexRunning = false; vortexSpeed = 0; break;
            case 25: uvcOn = true; uvcRemain = (int)val * 120; Console.WriteLine($"  UV-C: {(int)val} min"); break;
            case 26: uvcOn = false; break;
            case 27: fumeHoodOn = true; airflow = 80; Console.WriteLine("  Fume hood ON"); break;
            case 28: fumeHoodOn = false; airflow = 0; break;
            case 29: estop = false; Console.WriteLine("  E-stop reset"); break;
            case 30: Console.WriteLine("  Gel image captured"); break;
            case 31: transOn = true; break;
            case 32: transOn = false; break;
            case 33: transBrightness = (int)val; break;
            case 34: filterEngaged = true; break;
            case 35: filterEngaged = false; break;
            case 36: dlsRunning = true; Console.WriteLine("  DLS measuring..."); break;
            case 37: turbOd = 0.3 + rng.NextDouble() * 0.4; turbTrans = Math.Pow(10, -turbOd) * 100; turbReady = true; Console.WriteLine($"  Turbidity: OD={turbOd:F3}"); break;
            default: Console.WriteLine($"  Unknown: {cmd}"); break;
        }
    }

    public object GetFullStatus() => new
    {
        heatBlock = new { currentTemp = Math.Round(heatTemp, 1), targetTemp = heatTarget, heaterOn, atTarget = Math.Abs(heatTemp - heatTarget) < 0.5, pidKp = 2.0, pidKi = 0.5, pidKd = 1.0 },
        syringePumps = new[]
        {
            new { name = "A (mRNA/Aqueous)", flowRateMlMin = Math.Round(pumpFlow[0], 2), volumeMl = Math.Round(pumpVol[0], 1), dispensedMl = Math.Round(pumpDispensed[0], 2), running = pumpRunning[0], stepsPerMl = 3200 },
            new { name = "B (Lipid/Ethanol)", flowRateMlMin = Math.Round(pumpFlow[1], 2), volumeMl = Math.Round(pumpVol[1], 1), dispensedMl = Math.Round(pumpDispensed[1], 2), running = pumpRunning[1], stepsPerMl = 3200 },
        },
        spectrophotometer = new { a260 = Math.Round(a260, 3), a280 = Math.Round(a280, 3), ratio260280 = a280 > 0.001 ? Math.Round(a260 / a280, 2) : 0, concentrationNgUl = Math.Round(a260 * 40, 1), measurementReady = spectroReady },
        centrifuge = new { currentRpm = centRpm, targetRpm = centTarget, running = centRunning, remainingSeconds = centRemain },
        gel = new { voltage = gelVoltage, currentMa = Math.Round(gelCurrent, 1), running = gelRunning, remainingMinutes = gelRemain / 120 },
        connected = true,
        room = new { tempC = Math.Round(roomTemp, 1), humidity = Math.Round(roomHumidity, 1) },
        freezer = new { tempC = Math.Round(freezerTemp, 1), alarm = freezerTemp > -70 },
        fridge = new { tempC = Math.Round(fridgeTemp, 1), alarm = fridgeTemp < 1 || fridgeTemp > 10 },
        ph = new { value = Math.Round(phValue, 2), tempC = Math.Round(phTemp, 1), ready = phReady },
        stirrer = new { rpm = stirrerRpm, targetRpm = stirrerTarget, running = stirrerRunning },
        vortex = new { speed = vortexSpeed, running = vortexRunning },
        uvc = new { on = uvcOn, remainingMin = uvcRemain / 120 },
        fumeHood = new { on = fumeHoodOn, airflow = Math.Round(airflow, 1) },
        safety = new { estop, doorOpen, gasLevel = Math.Round(gasLevel), gasAlarm },
        gelImager = new { transilluminatorOn = transOn, brightness = transBrightness, filterEngaged, imageReady = false, imageSize = 0 },
        dls = new { running = dlsRunning, particleSizeNm = Math.Round(dlsSize, 1), pdi = Math.Round(dlsPdi, 3), ready = dlsReady, sizeOk = dlsReady && dlsSize >= 60 && dlsSize <= 100 },
        turbidity = new { od = Math.Round(turbOd, 3), transmission = Math.Round(turbTrans, 1), ready = turbReady },
    };
}
