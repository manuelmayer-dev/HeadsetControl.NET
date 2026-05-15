using HeadsetControl.NET.Internal;
using HeadsetControl.NET.Native;

namespace HeadsetControl.NET;

/// <summary>
/// A discovered USB headset. Becomes invalid once the owning
/// <see cref="HeadsetCollection"/> is disposed.
/// </summary>
public sealed class Headset
{
    private readonly HeadsetCollection _owner;
    private readonly IntPtr _handle;

    public string Name { get; }
    public ushort VendorId { get; }
    public string? VendorName { get; }
    public ushort ProductId { get; }
    public string? ProductName { get; }
    public int CapabilitiesBitmask { get; }

    internal Headset(HeadsetCollection owner, IntPtr handle)
    {
        _owner = owner;
        _handle = handle;
        Name = NativeStringMarshaller.PtrToStringOrEmpty(NativeMethods.GetName(handle));
        VendorId = NativeMethods.GetVendorId(handle);
        VendorName = NativeStringMarshaller.PtrToString(NativeMethods.GetVendorName(handle));
        ProductId = NativeMethods.GetProductId(handle);
        ProductName = NativeStringMarshaller.PtrToString(NativeMethods.GetProductName(handle));
        CapabilitiesBitmask = NativeMethods.GetCapabilities(handle);
    }

    public IEnumerable<HeadsetCapability> SupportedCapabilities
    {
        get
        {
            foreach (HeadsetCapability cap in Enum.GetValues<HeadsetCapability>())
            {
                if ((CapabilitiesBitmask & (1 << (int)cap)) != 0)
                {
                    yield return cap;
                }
            }
        }
    }

    public bool Supports(HeadsetCapability capability)
    {
        ThrowIfHandleInvalid();
        return NativeMethods.Supports(_handle, (HscCapability)(int)capability);
    }

    public BatteryInfo GetBattery()
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.GetBattery(_handle, out HscBattery raw);
        ResultMapping.ThrowIfError(result, "GetBattery", HeadsetCapability.BatteryStatus);

        BatteryStatus status = ResultMapping.MapBatteryStatus(raw.Status);
        int? level = status == BatteryStatus.Available ? raw.LevelPercent : null;
        Voltage? voltage = raw.VoltageMv >= 0 ? new Voltage(raw.VoltageMv) : null;
        TimeSpan? timeToFull = raw.TimeToFullMin >= 0 ? TimeSpan.FromMinutes(raw.TimeToFullMin) : null;
        TimeSpan? timeToEmpty = raw.TimeToEmptyMin >= 0 ? TimeSpan.FromMinutes(raw.TimeToEmptyMin) : null;

