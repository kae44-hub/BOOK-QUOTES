using Microsoft.EntityFrameworkCore;
using BookQuotes.Api.Models;
namespace BookQuotes.Api.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<UserSession> Sessions => Set<UserSession>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(u => u.NormalizedUsername).IsUnique();
        modelBuilder.Entity<AppUser>().Property(u => u.Username).HasMaxLength(30);
        modelBuilder.Entity<AppUser>().Property(u => u.NormalizedUsername).HasMaxLength(30);
        modelBuilder.Entity<Quote>().Property(q => q.Text).HasMaxLength(1000);
        modelBuilder.Entity<Quote>().Property(q => q.Author).HasMaxLength(200);
        modelBuilder.Entity<Quote>().HasIndex(q => q.UserId);
        modelBuilder.Entity<UserSession>().HasIndex(s => s.ExpiresAt);
    }
}
