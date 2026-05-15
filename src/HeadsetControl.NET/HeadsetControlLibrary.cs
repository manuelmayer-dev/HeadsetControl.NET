using HeadsetControl.NET.Internal;
using HeadsetControl.NET.Native;

namespace HeadsetControl.NET;

/// <summary>
/// Entry point for the HeadsetControl wrapper.
/// </summary>
/// <remarks>
/// <para>
/// Provides device discovery and configuration of process-wide settings such
/// as the HID timeout and the synthetic test device used in unit tests.
/// </para>
/// <para>
/// Calling any member of this class is sufficient to trigger native library
/// resolution. If loading the native library fails, a <see cref="DllNotFoundException"/>
/// is raised by the runtime — see the project README for build / packaging
/// requirements.
/// </para>
/// </remarks>
public static class HeadsetControlLibrary
{
    static HeadsetControlLibrary()
    {
        NativeLibraryLoader.EnsureInitialized();
    }

    /// <summary>The version string reported by the native library.</summary>
    public static string Version
    {
        get
        {
            string? version = NativeStringMarshaller.PtrToString(NativeMethods.Version());
            return version ?? "0.0.0";
        }
    }

    /// <summary>
    /// HID read timeout used by the native library for all device operations.
    /// </summary>
    public static TimeSpan DeviceTimeout
    {
        get => TimeSpan.FromMilliseconds(NativeMethods.GetDeviceTimeout());
        set
        {
            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Timeout must be non-negative.");
            }

            double ms = value.TotalMilliseconds;
            int clamped = ms >= int.MaxValue ? int.MaxValue : (int)ms;
            NativeMethods.SetDeviceTimeout(clamped);
        }
    }

    /// <summary>
    /// Whether the synthetic HeadsetControl Test device is enabled. When set
    /// to <see langword="true"/>, <see cref="Discover"/> will return a virtual
    /// headset even without any physical hardware connected — useful for
    /// integration tests on CI.
    /// </summary>
    public static bool TestDeviceEnabled
    {
        get => NativeMethods.IsTestDeviceEnabled();
        set => NativeMethods.EnableTestDevice(value);
    }

    /// <summary>
    /// Profile number for the synthetic test device. See the native library
    /// documentation for the supported values.
    /// </summary>
    public static int TestProfile
    {
        get => NativeMethods.GetTestProfile();
        set => NativeMethods.SetTestProfile(value);
    }

    /// <summary>Number of headset models the native library supports.</summary>
    public static int SupportedDeviceCount => NativeMethods.SupportedDeviceCount();

    /// <summary>Names of every headset model the native library supports.</summary>
    public static IReadOnlyList<string> SupportedDeviceNames
    {
        get
        {
            int count = NativeMethods.SupportedDeviceCount();
            if (count <= 0)
            {
                return Array.Empty<string>();
            }

            var names = new string[count];
            for (int i = 0; i < count; i++)
            {
                names[i] = NativeStringMarshaller.PtrToStringOrEmpty(NativeMethods.SupportedDeviceName(i));
            }

            return names;
        }
    }

    /// <summary>
    /// Discovers every connected headset and returns them as a disposable
    /// collection.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="HeadsetCollection"/> owns the native handle
    /// array; dispose it (or wrap in <c>using</c>) once the contained
    /// <see cref="Headset"/> instances are no longer needed.
    /// </remarks>
    /// <exception cref="HeadsetControlException">
    /// Thrown when the native library reports a discovery error.
    /// </exception>
    public static HeadsetCollection Discover()
    {
        int count = NativeMethods.Discover(out IntPtr nativeArray);

        if (count < 0)
        {
            ResultMapping.ThrowIfError((HscResult)count, "Discover");
        }

        return new HeadsetCollection(nativeArray, count);
    }
}