        return new BatteryInfo(status, level, voltage, timeToFull, timeToEmpty);
    }

    public ChatMixInfo GetChatMix()
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.GetChatMix(_handle, out HscChatMix raw);
        ResultMapping.ThrowIfError(result, "GetChatMix", HeadsetCapability.ChatMixStatus);
        return new ChatMixInfo(raw.Level, raw.GameVolumePercent, raw.ChatVolumePercent);
    }

    /// <summary>Sets the sidetone level (0..128, 0 = off).</summary>
    public SidetoneResult SetSidetone(byte level)
    {
        ThrowIfHandleInvalid();
        if (level > 128)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, "Sidetone level must be in 0..128.");
        }

        HscResult result = NativeMethods.SetSidetone(_handle, level, out HscSidetone raw);
        ResultMapping.ThrowIfError(result, "SetSidetone", HeadsetCapability.Sidetone);
        return new SidetoneResult(raw.CurrentLevel, raw.MinLevel, raw.MaxLevel);
    }

    public void SetVolumeLimiter(bool enabled)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetVolumeLimiter(_handle, enabled);
        ResultMapping.ThrowIfError(result, "SetVolumeLimiter", HeadsetCapability.VolumeLimiter);
    }

    public void SetEqualizerPreset(byte preset)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetEqualizerPreset(_handle, preset);
        ResultMapping.ThrowIfError(result, "SetEqualizerPreset", HeadsetCapability.EqualizerPreset);
    }

    public void SetEqualizer(ReadOnlySpan<float> bands)
    {
        ThrowIfHandleInvalid();
        if (bands.IsEmpty)
        {
            throw new ArgumentException("Equalizer band array must not be empty.", nameof(bands));
        }

        unsafe
        {
            fixed (float* p = bands)
            {
                HscResult result = NativeMethods.SetEqualizer(_handle, p, bands.Length);
                ResultMapping.ThrowIfError(result, "SetEqualizer", HeadsetCapability.Equalizer);
            }
        }
    }

    public IReadOnlyList<EqualizerPreset> GetEqualizerPresets()
    {
        ThrowIfHandleInvalid();
        int count = NativeMethods.GetEqualizerPresetsCount(_handle);
        if (count <= 0)
        {
            return Array.Empty<EqualizerPreset>();
        }

        var presets = new EqualizerPreset[count];
        for (int i = 0; i < count; i++)
        {
            string name = NativeStringMarshaller.PtrToStringOrEmpty(
                NativeMethods.GetEqualizerPresetName(_handle, i));
            int bandCount = NativeMethods.GetEqualizerPresetBandCount(_handle, i);
            float[] bands = bandCount > 0 ? new float[bandCount] : Array.Empty<float>();

            if (bandCount > 0)
            {
                unsafe
                {
                    fixed (float* p = bands)
                    {
                        HscResult result = NativeMethods.GetEqualizerPresetBands(_handle, i, p, bandCount);
                        ResultMapping.ThrowIfError(result, "GetEqualizerPresets");
                    }
                }
            }

            presets[i] = new EqualizerPreset((byte)i, name, bands);
        }

        return presets;
    }

    /// <summary>Sets the microphone input volume (0..128).</summary>
    public void SetMicrophoneVolume(byte volume)
    {
        ThrowIfHandleInvalid();
        if (volume > 128)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), volume, "Microphone volume must be in 0..128.");
        }

        HscResult result = NativeMethods.SetMicVolume(_handle, volume);
        ResultMapping.ThrowIfError(result, "SetMicrophoneVolume", HeadsetCapability.MicrophoneVolume);
    }

    /// <summary>Sets the microphone-mute LED brightness (0..3).</summary>
    public void SetMicrophoneMuteLedBrightness(byte brightness)
    {
        ThrowIfHandleInvalid();
        if (brightness > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(brightness), brightness, "Brightness must be in 0..3.");
        }

        HscResult result = NativeMethods.SetMicMuteLedBrightness(_handle, brightness);
        ResultMapping.ThrowIfError(result, "SetMicrophoneMuteLedBrightness", HeadsetCapability.MicrophoneMuteLedBrightness);
    }

    public void SetRotateToMute(bool enabled)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetRotateToMute(_handle, enabled);
        ResultMapping.ThrowIfError(result, "SetRotateToMute", HeadsetCapability.RotateToMute);
    }

    public void SetLights(bool enabled)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetLights(_handle, enabled);
        ResultMapping.ThrowIfError(result, "SetLights", HeadsetCapability.Lights);
    }

    public void SetVoicePrompts(bool enabled)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetVoicePrompts(_handle, enabled);
        ResultMapping.ThrowIfError(result, "SetVoicePrompts", HeadsetCapability.VoicePrompts);
    }

    public void PlayNotificationSound(byte soundId)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.PlayNotificationSound(_handle, soundId);
        ResultMapping.ThrowIfError(result, "PlayNotificationSound", HeadsetCapability.NotificationSound);
    }

    /// <summary>Configures the auto-power-off timer in minutes (0 disables it).</summary>
    public InactiveTimeResult SetInactiveTime(byte minutes)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetInactiveTime(_handle, minutes, out HscInactiveTime raw);
        ResultMapping.ThrowIfError(result, "SetInactiveTime", HeadsetCapability.InactiveTime);
        return new InactiveTimeResult(raw.Minutes, raw.MinMinutes, raw.MaxMinutes);
    }

    public void SetBluetoothWhenPoweredOn(bool enabled)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetBluetoothWhenPoweredOn(_handle, enabled);
        ResultMapping.ThrowIfError(result, "SetBluetoothWhenPoweredOn", HeadsetCapability.BluetoothWhenPoweredOn);
    }

    public void SetBluetoothCallVolume(byte volume)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetBluetoothCallVolume(_handle, volume);
        ResultMapping.ThrowIfError(result, "SetBluetoothCallVolume", HeadsetCapability.BluetoothCallVolume);
    }

    public override string ToString() => $"{Name} (VID=0x{VendorId:X4}, PID=0x{ProductId:X4})";

    private void ThrowIfHandleInvalid()
    {
        if (_owner.IsDisposed)
        {
            throw new ObjectDisposedException(
                nameof(Headset),
                "The owning HeadsetCollection has been disposed; this handle is no longer valid.");
        }
    }
}
