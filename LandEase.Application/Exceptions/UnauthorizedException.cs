namespace LandEase.Application.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message, Exception? innerException = null)
        : base(message, statusCode: 401, errorCode: "unauthorized", innerException: innerException)
    {
    }
}

