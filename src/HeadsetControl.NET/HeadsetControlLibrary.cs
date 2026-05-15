using HeadsetControl.NET.Internal;
using HeadsetControl.NET.Native;

namespace HeadsetControl.NET;

/// <summary>
/// Entry point: discovery and process-wide configuration of the native library.
/// </summary>
public static class HeadsetControlLibrary
{
    static HeadsetControlLibrary()
    {
        NativeLibraryLoader.EnsureInitialized();
    }

    public static string Version
        => NativeStringMarshaller.PtrToString(NativeMethods.Version()) ?? "0.0.0";

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
    /// When true, <see cref="Discover"/> returns a synthetic test device even
    /// with no physical hardware connected.
    /// </summary>
    public static bool TestDeviceEnabled
    {
        get => NativeMethods.IsTestDeviceEnabled();
        set => NativeMethods.EnableTestDevice(value);
    }

    public static int TestProfile
    {
        get => NativeMethods.GetTestProfile();
        set => NativeMethods.SetTestProfile(value);
    }

    public static int SupportedDeviceCount => NativeMethods.SupportedDeviceCount();

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
