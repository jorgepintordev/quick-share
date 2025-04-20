using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using quick_share.api.Business.Commands;
using quick_share.api.Business.Contracts;
using quick_share.api.Business.Models;
using quick_share.api.Endpoints;

namespace quick_share.tests.Endpoints;

public class PostEndTests
{
    readonly Mock<ISessionService> _sessionServiceMock;

    public PostEndTests()
    {
        _sessionServiceMock = new();
    }

    [Fact]
    public async Task PostEnd_IsValid()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.EndSession(It.IsAny<EndSessionCommand>())).ReturnsAsync(Result.Ok());
        
        // Act
        var actual = await SessionEndpoints.PostEnd(It.IsAny<string>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<NoContent>(actual);
    }

    [Fact]
    public async Task PostEnd_ServiceFail()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.EndSession(It.IsAny<EndSessionCommand>())).ReturnsAsync(Result.Fail(string.Empty));

        // Act
        var actual = await SessionEndpoints.PostEnd(It.IsAny<string>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<NotFound>(actual);
    }

    [Fact]
    public async Task PostEnd_ServiceException()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.EndSession(It.IsAny<EndSessionCommand>())).ThrowsAsync(new Exception());

        // Act
        var actual = await SessionEndpoints.PostEnd(It.IsAny<string>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }
}