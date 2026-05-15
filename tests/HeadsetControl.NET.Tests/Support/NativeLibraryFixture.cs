namespace HeadsetControl.NET.Tests.Support;

/// <summary>
/// xUnit collection fixture that decides whether tests requiring a loaded
/// native library can run on this host.
/// </summary>
/// <remarks>
/// The native <c>libheadsetcontrol</c> is produced by an external CMake build
/// and is not guaranteed to be present on every developer machine. Tests in
/// <see cref="NativeLibraryCollection"/> that depend on it are gated by
/// <see cref="IsNativeLibraryAvailable"/> and <see cref="Skip"/>-ped otherwise,
/// so the test project still runs cleanly without the native artefact.
/// </remarks>
public sealed class NativeLibraryFixture
{
    public NativeLibraryFixture()
    {
        try
        {
            // Trigger the static ctor on HeadsetControlLibrary which registers
            // the DllImport resolver, then probe the library by reading the
            // version string.
            string version = HeadsetControlLibrary.Version;
            IsNativeLibraryAvailable = !string.IsNullOrEmpty(version);
            Version = version;
        }
        catch (DllNotFoundException ex)
        {
            IsNativeLibraryAvailable = false;
            LoadError = ex;
        }
        catch (TypeInitializationException ex) when (ex.InnerException is DllNotFoundException dnf)
        {
            IsNativeLibraryAvailable = false;
            LoadError = dnf;
        }
    }

    /// <summary>Whether the native library was loaded successfully.</summary>
    public bool IsNativeLibraryAvailable { get; }

    /// <summary>The native library version, populated on successful load.</summary>
    public string? Version { get; }

    /// <summary>Exception that prevented the native library from loading.</summary>
    public Exception? LoadError { get; }
}

/// <summary>
/// Marker collection used by tests that depend on the native library being
/// loadable. Sharing the fixture prevents repeated load attempts.
/// </summary>
[CollectionDefinition(Name)]
public sealed class NativeLibraryCollection : ICollectionFixture<NativeLibraryFixture>
{
    public const string Name = "NativeLibrary";
}
