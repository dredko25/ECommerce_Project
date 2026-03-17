using ECommerce_Project.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce_Project.DataAccess.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemEntity>
{
    public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.HasOne(oi => oi.Order)
               .WithMany(o => o.OrderItems)
               .HasForeignKey(oi => oi.OrderId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Product)
               .WithMany(p => p.OrderItems)
               .HasForeignKey(oi => oi.ProductId)
               .IsRequired()
               .OnDelete(DeleteBehavior.SetNull);

        builder.Property(oi => oi.Price)
               .IsRequired()
               .HasPrecision(18, 2);

        builder.Property(oi => oi.Quantity)
               .IsRequired();

        builder.Property(oi => oi.Total)
               .HasPrecision(18, 2);

        builder.Ignore(oi => oi.Total);
    }
}