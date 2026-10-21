// ============================================================
// Tests/Controllers/HomeControllerTests.cs
// Unit tests for HomeController using xUnit + Moq.
// Tests verify .NET 10 controller behavior including:
//   - IActionResult return types
//   - IMediator injection
//   - ICurrentUserService injection
//   - Error action with trace identifier
// ============================================================
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Application.Common.Interfaces;
using OrbitAOS.V6.Controllers;
using OrbitAOS.V6.Models;
using Xunit;

namespace OrbitAOS.V6.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="HomeController"/>.
/// Uses Moq to mock all dependencies so tests are isolated
/// from infrastructure (database, HTTP context, etc.).
/// </summary>
public class HomeControllerTests
{
    private readonly Mock<ILogger<HomeController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly HomeController _controller;

    /// <summary>
    /// Initializes test fixtures shared across all test methods.
    /// </summary>
    public HomeControllerTests()
    {
        _loggerMock = new Mock<ILogger<HomeController>>();
        _mediatorMock = new Mock<IMediator>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _controller = new HomeController(
            _loggerMock.Object,
            _mediatorMock.Object,
            _currentUserServiceMock.Object);

        // Set up a default HttpContext for controller tests
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // -------------------------------------------------------
    // Index action tests
    // -------------------------------------------------------

    [Fact]
    public void Index_ReturnsViewResult()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.UserId).Returns("test-user-id");

        // Act
        var result = _controller.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Index_WithAnonymousUser_ReturnsViewResult()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.UserId).Returns((string?)null);

        // Act
        var result = _controller.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    // -------------------------------------------------------
    // Privacy action tests
    // -------------------------------------------------------

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        // Act
        var result = _controller.Privacy();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    // -------------------------------------------------------
    // Error action tests
    // -------------------------------------------------------

    [Fact]
    public void Error_ReturnsViewResult_WithErrorViewModel()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.TraceIdentifier = "test-trace-id";

        // Act
        var result = _controller.Error();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ErrorViewModel>();
    }

    [Fact]
    public void Error_ErrorViewModel_HasRequestId_WhenTraceIdentifierSet()
    {
        // Arrange
        const string expectedTraceId = "test-trace-id-12345";
        _controller.ControllerContext.HttpContext.TraceIdentifier = expectedTraceId;

        // Act
        var result = (ViewResult)_controller.Error();
        var model = (ErrorViewModel)result.Model!;

        // Assert
        model.RequestId.Should().Be(expectedTraceId);
        model.ShowRequestId.Should().BeTrue();
    }

    [Fact]
    public void Error_ErrorViewModel_ShowRequestId_FalseWhenEmpty()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.TraceIdentifier = string.Empty;

        // Act
        var result = (ViewResult)_controller.Error();
        var model = (ErrorViewModel)result.Model!;

        // Assert
        model.ShowRequestId.Should().BeFalse();
    }
}
