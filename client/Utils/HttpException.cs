using System.Net;

namespace client.Utils;

public class HttpException : Exception
{
    public HttpStatusCode Code { get; }

    public HttpException(string? message, HttpStatusCode code) : base(message)
    {
        Code = code;
    }
}

