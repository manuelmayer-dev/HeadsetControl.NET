namespace HeadsetControl.NET;

/// <summary>
/// Reports the current charge state of a headset's battery.
/// </summary>
public enum BatteryStatus
{
    /// <summary>Status is currently unavailable (e.g. wireless link not established yet).</summary>
    Unavailable = 0,

    /// <summary>The headset is plugged in and charging.</summary>
    Charging = 1,

    /// <summary>A regular battery level reading is available.</summary>
    Available = 2,

    /// <summary>The device reported a HID-level error while querying the battery.</summary>
    Error = 3,

    /// <summary>The battery query timed out.</summary>
    Timeout = 4,
}
