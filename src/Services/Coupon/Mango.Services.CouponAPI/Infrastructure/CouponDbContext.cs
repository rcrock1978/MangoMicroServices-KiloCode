using Microsoft.EntityFrameworkCore;
using Mango.Services.CouponAPI.Domain;

namespace Mango.Services.CouponAPI.Infrastructure;

public class CouponDbContext : DbContext
{
    public CouponDbContext(DbContextOptions<CouponDbContext> options) : base(options) { }
    public DbSet<Coupon> Coupons { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Coupon>(entity =>
        {
            entity.ToTable("Coupons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.MinAmount).HasPrecision(18, 2);
            entity.HasIndex(e => e.Code).IsUnique();
        });
    }
}
