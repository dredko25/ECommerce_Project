using ECommerce_Project.Api.DTOs.User;
using FluentAssertions;
using IntegrationTests.Abstractions;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net.Http.Json;

namespace IntegrationTests.Users;

public class UsersControllerTests : BaseFunctionalTest
{
    public UsersControllerTests(FunctionalTestWebAppFactory factory) : base(factory)
    {

    }
    public async Task<(string Token, Guid Id)> CreateUserAndGetTokenAsync(string email = "test_update@gmail.com", string password = "Password123!")
    {

        var registerRequest = new
        {
            FirstName = "Alice",
            LastName = "Wonderland",
            ContactNumber = "0987654321",
            Email = email,
            Password = password
        };
        await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        
        var loginRequest = new
        {
            Email = email,
            Password = password
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        
        var loginData = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        var token = loginData!.AccessToken;
        var id = loginData!.User.Id;

        return (token, id);
    }

    [Fact]
    public async Task Create_CreateUser_ReturnsCreated()
    {
        // Arrange
        var request = new
        {
            FirstName = "John",
            LastName = "Smith",
            ContactNumber = "1234567890",
            Email = "testCreateUser@gmail.com",
            Password = "password"
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/api/users/register", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_CreateUserWithExistingEmail_ReturnsConflict()
    {
        // Arrange
        _ = await CreateUserAndGetTokenAsync();
        var request = new
        {
            FirstName = "John",
            LastName = "Smith",
            ContactNumber = "1234567890",
            Email = "test_update@gmail.com",
            Password = "Password123!"
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/api/users/register", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAll_GetAllUsers_ReturnsOk()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/users", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_GetUserWithValidData_ReturnsOk()
    {
        // Arrange
        var (_, userId) = await CreateUserAndGetTokenAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/users/{userId}", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_GetUserWithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/users/{nonExistentId}", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_UpdateUserWithValidData_ReturnsOk()
    {
        // Arrange
        var (_, userId) = await CreateUserAndGetTokenAsync();

        var request = new { FirstName = "UpdatedName" };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"/api/users/{userId}", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_UpdateUserWithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new { FirstName = "UpdatedName" };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"/api/users/{nonExistentId}", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_LoginUserWithValidData_ReturnsOk()
    {
        // Arrange
        _ = await CreateUserAndGetTokenAsync();
        var request = new { Email = "test_update@gmail.com", Password = "Password123!" };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/api/users/login", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_LoginUserWithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        _ = await CreateUserAndGetTokenAsync();
        var request = new { Email = "test_update@gmail.com", Password = "password123!" };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/api/users/login", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_LoginUserWithWrongLogin_ReturnsUnauthorized()
    {
        // Arrange
        _ = await CreateUserAndGetTokenAsync();
        var request = new { Email = "", Password = "password123!" };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/api/users/login", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_DeleteUserWithValidId_ReturnsNoContent()
    {
        // Arrange
        var (_, userId) = await CreateUserAndGetTokenAsync();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/users/{userId}", cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        var getResponse = await HttpClient.GetAsync($"/api/users/{userId}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_DeleteUserWithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/users/{nonExistentId}", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Logout_LogoutUserWithValidToken_ReturnsOk()
    {
        // Arrange
        var (token, _) = await CreateUserAndGetTokenAsync();

        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/users/logout", null, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

}