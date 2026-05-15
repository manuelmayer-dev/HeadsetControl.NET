using HeadsetControl.NET;

bool useTestDevice = args.Contains("--test", StringComparer.OrdinalIgnoreCase);

Console.WriteLine($"HeadsetControl native version: {HeadsetControlLibrary.Version}");
Console.WriteLine($"Supported device models: {HeadsetControlLibrary.SupportedDeviceCount}");
Console.WriteLine();

if (useTestDevice)
{
    HeadsetControlLibrary.TestDeviceEnabled = true;
    Console.WriteLine("Synthetic test device: ENABLED");
    Console.WriteLine();
}

using HeadsetCollection headsets = HeadsetControlLibrary.Discover();

if (headsets.Count == 0)
{
    Console.WriteLine("No supported headset connected.");
    return 0;
}

for (int i = 0; i < headsets.Count; i++)
{
    Headset headset = headsets[i];

    Console.WriteLine($"[{i}] {headset}");
    Console.WriteLine($"     Vendor : {headset.VendorName ?? "(unknown)"} (0x{headset.VendorId:X4})");
    Console.WriteLine($"     Product: {headset.ProductName ?? "(unknown)"} (0x{headset.ProductId:X4})");
    Console.WriteLine($"     Capabilities: {string.Join(", ", headset.SupportedCapabilities)}");

    TryReadBattery(headset);
    TryReadChatMix(headset);

    Console.WriteLine();
}

return 0;

static void TryReadBattery(Headset headset)
{
    if (!headset.Supports(HeadsetCapability.BatteryStatus))
    {
        return;
    }

    try
    {
        BatteryInfo battery = headset.GetBattery();
        string level = battery.LevelPercent is int pct ? $"{pct}%" : "n/a";
        Console.WriteLine($"     Battery: {battery.Status} ({level})");
    }
    catch (HeadsetControlException ex)
    {
        Console.WriteLine($"     Battery: <error: {ex.Message}>");
    }
}

static void TryReadChatMix(Headset headset)
{
    if (!headset.Supports(HeadsetCapability.ChatMixStatus))
    {
        return;
    }

    try
    {
        ChatMixInfo mix = headset.GetChatMix();
        Console.WriteLine($"     ChatMix: level={mix.Level} game={mix.GameVolumePercent}% chat={mix.ChatVolumePercent}%");
    }
    catch (HeadsetControlException ex)
    {
        Console.WriteLine($"     ChatMix: <error: {ex.Message}>");
    }
}
