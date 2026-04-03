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
               .WithMany()
               .HasForeignKey(oi => oi.ProductId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(oi => oi.Price)
               .IsRequired()
               .HasPrecision(18, 2);

        builder.Property(oi => oi.Quantity)
               .IsRequired();

        builder.Ignore(oi => oi.Total);
    }
}