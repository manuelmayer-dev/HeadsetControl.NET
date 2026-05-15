using HeadsetControl.NET.Tests.Support;

namespace HeadsetControl.NET.Tests;

[Collection(NativeLibraryCollection.Name)]
public sealed class HeadsetControlLibraryTests
{
    private readonly NativeLibraryFixture _fixture;

    public HeadsetControlLibraryTests(NativeLibraryFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public void Version_IsNonEmpty()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        string version = HeadsetControlLibrary.Version;
        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    [SkippableFact]
    public void DeviceTimeout_RoundTrips()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        TimeSpan original = HeadsetControlLibrary.DeviceTimeout;
        try
        {
            HeadsetControlLibrary.DeviceTimeout = TimeSpan.FromMilliseconds(1234);
            Assert.Equal(TimeSpan.FromMilliseconds(1234), HeadsetControlLibrary.DeviceTimeout);
        }
        finally
        {
            HeadsetControlLibrary.DeviceTimeout = original;
        }
    }

    [SkippableFact]
    public void DeviceTimeout_Negative_Throws()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => HeadsetControlLibrary.DeviceTimeout = TimeSpan.FromSeconds(-1));
    }

    [SkippableFact]
    public void SupportedDeviceNames_AreReadable()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        IReadOnlyList<string> names = HeadsetControlLibrary.SupportedDeviceNames;
        Assert.Equal(HeadsetControlLibrary.SupportedDeviceCount, names.Count);
        Assert.All(names, name => Assert.False(string.IsNullOrWhiteSpace(name)));
    }

    [SkippableFact]
    public void Discover_WithTestDevice_FindsTheTestHeadset()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        using var scope = new TestDeviceScope();
        using HeadsetCollection headsets = HeadsetControlLibrary.Discover();

        Assert.NotEmpty(headsets);
        Headset test = headsets.Single(h => h.VendorId == 0xF00B && h.ProductId == 0xA00C);
        Assert.False(string.IsNullOrWhiteSpace(test.Name));
    }

    [SkippableFact]
    public void Discover_DisposedCollection_DisablesHeadsets()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        using var scope = new TestDeviceScope();
        HeadsetCollection headsets = HeadsetControlLibrary.Discover();
        Headset test = headsets.First(h => h.VendorId == 0xF00B);

        headsets.Dispose();

        Assert.True(headsets.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => test.Supports(HeadsetCapability.BatteryStatus));
    }
}
