namespace HeadsetControl.NET.Tests.Support;

// Probes the native library on construction so dependent tests can Skip
// when it's not buildable on the current host.
public sealed class NativeLibraryFixture
{
    public NativeLibraryFixture()
    {
        try
        {
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

    public bool IsNativeLibraryAvailable { get; }
    public string? Version { get; }
    public Exception? LoadError { get; }
}

[CollectionDefinition(Name)]
public sealed class NativeLibraryCollection : ICollectionFixture<NativeLibraryFixture>
{
    public const string Name = "NativeLibrary";
}
