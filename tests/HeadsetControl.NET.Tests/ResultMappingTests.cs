using HeadsetControl.NET.Internal;
using HeadsetControl.NET.Native;

namespace HeadsetControl.NET.Tests;

/// <summary>
/// Tests that the native error-code → managed-exception mapping is correct
/// for every documented <see cref="HscResult"/> value, plus the battery
/// status normalisation.
/// </summary>
/// <remarks>
/// These tests do not touch the native library — they exercise the pure
/// managed translation layer, so they run on every CI host.
/// </remarks>
public sealed class ResultMappingTests
{
    [Fact]
    public void ThrowIfError_Ok_DoesNotThrow()
    {
        ResultMapping.ThrowIfError(HscResult.Ok, "noop");
    }

    [Fact]
    public void ThrowIfError_NotSupported_ThrowsFeatureNotSupported()
    {
        FeatureNotSupportedException ex = Assert.Throws<FeatureNotSupportedException>(
            () => ResultMapping.ThrowIfError(HscResult.NotSupported, "op", HeadsetCapability.Sidetone));

        Assert.Equal(HeadsetCapability.Sidetone, ex.Capability);
        Assert.Equal(HeadsetControlErrorCode.NotSupported, ex.ErrorCode);
    }

    [Fact]
    public void ThrowIfError_NotSupported_WithoutCapability_StillThrows()
    {
        FeatureNotSupportedException ex = Assert.Throws<FeatureNotSupportedException>(
            () => ResultMapping.ThrowIfError(HscResult.NotSupported, "op"));

        Assert.Null(ex.Capability);
    }

    [Fact]
    public void ThrowIfError_DeviceOffline_ThrowsDeviceOffline()
    {
        DeviceOfflineException ex = Assert.Throws<DeviceOfflineException>(
            () => ResultMapping.ThrowIfError(HscResult.DeviceOffline, "op"));

        Assert.Equal(HeadsetControlErrorCode.DeviceOffline, ex.ErrorCode);
    }

    [Fact]
    public void ThrowIfError_Timeout_ThrowsDeviceTimeout()
    {
        Assert.Throws<DeviceTimeoutException>(
            () => ResultMapping.ThrowIfError(HscResult.Timeout, "op"));
    }

    [Fact]
    public void ThrowIfError_HidError_ThrowsHidCommunication()
    {
        Assert.Throws<HidCommunicationException>(
            () => ResultMapping.ThrowIfError(HscResult.HidError, "op"));
    }

    [Fact]
    public void ThrowIfError_InvalidParam_ThrowsInvalidParameter()
    {
        Assert.Throws<HeadsetControlInvalidParameterException>(
            () => ResultMapping.ThrowIfError(HscResult.InvalidParam, "op"));
    }

    [Fact]
    public void ThrowIfError_GenericError_ThrowsBaseException()
    {
        HeadsetControlException ex = Assert.Throws<HeadsetControlException>(
            () => ResultMapping.ThrowIfError(HscResult.Error, "op"));

        Assert.Equal(HeadsetControlErrorCode.Error, ex.ErrorCode);
    }

    [Theory]
    [InlineData(-1, BatteryStatus.Unavailable)]    // HSC_BATTERY_UNAVAILABLE
    [InlineData(-2, BatteryStatus.Charging)]       // HSC_BATTERY_CHARGING
    [InlineData(-100, BatteryStatus.Error)]        // HSC_BATTERY_ERROR
    [InlineData(-101, BatteryStatus.Timeout)]      // HSC_BATTERY_TIMEOUT
    public void MapBatteryStatus_DocumentedHscValues(int rawValue, BatteryStatus expected)
    {
        Assert.Equal(expected, ResultMapping.MapBatteryStatus((HscBatteryStatus)rawValue));
    }

    [Theory]
    [InlineData(0, BatteryStatus.Unavailable)]
    [InlineData(1, BatteryStatus.Charging)]
    [InlineData(2, BatteryStatus.Available)]
    [InlineData(3, BatteryStatus.Error)]
    [InlineData(4, BatteryStatus.Timeout)]
    public void MapBatteryStatus_CppPassThroughValues(int rawValue, BatteryStatus expected)
    {
        Assert.Equal(expected, ResultMapping.MapBatteryStatus((HscBatteryStatus)rawValue));
    }

    [Fact]
    public void MapBatteryStatus_UnknownValue_DefaultsToUnavailable()
    {
        Assert.Equal(BatteryStatus.Unavailable, ResultMapping.MapBatteryStatus((HscBatteryStatus)9999));
    }
}
