namespace HeadsetControl.NET;

/// <summary>
/// Base type for all exceptions raised by HeadsetControl.NET.
/// </summary>
/// <remarks>
/// Specific failure modes are represented by derived types
/// (<see cref="DeviceOfflineException"/>, <see cref="DeviceTimeoutException"/>,
/// <see cref="HidCommunicationException"/>, <see cref="FeatureNotSupportedException"/>,
/// <see cref="HeadsetControlInvalidParameterException"/>). Catch the base class
/// to handle any failure from this library uniformly; catch a derived class
/// to react to a specific failure mode.
/// </remarks>
public class HeadsetControlException : Exception
{
    /// <summary>The underlying native error code.</summary>
    public HeadsetControlErrorCode ErrorCode { get; }

    /// <inheritdoc />
    public HeadsetControlException()
        : this(HeadsetControlErrorCode.Error, "An unspecified HeadsetControl error occurred.")
    {
    }

    /// <inheritdoc />
    public HeadsetControlException(string message)
        : this(HeadsetControlErrorCode.Error, message)
    {
    }

    /// <inheritdoc />
    public HeadsetControlException(string message, Exception innerException)
        : this(HeadsetControlErrorCode.Error, message, innerException)
    {
    }

    /// <summary>Initializes a new instance with an explicit error code.</summary>
    public HeadsetControlException(HeadsetControlErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>Initializes a new instance with an explicit error code.</summary>
    public HeadsetControlException(HeadsetControlErrorCode errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>Raised when the headset is currently offline (powered off, out of range, etc.).</summary>
public sealed class DeviceOfflineException : HeadsetControlException
{
    /// <inheritdoc />
    public DeviceOfflineException()
        : base(HeadsetControlErrorCode.DeviceOffline, "The headset is offline.")
    {
    }

    /// <inheritdoc />
    public DeviceOfflineException(string message)
        : base(HeadsetControlErrorCode.DeviceOffline, message)
    {
    }

    /// <inheritdoc />
    public DeviceOfflineException(string message, Exception innerException)
        : base(HeadsetControlErrorCode.DeviceOffline, message, innerException)
    {
    }
}

/// <summary>Raised when a HID read operation times out.</summary>
public sealed class DeviceTimeoutException : HeadsetControlException
{
    /// <inheritdoc />
    public DeviceTimeoutException()
        : base(HeadsetControlErrorCode.Timeout, "The headset did not respond in time.")
    {
    }

    /// <inheritdoc />
    public DeviceTimeoutException(string message)
        : base(HeadsetControlErrorCode.Timeout, message)
    {
    }

    /// <inheritdoc />
    public DeviceTimeoutException(string message, Exception innerException)
        : base(HeadsetControlErrorCode.Timeout, message, innerException)
    {
    }
}

/// <summary>Raised when underlying HID communication fails.</summary>
public sealed class HidCommunicationException : HeadsetControlException
{
    /// <inheritdoc />
    public HidCommunicationException()
        : base(HeadsetControlErrorCode.HidError, "HID communication with the headset failed.")
    {
    }

    /// <inheritdoc />
    public HidCommunicationException(string message)
        : base(HeadsetControlErrorCode.HidError, message)
    {
    }

    /// <inheritdoc />
    public HidCommunicationException(string message, Exception innerException)
        : base(HeadsetControlErrorCode.HidError, message, innerException)
    {
    }
}

/// <summary>
/// Raised when a requested feature is not supported by the connected headset.
/// </summary>
public sealed class FeatureNotSupportedException : HeadsetControlException
{
    /// <summary>The capability that was attempted.</summary>
    public HeadsetCapability? Capability { get; }

    /// <inheritdoc />
    public FeatureNotSupportedException()
        : base(HeadsetControlErrorCode.NotSupported, "The requested feature is not supported by this headset.")
    {
    }

    /// <inheritdoc />
    public FeatureNotSupportedException(string message)
        : base(HeadsetControlErrorCode.NotSupported, message)
    {
    }

    /// <inheritdoc />
    public FeatureNotSupportedException(string message, Exception innerException)
        : base(HeadsetControlErrorCode.NotSupported, message, innerException)
    {
    }

    /// <summary>Initializes a new instance with the offending capability.</summary>
    public FeatureNotSupportedException(HeadsetCapability capability)
        : base(
            HeadsetControlErrorCode.NotSupported,
            $"The headset does not support the '{capability}' capability.")
    {
        Capability = capability;
    }
}

/// <summary>
/// Raised when an argument fails the native library's validation.
/// </summary>
/// <remarks>
/// Most parameter checks are done in managed code and surface as
/// <see cref="ArgumentException"/> / <see cref="ArgumentOutOfRangeException"/>.
/// This type is reserved for native-side rejections.
/// </remarks>
public sealed class HeadsetControlInvalidParameterException : HeadsetControlException
{
    /// <inheritdoc />
    public HeadsetControlInvalidParameterException()
        : base(HeadsetControlErrorCode.InvalidParameter, "The native library rejected a parameter.")
    {
    }

    /// <inheritdoc />
    public HeadsetControlInvalidParameterException(string message)
        : base(HeadsetControlErrorCode.InvalidParameter, message)
    {
    }

    /// <inheritdoc />
    public HeadsetControlInvalidParameterException(string message, Exception innerException)
        : base(HeadsetControlErrorCode.InvalidParameter, message, innerException)
    {
    }
}
