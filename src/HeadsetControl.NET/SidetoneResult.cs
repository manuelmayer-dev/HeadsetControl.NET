namespace HeadsetControl.NET;

/// <summary>
/// Reports the sidetone level after a successful <see cref="Headset.SetSidetone(byte)"/>
/// call, together with the supported range.
/// </summary>
/// <param name="CurrentLevel">The level the headset is now using.</param>
/// <param name="MinLevel">Minimum value accepted by the headset.</param>
/// <param name="MaxLevel">Maximum value accepted by the headset.</param>
public readonly record struct SidetoneResult(byte CurrentLevel, byte MinLevel, byte MaxLevel);

/// <summary>
/// Reports the inactive-timer setting after a successful
/// <see cref="Headset.SetInactiveTime(byte)"/> call, together with the supported range.
/// </summary>
/// <param name="Minutes">Configured timeout in minutes (0 disables auto-power-off).</param>
/// <param name="MinMinutes">Smallest accepted value.</param>
/// <param name="MaxMinutes">Largest accepted value.</param>
public readonly record struct InactiveTimeResult(byte Minutes, byte MinMinutes, byte MaxMinutes);
