using System.Runtime.InteropServices;

namespace HeadsetControl.NET.Native;

// Const-char* returns are marshalled as IntPtr and decoded by NativeStringMarshaller
// so the source generator doesn't free memory the native library still owns.
internal static partial class NativeMethods
{
    private const string LibName = NativeLibraryLoader.LibraryName;

    [LibraryImport(LibName, EntryPoint = "hsc_discover")]
    internal static partial int Discover(out IntPtr headsets);

    [LibraryImport(LibName, EntryPoint = "hsc_free_headsets")]
    internal static partial void FreeHeadsets(IntPtr headsets, int count);

    [LibraryImport(LibName, EntryPoint = "hsc_version")]
    internal static partial IntPtr Version();

    [LibraryImport(LibName, EntryPoint = "hsc_supported_device_count")]
    internal static partial int SupportedDeviceCount();

    [LibraryImport(LibName, EntryPoint = "hsc_supported_device_name")]
    internal static partial IntPtr SupportedDeviceName(int index);

    [LibraryImport(LibName, EntryPoint = "hsc_get_name")]
    internal static partial IntPtr GetName(IntPtr headset);

    [LibraryImport(LibName, EntryPoint = "hsc_get_vendor_id")]
    internal static partial ushort GetVendorId(IntPtr headset);

    [LibraryImport(LibName, EntryPoint = "hsc_get_vendor_name")]
    internal static partial IntPtr GetVendorName(IntPtr headset);

    [LibraryImport(LibName, EntryPoint = "hsc_get_product_id")]
    internal static partial ushort GetProductId(IntPtr headset);

    [LibraryImport(LibName, EntryPoint = "hsc_get_product_name")]
    internal static partial IntPtr GetProductName(IntPtr headset);

    [LibraryImport(LibName, EntryPoint = "hsc_supports")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool Supports(IntPtr headset, HscCapability cap);

    [LibraryImport(LibName, EntryPoint = "hsc_get_capabilities")]
    internal static partial int GetCapabilities(IntPtr headset);

    [LibraryImport(LibName, EntryPoint = "hsc_get_battery")]
    internal static partial HscResult GetBattery(IntPtr headset, out HscBattery battery);

    [LibraryImport(LibName, EntryPoint = "hsc_get_chatmix")]
    internal static partial HscResult GetChatMix(IntPtr headset, out HscChatMix chatmix);

    [LibraryImport(LibName, EntryPoint = "hsc_set_sidetone")]
    internal static partial HscResult SetSidetone(IntPtr headset, byte level, out HscSidetone result);

    [LibraryImport(LibName, EntryPoint = "hsc_set_volume_limiter")]
    internal static partial HscResult SetVolumeLimiter(IntPtr headset, [MarshalAs(UnmanagedType.U1)] bool enabled);

    [LibraryImport(LibName, EntryPoint = "hsc_set_equalizer_preset")]
    internal static partial HscResult SetEqualizerPreset(IntPtr headset, byte preset);

    [LibraryImport(LibName, EntryPoint = "hsc_set_equalizer")]
    internal static unsafe partial HscResult SetEqualizer(IntPtr headset, float* bands, int numBands);

    [LibraryImport(LibName, EntryPoint = "hsc_get_equalizer_presets_count")]
    internal static partial int GetEqualizerPresetsCount(IntPtr headset);

    [LibraryImport(LibName, EntryPoint = "hsc_get_equalizer_preset_name")]
    internal static partial IntPtr GetEqualizerPresetName(IntPtr headset, int preset);

    [LibraryImport(LibName, EntryPoint = "hsc_get_equalizer_preset_band_count")]
    internal static partial int GetEqualizerPresetBandCount(IntPtr headset, int preset);

    [LibraryImport(LibName, EntryPoint = "hsc_get_equalizer_preset_bands")]
    internal static unsafe partial HscResult GetEqualizerPresetBands(IntPtr headset, int preset, float* bands, int numBands);

    [LibraryImport(LibName, EntryPoint = "hsc_set_mic_volume")]
    internal static partial HscResult SetMicVolume(IntPtr headset, byte volume);

    [LibraryImport(LibName, EntryPoint = "hsc_set_mic_mute_led_brightness")]
    internal static partial HscResult SetMicMuteLedBrightness(IntPtr headset, byte brightness);

    [LibraryImport(LibName, EntryPoint = "hsc_set_rotate_to_mute")]
    internal static partial HscResult SetRotateToMute(IntPtr headset, [MarshalAs(UnmanagedType.U1)] bool enabled);

    [LibraryImport(LibName, EntryPoint = "hsc_set_lights")]
    internal static partial HscResult SetLights(IntPtr headset, [MarshalAs(UnmanagedType.U1)] bool enabled);

    [LibraryImport(LibName, EntryPoint = "hsc_set_voice_prompts")]
    internal static partial HscResult SetVoicePrompts(IntPtr headset, [MarshalAs(UnmanagedType.U1)] bool enabled);

    [LibraryImport(LibName, EntryPoint = "hsc_play_notification_sound")]
    internal static partial HscResult PlayNotificationSound(IntPtr headset, byte soundId);

    [LibraryImport(LibName, EntryPoint = "hsc_set_inactive_time")]
    internal static partial HscResult SetInactiveTime(IntPtr headset, byte minutes, out HscInactiveTime result);

    [LibraryImport(LibName, EntryPoint = "hsc_set_bluetooth_when_powered_on")]
    internal static partial HscResult SetBluetoothWhenPoweredOn(IntPtr headset, [MarshalAs(UnmanagedType.U1)] bool enabled);

    [LibraryImport(LibName, EntryPoint = "hsc_set_bluetooth_call_volume")]
    internal static partial HscResult SetBluetoothCallVolume(IntPtr headset, byte volume);

    [LibraryImport(LibName, EntryPoint = "hsc_enable_test_device")]
    internal static partial void EnableTestDevice([MarshalAs(UnmanagedType.U1)] bool enabled);

    [LibraryImport(LibName, EntryPoint = "hsc_is_test_device_enabled")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool IsTestDeviceEnabled();

    [LibraryImport(LibName, EntryPoint = "hsc_set_device_timeout")]
    internal static partial void SetDeviceTimeout(int timeoutMs);

    [LibraryImport(LibName, EntryPoint = "hsc_get_device_timeout")]
    internal static partial int GetDeviceTimeout();

    [LibraryImport(LibName, EntryPoint = "hsc_set_test_profile")]
    internal static partial void SetTestProfile(int profile);

    [LibraryImport(LibName, EntryPoint = "hsc_get_test_profile")]
    internal static partial int GetTestProfile();
}
