namespace HeadsetControl.NET.Native;

/// <summary>
/// Mirrors <c>hsc_result_t</c> from <c>headsetcontrol_c.h</c>.
/// </summary>
internal enum HscResult
{
    Ok = 0,
    Error = -1,
    NotSupported = -2,
    DeviceOffline = -3,
    Timeout = -4,
    HidError = -5,
    InvalidParam = -6,
}

/// <summary>
/// Mirrors <c>hsc_capability_t</c> from <c>headsetcontrol_c.h</c>.
/// Values are stable and match the C header — they must not be reordered.
/// </summary>
internal enum HscCapability
{
    Sidetone = 0,
    BatteryStatus = 1,
    NotificationSound = 2,
    Lights = 3,
    InactiveTime = 4,
    ChatMixStatus = 5,
    VoicePrompts = 6,
    RotateToMute = 7,
    EqualizerPreset = 8,
    Equalizer = 9,
    ParametricEqualizer = 10,
    MicrophoneMuteLedBrightness = 11,
    MicrophoneVolume = 12,
    VolumeLimiter = 13,
    BluetoothWhenPoweredOn = 14,
    BluetoothCallVolume = 15,
}

/// <summary>
/// Mirrors <c>hsc_battery_status_t</c> from <c>headsetcontrol_c.h</c>.
/// </summary>
/// <remarks>
/// The native C wrapper passes the raw C++ <c>battery_status</c> enum values
/// through a static cast, so callers may receive either the C-API constants
/// declared here or the underlying C++ values (0..4). The managed layer
/// normalizes both encodings.
/// </remarks>
internal enum HscBatteryStatus
{
    Unavailable = -1,
    Charging = -2,
    Available = 0,
    Error = -100,
    Timeout = -101,
}
