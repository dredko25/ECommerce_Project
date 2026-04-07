using FluentAssertions;
using IntegrationTests.Abstractions;
using System.Net.Http.Json;

namespace IntegrationTests.Users;

public class CreateUserTest : BaseFunctionalTest
{
    public CreateUserTest(FunctionalTestWebAppFactory factory) : base(factory)
    {

    }

    [Fact]
    public async Task CreateUser_ReturnsCreated()
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

}