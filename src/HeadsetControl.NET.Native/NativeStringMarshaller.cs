using System.Runtime.InteropServices;

namespace HeadsetControl.NET.Native;

internal static class NativeStringMarshaller
{
    public static string? PtrToString(IntPtr ptr)
        => ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);

    public static string PtrToStringOrEmpty(IntPtr ptr)
        => PtrToString(ptr) ?? string.Empty;
}
