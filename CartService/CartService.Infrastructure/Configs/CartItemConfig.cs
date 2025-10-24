using CartService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CartService.Infrastructure.Configs;

public class CartItemConfig : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.CartId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(ci => ci.ProductId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(ci => ci.Quantity)
            .HasColumnType("integer")
            .IsRequired();

        builder.Property(ci => ci.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}