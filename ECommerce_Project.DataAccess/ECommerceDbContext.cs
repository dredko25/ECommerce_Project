using Microsoft.EntityFrameworkCore;
using ECommerce_Project.DataAccess.Models;

namespace ECommerce_Project.DataAccess;

public class ECommerceDbContext : DbContext
{
    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<CategoryEntity> Categories { get; set; } = null!;
    public DbSet<CartEntity> Carts { get; set; } = null!;
    public DbSet<CartItemEntity> CartItems { get; set; } = null!;
    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<OrderItemEntity> OrderItems { get; set; } = null!;

}
