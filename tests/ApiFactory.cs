using BookQuotes.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookQuotes.Tests;

// Each test owns an isolated in-memory relational database. Never touches Neon.
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string SigningKey = "integration-tests-only-signing-key-64-characters-long-not-a-secret";
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=unused");
        builder.UseSetting("Jwt:Key", SigningKey);
        builder.UseSetting("Database:AutoMigrate", "false");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            connection.Open();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) connection.Dispose();
    }
}
