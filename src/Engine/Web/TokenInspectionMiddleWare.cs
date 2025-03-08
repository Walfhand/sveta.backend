using Microsoft.AspNetCore.Http;

namespace Engine.Web;

public class TokenInspectionMiddleware
{
    private readonly RequestDelegate _next;

    public TokenInspectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.ContainsKey("Authorization"))
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            Console.WriteLine($"Authorization Header: {authHeader}");
        }
        else
        {
            Console.WriteLine("No Authorization Header Found");
        }

        await _next(context);
    }
}