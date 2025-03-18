using System.Reflection;
using DomainEssentials.Core.Abstractions;
using Engine.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data.Contexts;

public class AppDbContext(DbContextOptions<AppDbContext> options) : AppDbContextBase(options, "dbo")
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.UseGlobalQueryFilterFor<IAggregateRoot>(x => !x.IsDeleted);
    }
}

public class AppDbContextFactory : DbContextFactory<AppDbContext>
{
    public override AppDbContext CreateContext(DbContextOptions<AppDbContext> options)
    {
        return new AppDbContext(options);
    }
}