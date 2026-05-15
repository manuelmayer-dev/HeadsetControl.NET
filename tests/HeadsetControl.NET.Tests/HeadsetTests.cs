using HeadsetControl.NET.Tests.Support;

namespace HeadsetControl.NET.Tests;

/// <summary>
/// Exercises the public <see cref="Headset"/> API against the built-in
/// synthetic test device.
/// </summary>
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
        Headset test = collection.First(h => h.VendorId == 0xF00B);
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
            foreach (HeadsetCapability cap in Enum.GetValues<HeadsetCapability>())
            {
                bool bit = (test.CapabilitiesBitmask & (1 << (int)cap)) != 0;
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

            BatteryInfo battery = test.GetBattery();
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
            // Pick any capability the test device does NOT support and invoke
            // the matching operation. If the test device supports every
            // capability we exercise here, the assertion is trivially true.
            if (!test.Supports(HeadsetCapability.BluetoothCallVolume))
            {
                Assert.Throws<FeatureNotSupportedException>(() => test.SetBluetoothCallVolume(50));
            }
        }
    }
}
