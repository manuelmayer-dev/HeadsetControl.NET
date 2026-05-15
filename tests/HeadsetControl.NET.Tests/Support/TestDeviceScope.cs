namespace HeadsetControl.NET.Tests.Support;

internal sealed class TestDeviceScope : IDisposable
{
    private readonly bool _previousEnabled;
    private readonly int _previousProfile;

    public TestDeviceScope(int profile = 0)
    {
        _previousEnabled = HeadsetControlLibrary.TestDeviceEnabled;
        _previousProfile = HeadsetControlLibrary.TestProfile;
        HeadsetControlLibrary.TestProfile = profile;
        HeadsetControlLibrary.TestDeviceEnabled = true;
    }

    public void Dispose()
    {
        HeadsetControlLibrary.TestDeviceEnabled = _previousEnabled;
        HeadsetControlLibrary.TestProfile = _previousProfile;
    }
}
