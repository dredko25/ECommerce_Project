using ECommerce_Project.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce_Project.DataAccess.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<CartEntity>
{
    public void Configure(EntityTypeBuilder<CartEntity> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.User)
               .WithOne(u => u.Cart)
               .HasForeignKey<CartEntity>(c => c.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.CreatedAt)
               .IsRequired();

        builder.HasMany(c => c.CartItems)
               .WithOne(ci => ci.Cart)
               .HasForeignKey(ci => ci.CartId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

    }
}