# HeadsetControl.NET

.NET 10 wrapper for the [HeadsetControl](https://github.com/Sapd/HeadsetControl)
C/C++ library. Sidetone, battery, equalizer, lights, chat-mix, the lot.

Runs on `osx-arm64`, `linux-arm64`, `linux-x64`, `win-x64`.

## Layout

```
src/HeadsetControl.NET.Native/   P/Invoke layer (internal)
src/HeadsetControl.NET/          public managed API
tests/HeadsetControl.NET.Tests/  xUnit, uses the built-in test device
samples/HeadsetControl.NET.Sample/
build/build-native.sh            CMake driver per RID
headsetcontrollib/               git submodule with the native source
```

## Build

```bash
git submodule update --init --recursive
build/build-native.sh              # host RID — or: --rid linux-x64
dotnet test -c Release
```

Tests that need the native library `Skip` themselves if it wasn't built, so
`dotnet test` stays green even before step one.

Prerequisites for the native build: CMake, a C++20 compiler, `hidapi`
(`brew install hidapi` / `apt install libhidapi-dev`); on Windows the
upstream library uses vcpkg manifest mode.

## Usage

```csharp
using HeadsetControl.NET;

using HeadsetCollection headsets = HeadsetControlLibrary.Discover();
foreach (Headset headset in headsets)
{
    Console.WriteLine($"{headset}  caps={string.Join(",", headset.SupportedCapabilities)}");

    if (headset.Supports(HeadsetCapability.BatteryStatus))
    {
        BatteryInfo battery = headset.GetBattery();
        Console.WriteLine($"  battery: {battery.Status} ({battery.LevelPercent}%)");
    }

    if (headset.Supports(HeadsetCapability.Sidetone))
    {
        headset.SetSidetone(64);
    }
}
```

Run the sample against the synthetic test device:

```bash
dotnet run --project samples/HeadsetControl.NET.Sample -- --test
```

## Errors

| Exception                                  | Native code           |
|--------------------------------------------|-----------------------|
| `FeatureNotSupportedException`             | `HSC_RESULT_NOT_SUPPORTED` |
| `DeviceOfflineException`                   | `HSC_RESULT_DEVICE_OFFLINE` |
| `DeviceTimeoutException`                   | `HSC_RESULT_TIMEOUT` |
| `HidCommunicationException`                | `HSC_RESULT_HID_ERROR` |
| `HeadsetControlInvalidParameterException`  | `HSC_RESULT_INVALID_PARAM` |
| `HeadsetControlException` (base)           | any other failure |

## License

GPL-3.0-only — see [LICENSE](LICENSE). Matches upstream. This is copyleft, so
anything that links against `HeadsetControl.NET` (statically or dynamically)
inherits GPL-3.0. The bundled Windows `hidapi.dll` is BSD-3-Clause.
