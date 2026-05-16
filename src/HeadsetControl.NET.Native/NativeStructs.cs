using System.Runtime.InteropServices;

namespace HeadsetControl.NET.Native;

[StructLayout(LayoutKind.Sequential)]
struct HscBattery
{
    public int LevelPercent;
    public HscBatteryStatus Status;
    public int VoltageMv;
    public int TimeToFullMin;
    public int TimeToEmptyMin;
}

[StructLayout(LayoutKind.Sequential)]
struct HscSidetone
{
    public byte CurrentLevel;
    public byte MinLevel;
    public byte MaxLevel;
}

[StructLayout(LayoutKind.Sequential)]
struct HscChatMix
{
    public int Level;
    public int GameVolumePercent;
    public int ChatVolumePercent;
}

[StructLayout(LayoutKind.Sequential)]
struct HscInactiveTime
{
    public byte Minutes;
    public byte MinMinutes;
    public byte MaxMinutes;
}
