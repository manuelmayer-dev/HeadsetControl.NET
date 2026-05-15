using System.Runtime.InteropServices;

namespace HeadsetControl.NET.Native;

/// <summary>Mirrors <c>hsc_battery_t</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct HscBattery
{
    public int LevelPercent;
    public HscBatteryStatus Status;
    public int VoltageMv;
    public int TimeToFullMin;
    public int TimeToEmptyMin;
}

/// <summary>Mirrors <c>hsc_sidetone_t</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct HscSidetone
{
    public byte CurrentLevel;
    public byte MinLevel;
    public byte MaxLevel;
}

/// <summary>Mirrors <c>hsc_chatmix_t</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct HscChatMix
{
    public int Level;
    public int GameVolumePercent;
    public int ChatVolumePercent;
}

/// <summary>Mirrors <c>hsc_inactive_time_t</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct HscInactiveTime
{
    public byte Minutes;
    public byte MinMinutes;
    public byte MaxMinutes;
}
