namespace HeadsetControl.NET;

public sealed record EqualizerPreset(byte Index, string Name, IReadOnlyList<float> Bands);
