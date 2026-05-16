namespace HeadsetControl.NET.Native;

enum HscResult
{
    Ok = 0,
    Error = -1,
    NotSupported = -2,
    DeviceOffline = -3,
    Timeout = -4,
    HidError = -5,
    InvalidParam = -6,
}

enum HscCapability
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

// The native C wrapper currently forwards the underlying C++ battery_status
// enum (0..4) through a static cast, so callers may receive either these
// constants or the C++ values. ResultMapping handles both.
enum HscBatteryStatus
{
    Unavailable = -1,
    Charging = -2,
    Available = 0,
    Error = -100,
    Timeout = -101,
}
