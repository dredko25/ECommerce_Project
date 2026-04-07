using AutoMapper;
using ECommerce_Project.Api.DTOs.Cart;
using ECommerce_Project.DataAccess;
using ECommerce_Project.DataAccess.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce_Project.UnitTests;


public class CartServiceTests
{
    [Fact]
    public async Task AddItemAsync_WhenQuantityExceedsStock_ThrowsInvalidOperationException()
    {
        // 1. Arrange
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ECommerceDbContext(options);

        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var product = new ProductEntity
        {
            Id = productId,
            Name = "Test Item",
            QuantityAvailable = 5
        };

        context.Products.Add(product);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var mockLogger = new Mock<ILogger<CartService>>();
        var mockMapper = new Mock<IMapper>();

        var cartService = new CartService(context, mockMapper.Object, mockLogger.Object);

        var addToCartDto = new AddToCartDto
        {
            ProductId = productId,
            Quantity = 10
        };

        // 2. Act
        Func<Task> act = async () => await cartService.AddItemAsync(userId, addToCartDto);

        // 3. Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("На складі доступно лише 5 шт.");
    }
}