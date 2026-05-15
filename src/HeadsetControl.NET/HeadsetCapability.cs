namespace HeadsetControl.NET;

/// <summary>
/// A controllable feature of a headset.
/// </summary>
/// <remarks>
/// Use <see cref="Headset.Supports"/> to check whether a particular device
/// implements a capability before invoking the corresponding operation.
/// </remarks>
public enum HeadsetCapability
{
    /// <summary>Sidetone level (your own voice fed back into the headphones).</summary>
    Sidetone = 0,

    /// <summary>Battery level / status reporting.</summary>
    BatteryStatus = 1,

    /// <summary>Playing a notification sound on the headset.</summary>
    NotificationSound = 2,

    /// <summary>Headset LED lights.</summary>
    Lights = 3,

    /// <summary>Inactive-timer (auto power-off).</summary>
    InactiveTime = 4,

    /// <summary>Chat-mix dial status.</summary>
    ChatMixStatus = 5,

    /// <summary>Spoken voice prompts on the headset.</summary>
    VoicePrompts = 6,

    /// <summary>Mute the microphone by rotating it.</summary>
    RotateToMute = 7,

    /// <summary>Selecting one of the built-in equalizer presets.</summary>
    EqualizerPreset = 8,

    /// <summary>Setting a custom equalizer curve.</summary>
    Equalizer = 9,

    /// <summary>Parametric equalizer.</summary>
    ParametricEqualizer = 10,

    /// <summary>Brightness of the microphone-mute LED.</summary>
    MicrophoneMuteLedBrightness = 11,

    /// <summary>Microphone input volume.</summary>
    MicrophoneVolume = 12,

    /// <summary>Output volume limiter.</summary>
    VolumeLimiter = 13,

    /// <summary>Bluetooth-when-powered-on toggle.</summary>
    BluetoothWhenPoweredOn = 14,

    /// <summary>Bluetooth call volume.</summary>
    BluetoothCallVolume = 15,
}
