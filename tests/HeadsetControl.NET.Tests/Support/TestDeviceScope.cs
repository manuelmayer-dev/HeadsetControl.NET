namespace HeadsetControl.NET.Tests.Support;

/// <summary>
/// Enables the synthetic HeadsetControl Test device for the lifetime of the
/// scope and restores the previous state on disposal. Keeps tests from
/// leaking global library state into each other.
/// </summary>
sealed class TestDeviceScope : IDisposable
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
