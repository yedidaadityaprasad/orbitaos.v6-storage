// ============================================================
// Tests/Infrastructure/Services/CurrentUserServiceTests.cs
// Unit tests for CurrentUserService.
// Verifies that the service correctly reads claims from
// the ASP.NET Core HTTP context.
// ============================================================
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using OrbitAOS.V6.Infrastructure.Services;
using System.Security.Claims;
using Xunit;

namespace OrbitAOS.V6.Tests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="CurrentUserService"/>.
/// </summary>
public class CurrentUserServiceTests
{
    // -------------------------------------------------------
    // UserId tests
    // -------------------------------------------------------

    [Fact]
    public void UserId_ReturnsNameIdentifierClaim_WhenAuthenticated()
    {
        // Arrange
        const string expectedUserId = "user-123";
        var httpContextAccessor = CreateHttpContextAccessorWithClaims(
            new Claim(ClaimTypes.NameIdentifier, expectedUserId),
            new Claim(ClaimTypes.Name, "testuser"));

        var service = new CurrentUserService(httpContextAccessor);

        // Act
        var userId = service.UserId;

        // Assert
        userId.Should().Be(expectedUserId);
    }

    [Fact]
    public void UserId_ReturnsNull_WhenNotAuthenticated()
    {
        // Arrange
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(httpContextAccessor.Object);

        // Act
        var userId = service.UserId;

        // Assert
        userId.Should().BeNull();
    }

    // -------------------------------------------------------
    // UserName tests
    // -------------------------------------------------------

    [Fact]
    public void UserName_ReturnsNameClaim_WhenAuthenticated()
    {
        // Arrange
        const string expectedUserName = "john.doe@example.com";
        var httpContextAccessor = CreateHttpContextAccessorWithClaims(
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, expectedUserName));

        var service = new CurrentUserService(httpContextAccessor);

        // Act
        var userName = service.UserName;

        // Assert
        userName.Should().Be(expectedUserName);
    }

    // -------------------------------------------------------
    // IsAuthenticated tests
    // -------------------------------------------------------

    [Fact]
    public void IsAuthenticated_ReturnsTrue_WhenUserIsAuthenticated()
    {
        // Arrange
        var httpContextAccessor = CreateHttpContextAccessorWithClaims(
            new Claim(ClaimTypes.NameIdentifier, "user-123"));

        var service = new CurrentUserService(httpContextAccessor);

        // Act
        var isAuthenticated = service.IsAuthenticated;

        // Assert
        isAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_ReturnsFalse_WhenHttpContextIsNull()
    {
        // Arrange
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(httpContextAccessor.Object);

        // Act
        var isAuthenticated = service.IsAuthenticated;

        // Assert
        isAuthenticated.Should().BeFalse();
    }

    // -------------------------------------------------------
    // Helper methods
    // -------------------------------------------------------

    private static IHttpContextAccessor CreateHttpContextAccessorWithClaims(
        params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        return httpContextAccessor.Object;
    }
}
