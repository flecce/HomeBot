namespace CommonHelpers.Inverters.Enums
{
    public enum TransmissionState
    {
        Ok,
        NotConnected,
        WriteTimeout,
        ReadTimeout,
        FrameError,
        ChecksumError,
        AddressError,
        WrongAnswer,
        UnknownParameter,
        PasswordProtected,
        MaxValueError,
        MinValueError,
        ParameterIsReadOnly,
        AccessCode1,
        AccessCode2,
        NoCommand,
        WrongValue,
        NoAnswer,
        NoFWFile,
        Error,
        UnknownFile,
        Canceled
    }
}