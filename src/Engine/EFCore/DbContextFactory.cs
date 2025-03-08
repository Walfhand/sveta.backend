using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Engine.EFCore;

public abstract class DbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    public TContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder
            .UseNpgsql("",
                dbOptions => { dbOptions.MigrationsAssembly(typeof(TContext).Assembly.GetName().Name); })
            .UseSnakeCaseNamingConvention();

        return CreateContext(optionsBuilder.Options);
    }

    public abstract TContext CreateContext(DbContextOptions<TContext> options);
}