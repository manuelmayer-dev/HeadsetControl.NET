namespace HeadsetControl.NET;

public readonly record struct BatteryInfo(
    BatteryStatus Status,
    int? LevelPercent,
    Voltage? Voltage,
    TimeSpan? TimeToFull,
    TimeSpan? TimeToEmpty);

public readonly record struct Voltage(int Millivolts)
{
    public double Volts => Millivolts / 1000.0;
}
