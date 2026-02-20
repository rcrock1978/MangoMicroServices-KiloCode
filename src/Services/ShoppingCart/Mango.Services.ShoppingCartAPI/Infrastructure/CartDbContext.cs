using Microsoft.EntityFrameworkCore;
using Mango.Services.ShoppingCartAPI.Domain;

namespace Mango.Services.ShoppingCartAPI.Infrastructure;

public class CartDbContext : DbContext
{
    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options)
    {
    }

    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cart>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.HasMany(e => e.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
        });
    }
}
