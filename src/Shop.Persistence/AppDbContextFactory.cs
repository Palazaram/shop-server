using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Shop.Persistence;

internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Port=5433;Database=shop;Username=shop;Password=shop_dev_password";

    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        DbContextOptionsConfigurator.Configure(builder, DesignTimeConnectionString);
        return new AppDbContext(builder.Options);
    }
}