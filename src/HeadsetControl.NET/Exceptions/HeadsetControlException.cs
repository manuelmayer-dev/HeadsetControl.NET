namespace HeadsetControl.NET.Exceptions;

public class HeadsetControlException : Exception
{
    public HeadsetControlErrorCode ErrorCode { get; }

    public HeadsetControlException()
        : this(HeadsetControlErrorCode.Error, "An unspecified HeadsetControl error occurred.")
    {
    }

    public HeadsetControlException(string message)
        : this(HeadsetControlErrorCode.Error, message)
    {
    }

    public HeadsetControlException(string message, Exception innerException)
        : this(HeadsetControlErrorCode.Error, message, innerException)
    {
    }

    public HeadsetControlException(HeadsetControlErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public HeadsetControlException(HeadsetControlErrorCode errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public sealed class DeviceOfflineException : HeadsetControlException
{
    public DeviceOfflineException()
        : base(HeadsetControlErrorCode.DeviceOffline, "The headset is offline.")
    {
    }

    public DeviceOfflineException(string message)
        : base(HeadsetControlErrorCode.DeviceOffline, message)
    {
    }

    public DeviceOfflineException(string message, Exception innerException)
        : base(HeadsetControlErrorCode.DeviceOffline, message, innerException)
    {
    }
}

public sealed class DeviceTimeoutException : HeadsetControlException
{
    public DeviceTimeoutException()
        : base(HeadsetControlErrorCode.Timeout, "The headset did not respond in time.")
    {
    }

    public DeviceTimeoutException(string message)
        : base(HeadsetControlErrorCode.Timeout, message)
    {
    }

    public DeviceTimeoutException(string message, Exception innerException)
        : base(HeadsetControlErrorCode.Timeout, message, innerException)
    {
    }
}

public sealed class HidCommunicationException : HeadsetControlException
{
    public HidCommunicationException()
        : base(HeadsetControlErrorCode.HidError, "HID communication with the headset failed.")
    {
    }

    public HidCommunicationException(string message)
        : base(HeadsetControlErrorCode.HidError, message)
    {
    }

    public HidCommunicationException(string message, Exception innerException)
        : base(HeadsetControlErrorCode.HidError, message, innerException)
    {
    }
}

public sealed class FeatureNotSupportedException : HeadsetControlException
{
    public HeadsetCapability? Capability { get; }

    public FeatureNotSupportedException()
        : base(HeadsetControlErrorCode.NotSupported, "The requested feature is not supported by this headset.")
    {
    }

    public FeatureNotSupportedException(string message)
        : base(HeadsetControlErrorCode.NotSupported, message)
    {
    }

    public FeatureNotSupportedException(string message, Exception innerException)
        : base(HeadsetControlErrorCode.NotSupported, message, innerException)
    {
    }

    public FeatureNotSupportedException(HeadsetCapability capability)
        : base(
            HeadsetControlErrorCode.NotSupported,
            $"The headset does not support the '{capability}' capability.")
    {
        Capability = capability;
    }
}

public sealed class HeadsetControlInvalidParameterException : HeadsetControlException
{
    public HeadsetControlInvalidParameterException()
        : base(HeadsetControlErrorCode.InvalidParameter, "The native library rejected a parameter.")
    {
    }

    public HeadsetControlInvalidParameterException(string message)
        : base(HeadsetControlErrorCode.InvalidParameter, message)
    {
    }

    public HeadsetControlInvalidParameterException(string message, Exception innerException)
        : base(HeadsetControlErrorCode.InvalidParameter, message, innerException)
    {
    }
}
