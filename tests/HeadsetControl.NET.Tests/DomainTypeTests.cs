using HeadsetControl.NET.Exceptions;

namespace HeadsetControl.NET.Tests;

public sealed class DomainTypeTests
{
    [Fact]
    public void BatteryInfo_IsValueEquatable()
    {
        var a = new BatteryInfo(BatteryStatus.Available, 80, new Voltage(3700), null, null);
        var b = new BatteryInfo(BatteryStatus.Available, 80, new Voltage(3700), null, null);
        var c = new BatteryInfo(BatteryStatus.Charging, null, null, null, null);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, c);
    }

    [Theory]
    [InlineData(0, 0.0)]
    [InlineData(3700, 3.7)]
    [InlineData(5000, 5.0)]
    public void Voltage_ConvertsMillivoltsToVolts(int mv, double expected)
    {
        Assert.Equal(expected, new Voltage(mv).Volts, precision: 6);
    }

    [Fact]
    public void EqualizerPreset_RoundTripsProperties()
    {
        var preset = new EqualizerPreset(0, "Flat", new float[] { 0f, 0f, 0f, 0f });

        Assert.Equal(0, preset.Index);
        Assert.Equal("Flat", preset.Name);
        Assert.Equal(4, preset.Bands.Count);
    }

    [Theory]
    [InlineData(HeadsetCapability.Sidetone, 0)]
    [InlineData(HeadsetCapability.BatteryStatus, 1)]
    [InlineData(HeadsetCapability.BluetoothCallVolume, 15)]
    public void HeadsetCapability_MatchesNativeBitPositions(HeadsetCapability cap, int expectedBit)
    {
        Assert.Equal(expectedBit, (int)cap);
    }

    [Fact]
    public void HeadsetControlException_KeepsErrorCode()
    {
        var ex = new HeadsetControlException(HeadsetControlErrorCode.Timeout, "boom");
        Assert.Equal(HeadsetControlErrorCode.Timeout, ex.ErrorCode);
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public void FeatureNotSupportedException_PreservesCapability()
    {
        var ex = new FeatureNotSupportedException(HeadsetCapability.Equalizer);
        Assert.Equal(HeadsetCapability.Equalizer, ex.Capability);
        Assert.Contains("Equalizer", ex.Message, StringComparison.Ordinal);
    }
}
