namespace HeadsetControl.NET;

public readonly record struct SidetoneResult(byte CurrentLevel, byte MinLevel, byte MaxLevel);

public readonly record struct InactiveTimeResult(byte Minutes, byte MinMinutes, byte MaxMinutes);
