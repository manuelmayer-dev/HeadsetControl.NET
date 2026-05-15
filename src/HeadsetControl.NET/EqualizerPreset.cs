namespace HeadsetControl.NET;

/// <summary>
/// A built-in equalizer preset exposed by a headset.
/// </summary>
/// <param name="Index">Zero-based preset index, used with <see cref="Headset.SetEqualizerPreset(byte)"/>.</param>
/// <param name="Name">Human-readable preset name.</param>
/// <param name="Bands">Band values that make up the preset curve.</param>
public sealed record EqualizerPreset(byte Index, string Name, IReadOnlyList<float> Bands);
