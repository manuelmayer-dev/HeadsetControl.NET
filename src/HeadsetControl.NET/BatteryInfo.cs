namespace HeadsetControl.NET;

/// <summary>
/// Snapshot of a headset's battery state.
/// </summary>
/// <param name="Status">Whether the reading is available, charging, etc.</param>
/// <param name="LevelPercent">
/// Charge level in the range 0-100, or <see langword="null"/> when not applicable
/// (for example while <see cref="BatteryStatus.Charging"/> or
/// <see cref="BatteryStatus.Unavailable"/>).
/// </param>
/// <param name="Voltage">Battery voltage, or <see langword="null"/> if the device does not report it.</param>
/// <param name="TimeToFull">Estimated time until fully charged, or <see langword="null"/> if unknown.</param>
/// <param name="TimeToEmpty">Estimated time until fully discharged, or <see langword="null"/> if unknown.</param>
public readonly record struct BatteryInfo(
    BatteryStatus Status,
    int? LevelPercent,
    Voltage? Voltage,
    TimeSpan? TimeToFull,
    TimeSpan? TimeToEmpty);

/// <summary>
/// Strongly-typed wrapper around a millivolt voltage reading.
/// </summary>
/// <param name="Millivolts">Voltage in millivolts. Always non-negative.</param>
public readonly record struct Voltage(int Millivolts)
{
    /// <summary>The voltage in volts.</summary>
    public double Volts => Millivolts / 1000.0;
}
