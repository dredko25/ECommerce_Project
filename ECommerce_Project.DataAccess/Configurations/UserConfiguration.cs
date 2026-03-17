using ECommerce_Project.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce_Project.DataAccess.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(u => u.LastName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(u => u.ContactNumber)
               .IsRequired()
               .HasMaxLength(20);

        builder.HasIndex(u => u.ContactNumber)
               .IsUnique();

        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(u => u.Email)
               .IsUnique();

        builder.Property(u => u.PasswordHash)
               .HasMaxLength(200);

        builder.HasOne(u => u.Cart)
               .WithOne(c => c.User)
               .HasForeignKey<UserEntity>(u => u.CartId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Cascade);

    }
}