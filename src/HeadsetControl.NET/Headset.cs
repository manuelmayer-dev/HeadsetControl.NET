using HeadsetControl.NET.Internal;
using HeadsetControl.NET.Native;

namespace HeadsetControl.NET;

/// <summary>
/// A discovered USB headset.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Headset"/> instances are produced by
/// <see cref="HeadsetControlLibrary.Discover"/>; they are owned by the returned
/// <see cref="HeadsetCollection"/> and become invalid once that collection is
/// disposed.
/// </para>
/// <para>
/// Cheaply-cached properties (<see cref="Name"/>, <see cref="VendorId"/>, etc.)
/// are snapshotted at discovery time and remain readable after disposal.
/// Operations that touch the device (such as <see cref="GetBattery"/> and
/// <see cref="SetSidetone"/>) require an active native handle and throw
/// <see cref="ObjectDisposedException"/> on a disposed collection.
/// </para>
/// <para>
/// Headset operations are not thread-safe with respect to a single device:
/// the native library serializes HID I/O internally, but concurrent calls on
/// the same headset can deadlock or interleave responses.
/// </para>
/// </remarks>
public sealed class Headset
{
    private readonly HeadsetCollection _owner;
    private readonly IntPtr _handle;

    /// <summary>Human-readable device name (e.g. "SteelSeries Arctis 7").</summary>
    public string Name { get; }

    /// <summary>USB vendor identifier.</summary>
    public ushort VendorId { get; }

    /// <summary>USB vendor name as reported by HID, or <see langword="null"/> if unavailable.</summary>
    public string? VendorName { get; }

    /// <summary>USB product identifier.</summary>
    public ushort ProductId { get; }

    /// <summary>USB product name as reported by HID, or <see langword="null"/> if unavailable.</summary>
    public string? ProductName { get; }

    /// <summary>Bitmask of supported capabilities (one bit per <see cref="HeadsetCapability"/>).</summary>
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

    /// <summary>The enumerable set of capabilities supported by this headset.</summary>
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

    /// <summary>Checks whether this headset supports the given <paramref name="capability"/>.</summary>
    public bool Supports(HeadsetCapability capability)
    {
        ThrowIfHandleInvalid();
        return NativeMethods.Supports(_handle, (HscCapability)(int)capability);
    }

    // ========================================================================
    // Battery & Status
    // ========================================================================

    /// <summary>Reads the current battery state from the device.</summary>
    /// <exception cref="FeatureNotSupportedException">Battery reporting is not supported.</exception>
    /// <exception cref="DeviceOfflineException">The headset is offline.</exception>
    /// <exception cref="DeviceTimeoutException">The query timed out.</exception>
    /// <exception cref="HidCommunicationException">A HID-level error occurred.</exception>
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

    /// <summary>Reads the current chat-mix dial position.</summary>
    public ChatMixInfo GetChatMix()
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.GetChatMix(_handle, out HscChatMix raw);
        ResultMapping.ThrowIfError(result, "GetChatMix", HeadsetCapability.ChatMixStatus);
        return new ChatMixInfo(raw.Level, raw.GameVolumePercent, raw.ChatVolumePercent);
    }

    // ========================================================================
    // Audio
    // ========================================================================

    /// <summary>Sets the sidetone level.</summary>
    /// <param name="level">Sidetone level in the range 0..128 (0 = off).</param>
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

    /// <summary>Enables or disables the output volume limiter.</summary>
    public void SetVolumeLimiter(bool enabled)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetVolumeLimiter(_handle, enabled);
        ResultMapping.ThrowIfError(result, "SetVolumeLimiter", HeadsetCapability.VolumeLimiter);
    }

    // ========================================================================
    // Equalizer
    // ========================================================================

    /// <summary>Activates a built-in equalizer preset.</summary>
    public void SetEqualizerPreset(byte preset)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetEqualizerPreset(_handle, preset);
        ResultMapping.ThrowIfError(result, "SetEqualizerPreset", HeadsetCapability.EqualizerPreset);
    }

    /// <summary>Applies a custom equalizer curve.</summary>
    /// <param name="bands">Band values; their count and meaning are device-specific.</param>
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

    /// <summary>Lists every built-in equalizer preset and its band values.</summary>
    /// <remarks>Returns an empty list when the device does not expose any presets.</remarks>
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

    // ========================================================================
    // Microphone
    // ========================================================================

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

    /// <summary>Toggles the rotate-to-mute microphone feature.</summary>
    public void SetRotateToMute(bool enabled)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetRotateToMute(_handle, enabled);
        ResultMapping.ThrowIfError(result, "SetRotateToMute", HeadsetCapability.RotateToMute);
    }

    // ========================================================================
    // Lights & Cues
    // ========================================================================

    /// <summary>Turns the headset LEDs on or off.</summary>
    public void SetLights(bool enabled)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetLights(_handle, enabled);
        ResultMapping.ThrowIfError(result, "SetLights", HeadsetCapability.Lights);
    }

    /// <summary>Enables or disables the spoken voice prompts.</summary>
    public void SetVoicePrompts(bool enabled)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetVoicePrompts(_handle, enabled);
        ResultMapping.ThrowIfError(result, "SetVoicePrompts", HeadsetCapability.VoicePrompts);
    }

    /// <summary>Plays one of the device's notification sounds.</summary>
    public void PlayNotificationSound(byte soundId)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.PlayNotificationSound(_handle, soundId);
        ResultMapping.ThrowIfError(result, "PlayNotificationSound", HeadsetCapability.NotificationSound);
    }

    // ========================================================================
    // Power & Bluetooth
    // ========================================================================

    /// <summary>Configures the auto-power-off timer.</summary>
    /// <param name="minutes">Idle timeout in minutes; pass 0 to disable.</param>
    public InactiveTimeResult SetInactiveTime(byte minutes)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetInactiveTime(_handle, minutes, out HscInactiveTime raw);
        ResultMapping.ThrowIfError(result, "SetInactiveTime", HeadsetCapability.InactiveTime);
        return new InactiveTimeResult(raw.Minutes, raw.MinMinutes, raw.MaxMinutes);
    }

    /// <summary>Configures whether Bluetooth turns on with the headset.</summary>
    public void SetBluetoothWhenPoweredOn(bool enabled)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetBluetoothWhenPoweredOn(_handle, enabled);
        ResultMapping.ThrowIfError(result, "SetBluetoothWhenPoweredOn", HeadsetCapability.BluetoothWhenPoweredOn);
    }

    /// <summary>Sets the Bluetooth call volume.</summary>
    public void SetBluetoothCallVolume(byte volume)
    {
        ThrowIfHandleInvalid();
        HscResult result = NativeMethods.SetBluetoothCallVolume(_handle, volume);
        ResultMapping.ThrowIfError(result, "SetBluetoothCallVolume", HeadsetCapability.BluetoothCallVolume);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name} (VID=0x{VendorId:X4}, PID=0x{ProductId:X4})";
    }

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
