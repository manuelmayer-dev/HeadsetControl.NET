using System.Reflection;
using System.Runtime.InteropServices;

namespace HeadsetControl.NET.Native;

/// <summary>
/// Resolves the native HeadsetControl shared library across platforms.
/// </summary>
/// <remarks>
/// <para>
/// .NET's default <c>DllImport</c> resolution does not always pick the correct
/// per-RID binary when the host is not running as a published app (for example
/// in unit tests). This loader registers a <see cref="DllImportResolver"/> that
/// looks first in the standard <c>runtimes/&lt;rid&gt;/native/</c> layout next to
/// the executing assembly, then falls back to the OS loader so platform-installed
/// copies of <c>libheadsetcontrol</c> can be used too.
/// </para>
/// <para>
/// Initialization is idempotent: subsequent calls to <see cref="EnsureInitialized"/>
/// are cheap and thread-safe.
/// </para>
/// </remarks>
internal static class NativeLibraryLoader
{
    /// <summary>The library name used in <see cref="LibraryImportAttribute"/>.</summary>
    public const string LibraryName = "headsetcontrol";

    private static readonly Lock InitLock = new();
    private static bool _initialized;

    /// <summary>
    /// Ensures the import resolver is registered before any P/Invoke call into
    /// the native library is made.
    /// </summary>
    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        lock (InitLock)
        {
            if (_initialized)
            {
                return;
            }

            NativeLibrary.SetDllImportResolver(
                typeof(NativeLibraryLoader).Assembly,
                Resolve);

            _initialized = true;
        }
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, LibraryName, StringComparison.Ordinal))
        {
            return IntPtr.Zero;
        }

        // Set HEADSETCONTROL_NATIVE_TRACE=1 to print probe paths to stderr —
        // useful for diagnosing missing-binary failures on CI hosts.
        bool trace = string.Equals(
            Environment.GetEnvironmentVariable("HEADSETCONTROL_NATIVE_TRACE"),
            "1",
            StringComparison.Ordinal);

        foreach (string candidate in EnumerateCandidatePaths())
        {
            if (trace)
            {
                Console.Error.WriteLine($"[HeadsetControl.NET] probe: {candidate}");
            }
            if (NativeLibrary.TryLoad(candidate, out IntPtr handle))
            {
                return handle;
            }
        }

        // Fall back to the OS loader (LD_LIBRARY_PATH, DYLD_LIBRARY_PATH,
        // %PATH%, or an installed system library).
        return NativeLibrary.TryLoad(libraryName, assembly, searchPath, out IntPtr fallback)
            ? fallback
            : IntPtr.Zero;
    }

    private static IEnumerable<string> EnumerateCandidatePaths()
    {
        string rid = GetRuntimeIdentifier();
        string fileName = GetNativeFileName();

        // 1. AppContext.BaseDirectory — correct for normal executables and
        //    published self-contained apps.
        string? baseDir = AppContext.BaseDirectory;
        if (!string.IsNullOrEmpty(baseDir))
        {
            yield return Path.Combine(baseDir, "runtimes", rid, "native", fileName);
            yield return Path.Combine(baseDir, fileName);
        }

        // 2. Directory of the Interop assembly itself — required under
        //    `dotnet test` where BaseDirectory points at the testhost.
        //    Skipped under single-file publish (Location returns empty).
        string? assemblyDir = GetAssemblyDirectory();
        if (!string.IsNullOrEmpty(assemblyDir) &&
            !string.Equals(assemblyDir, baseDir, StringComparison.Ordinal))
        {
            yield return Path.Combine(assemblyDir, "runtimes", rid, "native", fileName);
            yield return Path.Combine(assemblyDir, fileName);
        }
    }

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(
        "SingleFile",
        "IL3000:Avoid accessing Assembly file path when publishing as a single file",
        Justification = "Empty Location is checked before use; the BaseDirectory probe " +
                         "always runs first and covers the single-file case.")]
    private static string? GetAssemblyDirectory()
    {
        string location = typeof(NativeLibraryLoader).Assembly.Location;
        return string.IsNullOrEmpty(location) ? null : Path.GetDirectoryName(location);
    }

    private static string GetRuntimeIdentifier()
    {
        string osPart;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            osPart = "osx";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            osPart = "linux";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            osPart = "win";
        }
        else
        {
            osPart = "unknown";
        }

        string archPart = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            _ => "unknown",
        };

        return $"{osPart}-{archPart}";
    }

    private static string GetNativeFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "headsetcontrol.dll";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "libheadsetcontrol.dylib";
        }

        return "libheadsetcontrol.so";
    }
}
