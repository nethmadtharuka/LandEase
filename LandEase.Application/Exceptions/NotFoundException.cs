namespace LandEase.Application.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message, Exception? innerException = null)
        : base(message, statusCode: 404, errorCode: "not_found", innerException: innerException)
    {
    }
}

