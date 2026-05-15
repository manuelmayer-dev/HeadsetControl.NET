using HeadsetControl.NET.Native;

namespace HeadsetControl.NET.Internal;

/// <summary>
/// Bridges raw <see cref="HscResult"/> codes to managed <see cref="HeadsetControlException"/>
/// instances, and normalizes the bimodal battery-status encoding from the C API.
/// </summary>
internal static class ResultMapping
{
    /// <summary>
    /// Throws the appropriate exception when <paramref name="result"/> is not
    /// <see cref="HscResult.Ok"/>.
    /// </summary>
    public static void ThrowIfError(HscResult result, string operation, HeadsetCapability? capability = null)
    {
        switch (result)
        {
            case HscResult.Ok:
                return;

            case HscResult.NotSupported:
                throw capability is { } cap
                    ? new FeatureNotSupportedException(cap)
                    : new FeatureNotSupportedException($"{operation} is not supported by this headset.");

            case HscResult.DeviceOffline:
                throw new DeviceOfflineException($"{operation} failed: the headset is offline.");

            case HscResult.Timeout:
                throw new DeviceTimeoutException($"{operation} timed out.");

            case HscResult.HidError:
                throw new HidCommunicationException($"{operation} failed: HID communication error.");

            case HscResult.InvalidParam:
                throw new HeadsetControlInvalidParameterException(
                    $"{operation} failed: the native library rejected a parameter.");

            case HscResult.Error:
            default:
                throw new HeadsetControlException(
                    (HeadsetControlErrorCode)result,
                    $"{operation} failed with native error code {(int)result}.");
        }
    }

    /// <summary>
    /// Translates the raw status integer returned by the native library into a
    /// managed <see cref="BatteryStatus"/>.
    /// </summary>
    /// <remarks>
    /// The native C wrapper currently forwards the underlying C++ <c>battery_status</c>
    /// enum (values 0..4) through a static cast to <c>hsc_battery_status_t</c>.
    /// This method accepts both encodings so the library will keep working if the
    /// C API is ever fixed to emit the documented negative constants.
    /// </remarks>
    public static BatteryStatus MapBatteryStatus(HscBatteryStatus raw)
    {
        int value = (int)raw;
        return value switch
        {
            // hsc_battery_status_t (documented negative encoding)
            -1 => BatteryStatus.Unavailable,
            -2 => BatteryStatus.Charging,
            -100 => BatteryStatus.Error,
            -101 => BatteryStatus.Timeout,

            // C++ battery_status (current pass-through encoding)
            0 => BatteryStatus.Unavailable,
            1 => BatteryStatus.Charging,
            2 => BatteryStatus.Available,
            3 => BatteryStatus.Error,
            4 => BatteryStatus.Timeout,

            _ => BatteryStatus.Unavailable,
        };
    }
}
