using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using quick_share.api.Business.Contracts;
using quick_share.api.Endpoints;

namespace quick_share.tests.Endpoints;

public class PostFileCleanupTests
{
    readonly Mock<IMaintenanceService> _maintenanceServiceMock;

    public PostFileCleanupTests()
    {
        _maintenanceServiceMock = new();
    }
    
    [Fact]
    public async Task PostFileCleanup_IsValid()
    {
        // Arrange
        _maintenanceServiceMock.Setup(s => s.FileCleanup()).ReturnsAsync(Result.Ok());
        // Act
        var actual = await MaintenanceEndpoints.PostFileCleanup(_maintenanceServiceMock.Object);
    
        // Assert
        Assert.IsType<NoContent>(actual);
    }

    [Fact]
    public async Task PostFileCleanup_ServiceFail()
    {
        // Arrange
        _maintenanceServiceMock.Setup(s => s.FileCleanup()).ReturnsAsync(Result.Fail(string.Empty));
        // Act
        var actual = await MaintenanceEndpoints.PostFileCleanup(_maintenanceServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }

    [Fact]
    public async Task PostFileCleanup_ServiceException()
    {
        // Arrange
        _maintenanceServiceMock.Setup(s => s.FileCleanup()).ThrowsAsync(new Exception());
        // Act
        var actual = await MaintenanceEndpoints.PostFileCleanup(_maintenanceServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }
}