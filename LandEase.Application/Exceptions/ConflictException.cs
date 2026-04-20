namespace LandEase.Application.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(string message, Exception? innerException = null)
        : base(message, statusCode: 409, errorCode: "conflict", innerException: innerException)
    {
    }
}

