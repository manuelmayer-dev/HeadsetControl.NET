using System.Runtime.InteropServices;

namespace HeadsetControl.NET.Native;

/// <summary>
/// Helpers for decoding C strings returned from the native library.
/// </summary>
/// <remarks>
/// Strings returned by <c>hsc_*</c> getters are pointers into native-owned
/// memory and must not be freed by managed code. We copy their contents
/// into managed strings on the boundary so the rest of the codebase deals
/// only in <see cref="string"/>.
/// </remarks>
internal static class NativeStringMarshaller
{
    /// <summary>
    /// Decodes a null-terminated UTF-8 string from native memory, returning
    /// <see langword="null"/> if the pointer is null.
    /// </summary>
    public static string? PtrToString(IntPtr ptr)
    {
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }

    /// <summary>
    /// Decodes a null-terminated UTF-8 string from native memory, returning
    /// an empty string if the pointer is null.
    /// </summary>
    public static string PtrToStringOrEmpty(IntPtr ptr)
    {
        return PtrToString(ptr) ?? string.Empty;
    }
}
