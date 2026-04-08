using AutoMapper;
using ECommerce_Project.Api.DTOs.Cart;
using ECommerce_Project.Api.Services;
using ECommerce_Project.DataAccess;
using ECommerce_Project.DataAccess.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce_Project.UnitTests;


public class TokenServiceTests
{
    private ECommerceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ECommerceDbContext(options);
    }

    private Mock<IConfiguration> GetMockConfiguration()
    {
        var mockConfig = new Mock<IConfiguration>();

        mockConfig.Setup(c => c["Jwt:Key"]).Returns("TKIuyrJLoJEzCULkezWNozTGavgCQKtSwfDS5Q9K3uF72zIl126RKvuqIeXW!TKIuyrJLoJEzCU");
        mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        return mockConfig;
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ReturnsTokenString()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockConfig = GetMockConfiguration();
        var mockLogger = new Mock<ILogger<TokenService>>();

        var tokenService = new TokenService(mockConfig.Object, context, mockLogger.Object);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "test@gmail.com",
            FirstName = "Test",
            LastName = "Testovych",
            IsAdmin = false
        };

        // Act
        var token = tokenService.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Length.Should().Be(3);
    }

    [Fact]
    public void GenerateAccessToken_WhenKeyIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockConfig = new Mock<IConfiguration>();
        string? key = null;

        mockConfig.Setup(c => c["Jwt:Key"]).Returns(key);

        var mockLogger = new Mock<ILogger<TokenService>>();
        var tokenService = new TokenService(mockConfig.Object, context, mockLogger.Object);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "test@gmail.com",
            FirstName = "Test",
            LastName = "Testovych",
            IsAdmin = false
        };

        // Act
        Action act = () => tokenService.GenerateAccessToken(user);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("JWT Key is not configured");
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithExpiredToken_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var mockConfig = GetMockConfiguration();
        var mockLogger = new Mock<ILogger<TokenService>>();

        var tokenService = new TokenService(mockConfig.Object, context, mockLogger.Object);

        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@gmail.com",
            RefreshToken = "oldToken",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await tokenService.ValidateRefreshTokenAsync(userId, "oldToken");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithValidToken_ReturnsUser()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var mockConfig = GetMockConfiguration();
        var mockLogger = new Mock<ILogger<TokenService>>();

        var tokenService = new TokenService(mockConfig.Object, context, mockLogger.Object);

        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@gmail.com",
            RefreshToken = "RefreshToken",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await tokenService.ValidateRefreshTokenAsync(userId, "RefreshToken");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithWrongToken_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var mockConfig = GetMockConfiguration();
        var mockLogger = new Mock<ILogger<TokenService>>();

        var tokenService = new TokenService(mockConfig.Object, context, mockLogger.Object);

        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@gmail.com",
            RefreshToken = "RefreshToken",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await tokenService.ValidateRefreshTokenAsync(userId, "hackerToken");

        // Assert
        result.Should().BeNull();
    }
}