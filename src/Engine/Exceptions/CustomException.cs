using System.Net;

namespace Engine.Exceptions;

public abstract class CustomException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}