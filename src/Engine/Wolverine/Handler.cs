using System.Security.Claims;
using Engine.EFCore;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Http;

namespace Engine.Wolverine;

public abstract class Handler
{
    protected readonly IDbContext DbContext;

    protected readonly HttpContext HttpContext;
    protected readonly Guid UserId;

    protected Handler(IAppDbContextFactory dbContextFactory, IHttpContextAccessor contextAccessor)
    {
        HttpContext = contextAccessor.HttpContext!;
        DbContext = dbContextFactory.CreateDbContext();
        var aud = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        UserId = string.IsNullOrWhiteSpace(aud) ? Guid.Empty : Guid.Parse(aud);
    }
}