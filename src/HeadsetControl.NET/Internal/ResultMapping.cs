using HeadsetControl.NET.Native;

namespace HeadsetControl.NET.Internal;

internal static class ResultMapping
{
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

    // The native C wrapper currently passes the underlying C++ battery_status
    // values (0..4) straight through a static_cast to hsc_battery_status_t,
    // so we accept both encodings. Value 0 is ambiguous (C++ Unavailable vs
    // documented Available) — we resolve it to Unavailable, matching the
    // current pass-through behaviour.
    public static BatteryStatus MapBatteryStatus(HscBatteryStatus raw)
    {
        int value = (int)raw;
        return value switch
        {
            -1 => BatteryStatus.Unavailable,
            -2 => BatteryStatus.Charging,
            -100 => BatteryStatus.Error,
            -101 => BatteryStatus.Timeout,

            0 => BatteryStatus.Unavailable,
            1 => BatteryStatus.Charging,
            2 => BatteryStatus.Available,
            3 => BatteryStatus.Error,
            4 => BatteryStatus.Timeout,

            _ => BatteryStatus.Unavailable,
        };
    }
}
