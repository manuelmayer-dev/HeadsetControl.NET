# HeadsetControl.NET

Cross-platform .NET 10 wrapper for the [HeadsetControl](https://github.com/Sapd/HeadsetControl)
native C/C++ library. Provides an idiomatic, fully-managed API for controlling
USB headsets (sidetone, battery, equalizer, lights, chat-mix, etc.).

Supported runtimes:

- `osx-arm64`
- `linux-arm64`, `linux-x64`
- `win-x64`

## Solution layout

```
HeadsetControl.NET/
├── Directory.Build.props        # solution-wide MSBuild settings
├── Directory.Packages.props     # central package management
├── headsetcontrollib/           # git submodule: native HeadsetControl source
├── build/
│   └── build-native.sh          # CMake driver that produces runtimes/<rid>/native/*
├── src/
│   ├── HeadsetControl.NET.Native/   # P/Invoke layer (LibraryImport, internal)
│   └── HeadsetControl.NET/          # public managed API
├── tests/
│   └── HeadsetControl.NET.Tests/    # xUnit, uses the built-in test device
└── samples/
    └── HeadsetControl.NET.Sample/   # console app showing API usage
```

The native interop layer is an internal implementation detail. Consumers
should depend on `HeadsetControl.NET` only.

## Building

### 1. Build the native library

The .NET projects look for the native shared library under
`build/native/<rid>/`. The convenience script wraps CMake:

```bash
git submodule update --init --recursive
build/build-native.sh                # detects host RID
build/build-native.sh --rid linux-x64
```

CMake prerequisites (Linux example): `cmake`, `hidapi`, a C++20-capable
toolchain. See the upstream `headsetcontrollib/README.md` for full requirements.

### 2. Build the .NET solution

```bash
dotnet restore
dotnet build -c Release
dotnet test  -c Release
```

The test project gates native-dependent tests on whether the shared library
loaded, so `dotnet test` still passes if step 1 has not been completed —
those tests are reported as `Skipped`.

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
        SidetoneResult result = headset.SetSidetone(64);
        Console.WriteLine($"  sidetone now: {result.CurrentLevel}");
    }
}
```

Run the sample against the synthetic test device (no hardware required):

```bash
dotnet run --project samples/HeadsetControl.NET.Sample -- --test
```

## Error handling

Native error codes never leak through the public API. Operations either
succeed or throw a typed exception:

| Exception                                  | Native code           |
|--------------------------------------------|-----------------------|
| `FeatureNotSupportedException`             | `HSC_RESULT_NOT_SUPPORTED` |
| `DeviceOfflineException`                   | `HSC_RESULT_DEVICE_OFFLINE` |
| `DeviceTimeoutException`                   | `HSC_RESULT_TIMEOUT` |
| `HidCommunicationException`                | `HSC_RESULT_HID_ERROR` |
| `HeadsetControlInvalidParameterException`  | `HSC_RESULT_INVALID_PARAM` |
| `HeadsetControlException` (base)           | any other failure |

All derive from `HeadsetControlException`; catch the base type to handle any
library failure uniformly.

## Architecture notes

- `HeadsetControl.NET.Native` uses `[LibraryImport]` source-generated marshalling
  so the binding layer is NativeAOT- and trim-compatible.
- `NativeLibraryLoader` registers a `DllImportResolver` that probes the standard
  `runtimes/<rid>/native/` layout before falling back to the OS loader. This
  means the library works under both `dotnet run`, `dotnet test`, and a
  published self-contained app without packaging tweaks.
- `HeadsetCollection` owns the native handle array. Disposing it frees the
  array; subsequent calls on the contained `Headset` instances throw
  `ObjectDisposedException`.
