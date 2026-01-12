using FoodServerClient.Models;
using Microsoft.EntityFrameworkCore;

namespace SmsConsoleApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Dish> Dishes => Set<Dish>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dish>(entity =>
        {
            entity.ToTable("menu_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Article)
                .HasColumnName("article")
                .IsRequired();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired();

            entity.Property(x => x.Price)
                .HasColumnName("price");

            entity.Property(x => x.IsWeighted)
                .HasColumnName("is_weighted");

            entity.Property(x => x.FullPath)
                .HasColumnName("full_path");
        });
    }
}
