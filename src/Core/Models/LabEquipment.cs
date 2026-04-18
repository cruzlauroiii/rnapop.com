namespace Core.Models;

/// <summary>Real-time state from ESP32-controlled lab equipment.</summary>
public sealed class LabStatus
{
    public HeatBlockStatus HeatBlock { get; set; } = new();
    public SyringePumpStatus[] SyringePumps { get; set; } = [new(), new()];
    public SpectrophotometerStatus Spectrophotometer { get; set; } = new();
    public CentrifugeStatus Centrifuge { get; set; } = new();
    public GelElectrophoresisStatus Gel { get; set; } = new();
    public bool Connected { get; set; }
    public DateTime LastUpdate { get; set; }
}

public sealed class HeatBlockStatus
{
    public double CurrentTemp { get; set; }   // Celsius
    public double TargetTemp { get; set; } = 37.0;
    public bool HeaterOn { get; set; }
    public bool AtTarget { get; set; }
    public double PidKp { get; set; } = 2.0;
    public double PidKi { get; set; } = 0.5;
    public double PidKd { get; set; } = 1.0;
}

public sealed class SyringePumpStatus
{
    public string Name { get; set; } = "";
    public double FlowRateMlMin { get; set; }  // mL/min
    public double VolumeMl { get; set; }        // total volume to dispense
    public double DispensedMl { get; set; }     // already dispensed
    public bool Running { get; set; }
    public int StepsPerMl { get; set; } = 3200; // NEMA17 + leadscrew calibration
}

public sealed class SpectrophotometerStatus
{
    public double A260 { get; set; }     // absorbance at 260nm
    public double A280 { get; set; }     // absorbance at 280nm
    public double Ratio260280 { get; set; } // purity ratio
    public double ConcentrationNgUl { get; set; } // RNA concentration
    public bool MeasurementReady { get; set; }
}

public sealed class CentrifugeStatus
{
    public int CurrentRpm { get; set; }
    public int TargetRpm { get; set; }
    public bool Running { get; set; }
    public int RemainingSeconds { get; set; }
}

public sealed class GelElectrophoresisStatus
{
    public double Voltage { get; set; }
    public double CurrentMa { get; set; }
    public bool Running { get; set; }
    public int RemainingMinutes { get; set; }
}

/// <summary>Commands sent to ESP32.</summary>
public enum LabCommand
{
    HeatBlockSetTemp,
    HeatBlockStart,
    HeatBlockStop,
    PumpSetFlow,
    PumpStart,
    PumpStop,
    SpectrometerMeasure,
    CentrifugeStart,
    CentrifugeStop,
    GelStart,
    GelStop,
}

public sealed class LabCommandMessage
{
    public LabCommand Command { get; set; }
    public int DeviceIndex { get; set; }  // for pumps
    public double Value { get; set; }
    public double Value2 { get; set; }    // secondary param (e.g., duration)
}
