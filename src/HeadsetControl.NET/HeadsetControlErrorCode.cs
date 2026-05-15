namespace HeadsetControl.NET;

public enum HeadsetControlErrorCode
{
    Ok = 0,
    Error = -1,
    NotSupported = -2,
    DeviceOffline = -3,
    Timeout = -4,
    HidError = -5,
    InvalidParameter = -6,
}
