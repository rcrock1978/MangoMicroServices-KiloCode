using Microsoft.EntityFrameworkCore;
using Mango.Services.RewardAPI.Domain;

namespace Mango.Services.RewardAPI.Infrastructure;

public class RewardDbContext : DbContext
{
    public RewardDbContext(DbContextOptions<RewardDbContext> options) : base(options) { }
    public DbSet<Reward> Rewards { get; set; } = null!;
    public DbSet<RewardTransaction> RewardTransactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Reward>(entity =>
        {
            entity.ToTable("Rewards");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
            entity.HasMany(e => e.Transactions).WithOne(t => t.Reward).HasForeignKey(t => t.RewardId);
        });
        builder.Entity<RewardTransaction>(entity =>
        {
            entity.ToTable("RewardTransactions");
            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(250);
        });
    }
}
