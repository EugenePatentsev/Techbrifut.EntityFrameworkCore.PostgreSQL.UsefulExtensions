using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

namespace ExampleApp;

public sealed class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    public static AppDbContext Create()
    {
        string connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = "localhost",
            Port = 5432,
            Username = "postgres",
            Password = "postgres",
            Database = "test_db"
        }.ConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder()
            .LogTo(Console.WriteLine, LogLevel.Warning)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .UseNpgsql(connectionString)
            .UseUsefulExtensions();

        var options = optionsBuilder.Options;

        return new AppDbContext(options);
    }
}