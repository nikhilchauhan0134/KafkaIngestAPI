using DataCaptureApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DataCaptureApi.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var item = modelBuilder.Entity<Item>();
        item.HasKey(entity => entity.Id);
        item.Property(entity => entity.Id).ValueGeneratedOnAdd();
        item.Property(entity => entity.Name).IsRequired().HasMaxLength(200);
        item.Property(entity => entity.Description).IsRequired().HasMaxLength(4000);
    }
}
