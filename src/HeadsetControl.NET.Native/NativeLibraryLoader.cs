using System.Reflection;
using System.Runtime.InteropServices;

namespace HeadsetControl.NET.Native;

static class NativeLibraryLoader
{
    public const string LibraryName = "headsetcontrol";

    private static readonly Lazy<bool> Initializer = new(() =>
    {
        NativeLibrary.SetDllImportResolver(
            typeof(NativeLibraryLoader).Assembly,
            Resolve);

        return true;
    });

    public static void EnsureInitialized()
    {
        _ = Initializer.Value;
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, LibraryName, StringComparison.Ordinal))
        {
            return IntPtr.Zero;
        }

        var trace = string.Equals(
            Environment.GetEnvironmentVariable("HEADSETCONTROL_NATIVE_TRACE"),
            "1",
            StringComparison.Ordinal);

        foreach (var candidate in EnumerateCandidatePaths())
        {
            if (trace)
            {
                Console.Error.WriteLine($"[HeadsetControl.NET] probe: {candidate}");
            }
            if (NativeLibrary.TryLoad(candidate, out var handle))
            {
                return handle;
            }
        }

        return NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var fallback)
            ? fallback
            : IntPtr.Zero;
    }

    private static IEnumerable<string> EnumerateCandidatePaths()
    {
        var rid = GetRuntimeIdentifier();
        var fileName = GetNativeFileName();

        var baseDir = AppContext.BaseDirectory;
        if (!string.IsNullOrEmpty(baseDir))
        {
            yield return Path.Combine(baseDir, "runtimes", rid, "native", fileName);
            yield return Path.Combine(baseDir, fileName);
        }

        // Under `dotnet test` BaseDirectory points at the testhost, so also
        // probe the directory of this assembly itself.
        var assemblyDir = GetAssemblyDirectory();
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
        Justification = "Empty Location is checked; the BaseDirectory probe covers single-file.")]
    private static string? GetAssemblyDirectory()
    {
        var location = typeof(NativeLibraryLoader).Assembly.Location;
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

        var archPart = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            _ => "unknown",
        };

        return $"{osPart}-{archPart}";
    }

    private static string GetNativeFileName() =>
        OperatingSystem.IsWindows() ? "headsetcontrol.dll" :
        OperatingSystem.IsMacOS()   ? "libheadsetcontrol.dylib" :
        "libheadsetcontrol.so";
}
