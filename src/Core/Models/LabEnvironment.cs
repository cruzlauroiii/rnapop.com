namespace Core.Models;

/// <summary>State from ESP32 #3 — Environment & Safety Controller.</summary>
public sealed class EnvironmentStatus
{
    public RoomStatus Room { get; set; } = new();
    public StorageStatus Freezer { get; set; } = new();
    public StorageStatus Fridge { get; set; } = new();
    public PhStatus Ph { get; set; } = new();
    public StirrerStatus Stirrer { get; set; } = new();
    public VortexStatus Vortex { get; set; } = new();
    public UvcStatus Uvc { get; set; } = new();
    public FumeHoodStatus FumeHood { get; set; } = new();
    public SafetyStatus Safety { get; set; } = new();
    public bool Connected { get; set; }
}

public sealed class RoomStatus
{
    public double TempC { get; set; }
    public double Humidity { get; set; }
}

public sealed class StorageStatus
{
    public double TempC { get; set; }
    public bool Alarm { get; set; }
}

public sealed class PhStatus
{
    public double Value { get; set; } = 7.0;
    public double TempC { get; set; }
    public bool Ready { get; set; }
}

public sealed class StirrerStatus
{
    public int Rpm { get; set; }
    public int TargetRpm { get; set; }
    public bool Running { get; set; }
}

public sealed class VortexStatus
{
    public int Speed { get; set; }
    public bool Running { get; set; }
}

public sealed class UvcStatus
{
    public bool On { get; set; }
    public int RemainingMin { get; set; }
}

public sealed class FumeHoodStatus
{
    public bool On { get; set; }
    public double Airflow { get; set; }
}

public sealed class SafetyStatus
{
    public bool Estop { get; set; }
    public bool DoorOpen { get; set; }
    public double GasLevel { get; set; }
    public bool GasAlarm { get; set; }
}

/// <summary>State from ESP32 #4 — QC & Imaging Controller.</summary>
public sealed class QcStatus
{
    public GelImagerStatus GelImager { get; set; } = new();
    public DlsStatus Dls { get; set; } = new();
    public TurbidityStatus Turbidity { get; set; } = new();
    public bool Connected { get; set; }
}

public sealed class GelImagerStatus
{
    public bool TransilluminatorOn { get; set; }
    public int Brightness { get; set; }
    public bool FilterEngaged { get; set; }
    public bool ImageReady { get; set; }
    public int ImageSize { get; set; }
}

public sealed class DlsStatus
{
    public bool Running { get; set; }
    public double ParticleSizeNm { get; set; }
    public double Pdi { get; set; }
    public bool Ready { get; set; }
    public bool SizeOk { get; set; }
}

public sealed class TurbidityStatus
{
    public double Od { get; set; }
    public double Transmission { get; set; }
    public bool Ready { get; set; }
}

/// <summary>Commands for ESP32 #3 and #4.</summary>
public enum EnvCommand
{
    PhMeasure = 20,
    StirrerSetRpm = 21,
    StirrerStop = 22,
    VortexSetSpeed = 23,
    VortexStop = 24,
    UvcStart = 25,
    UvcStop = 26,
    FumeHoodOn = 27,
    FumeHoodOff = 28,
    EstopReset = 29,
}

public enum QcCommand
{
    CaptureImage = 30,
    TransilluminatorOn = 31,
    TransilluminatorOff = 32,
    SetBrightness = 33,
    FilterEngage = 34,
    FilterDisengage = 35,
    DlsMeasure = 36,
    TurbidityMeasure = 37,
}
