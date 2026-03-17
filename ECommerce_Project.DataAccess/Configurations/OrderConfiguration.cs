using ECommerce_Project.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce_Project.DataAccess.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.HasKey(o => o.Id);

        builder.HasOne(o => o.User)
               .WithMany(u => u.Orders)
               .HasForeignKey(o => o.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(o => o.OrderItems)
               .WithOne(oi => oi.Order)
               .HasForeignKey(oi => oi.OrderId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(o => o.OrderNumber)
               .IsRequired()
               .HasMaxLength(20);

        builder.HasIndex(o => o.OrderNumber)
               .IsUnique();

        builder.Property(o => o.OrderDate)
               .IsRequired();

        builder.Property(o => o.Address)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(o => o.PaymentMethod)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(o => o.TotalAmount)
               .IsRequired()
               .HasPrecision(18, 2);


    }
}