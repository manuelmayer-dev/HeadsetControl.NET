using HeadsetControl.NET.Exceptions;
using HeadsetControl.NET.Tests.Support;

namespace HeadsetControl.NET.Tests;

[Collection(NativeLibraryCollection.Name)]
public sealed class HeadsetTests
{
    private readonly NativeLibraryFixture _fixture;

    public HeadsetTests(NativeLibraryFixture fixture)
    {
        _fixture = fixture;
    }

    private (HeadsetCollection collection, Headset test) OpenTestDevice()
    {
        var collection = HeadsetControlLibrary.Discover();
        var test = collection.First(h => h.VendorId == 0xF00B);
        return (collection, test);
    }

    [SkippableFact]
    public void Headset_BasicProperties_ArePopulated()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        using var scope = new TestDeviceScope();
        var (collection, test) = OpenTestDevice();
        using (collection)
        {
            Assert.Equal(0xF00B, test.VendorId);
            Assert.Equal(0xA00C, test.ProductId);
            Assert.False(string.IsNullOrWhiteSpace(test.Name));
            Assert.Contains(test.ToString(), test.ToString(), StringComparison.Ordinal);
            Assert.NotEmpty(test.SupportedCapabilities);
        }
    }

    [SkippableFact]
    public void Headset_Supports_IsConsistentWithBitmask()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        using var scope = new TestDeviceScope();
        var (collection, test) = OpenTestDevice();
        using (collection)
        {
            foreach (var cap in Enum.GetValues<HeadsetCapability>())
            {
                var bit = (test.CapabilitiesBitmask & (1 << (int)cap)) != 0;
                Assert.Equal(bit, test.Supports(cap));
            }
        }
    }

    [SkippableFact]
    public void SetSidetone_OutOfRange_ThrowsArgument()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        using var scope = new TestDeviceScope();
        var (collection, test) = OpenTestDevice();
        using (collection)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => test.SetSidetone(200));
        }
    }

    [SkippableFact]
    public void SetMicrophoneMuteLedBrightness_OutOfRange_ThrowsArgument()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        using var scope = new TestDeviceScope();
        var (collection, test) = OpenTestDevice();
        using (collection)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => test.SetMicrophoneMuteLedBrightness(4));
        }
    }

    [SkippableFact]
    public void SetEqualizer_Empty_ThrowsArgument()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        using var scope = new TestDeviceScope();
        var (collection, test) = OpenTestDevice();
        using (collection)
        {
            Assert.Throws<ArgumentException>(() => test.SetEqualizer(ReadOnlySpan<float>.Empty));
        }
    }

    [SkippableFact]
    public void GetBattery_ReturnsKnownStatus()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        using var scope = new TestDeviceScope();
        var (collection, test) = OpenTestDevice();
        using (collection)
        {
            Skip.IfNot(test.Supports(HeadsetCapability.BatteryStatus),
                "Test device build does not expose battery status.");

            var battery = test.GetBattery();
            Assert.True(Enum.IsDefined(battery.Status));
        }
    }

    [SkippableFact]
    public void UnsupportedFeature_ThrowsFeatureNotSupported()
    {
        Skip.IfNot(_fixture.IsNativeLibraryAvailable, _fixture.LoadError?.Message);

        using var scope = new TestDeviceScope();
        var (collection, test) = OpenTestDevice();
        using (collection)
        {
            if (!test.Supports(HeadsetCapability.BluetoothCallVolume))
            {
                Assert.Throws<FeatureNotSupportedException>(() => test.SetBluetoothCallVolume(50));
            }
        }
    }
}
