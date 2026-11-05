using LearningCrud.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningCrud.Web.Data;

public sealed class LearningDbContext(DbContextOptions<LearningDbContext> options)
    : DbContext(options)
{
    public DbSet<LearningItem> LearningItems => Set<LearningItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LearningItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Title).HasMaxLength(120).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(2_000);
            entity.Property(item => item.CreatedAtUtc).IsRequired();
        });
    }
}
