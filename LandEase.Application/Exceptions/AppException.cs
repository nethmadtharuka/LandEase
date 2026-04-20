namespace LandEase.Application.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(
        string message,
        int statusCode,
        string errorCode,
        IReadOnlyList<string>? errors = null,
        Exception? innerException = null) : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors;
    }

    public int StatusCode { get; }
    public string ErrorCode { get; }
    public IReadOnlyList<string>? Errors { get; }
}

