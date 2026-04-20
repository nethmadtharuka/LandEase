namespace LandEase.Application.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message, Exception? innerException = null)
        : base(message, statusCode: 403, errorCode: "forbidden", innerException: innerException)
    {
    }
}

