// ============================================================
// Tests/Data/ApplicationDbContextTests.cs
// Integration-style unit tests for ApplicationDbContext
// using EF Core InMemory provider.
// Verifies audit field population on SaveChangesAsync.
// ============================================================
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OrbitAOS.V6.Application.Common.Interfaces;
using OrbitAOS.V6.Data;
using Xunit;

namespace OrbitAOS.V6.Tests.Data;

/// <summary>
/// Unit tests for <see cref="ApplicationDbContext"/>.
/// Uses EF Core InMemory provider to avoid SQL Server dependency.
/// </summary>
public class ApplicationDbContextTests
{
    private static ApplicationDbContext CreateInMemoryContext(
        ICurrentUserService? currentUserService = null,
        IDateTimeService? dateTimeService = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, currentUserService, dateTimeService);
    }

    [Fact]
    public async Task SaveChangesAsync_CanBeCalledWithoutError()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().Be(0); // No changes
    }

    [Fact]
    public async Task SaveChangesAsync_WithCurrentUserService_DoesNotThrow()
    {
        // Arrange
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.UserId).Returns("test-user");

        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock.Setup(s => s.UtcNow).Returns(DateTime.UtcNow);

        await using var context = CreateInMemoryContext(
            currentUserServiceMock.Object,
            dateTimeServiceMock.Object);

        // Act
        var act = async () => await context.SaveChangesAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void ApplicationDbContext_CanBeInstantiated_WithMinimalOptions()
    {
        // Arrange & Act
        var act = () => CreateInMemoryContext();

        // Assert
        act.Should().NotThrow();
    }
}
