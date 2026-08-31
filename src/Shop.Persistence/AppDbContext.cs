using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shop.Domain.Abstractions;
using Shop.Domain.RefreshTokens;
using Shop.Domain.Roles;
using Shop.Domain.Users;
using Shop.Persistence.Exceptions;

namespace Shop.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        => SaveChangesAsync(cancellationToken);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" } postgresException)
        {
            throw new DuplicateKeyException(postgresException.ConstraintName, ex);
        }
    }
}