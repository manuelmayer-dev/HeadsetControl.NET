namespace HeadsetControl.NET;

/// <summary>
/// Stable identifier for a native HeadsetControl error.
/// </summary>
/// <remarks>
/// Exposed primarily for diagnostic logging — application code should typically
/// branch on exception type (e.g. <see cref="DeviceOfflineException"/>) rather
/// than inspect the code directly.
/// </remarks>
public enum HeadsetControlErrorCode
{
    /// <summary>Operation succeeded.</summary>
    Ok = 0,

    /// <summary>Generic / unspecified error.</summary>
    Error = -1,

    /// <summary>The requested feature is not supported by the device.</summary>
    NotSupported = -2,

    /// <summary>The device is currently offline.</summary>
    DeviceOffline = -3,

    /// <summary>A HID read or write operation timed out.</summary>
    Timeout = -4,

    /// <summary>Underlying HID communication failed.</summary>
    HidError = -5,

    /// <summary>A parameter was rejected by the native library.</summary>
    InvalidParameter = -6,
}
