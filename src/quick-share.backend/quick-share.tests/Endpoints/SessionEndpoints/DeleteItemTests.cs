using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using quick_share.api.Business.Commands;
using quick_share.api.Business.Contracts;
using quick_share.api.Business.Models;
using quick_share.api.Endpoints;

namespace quick_share.tests.Endpoints;

public class DeleteItemTests
{
    readonly Mock<ISessionService> _sessionServiceMock;

    public DeleteItemTests()
    {
        _sessionServiceMock = new();
    }

    [Fact]
    public async Task DeleteItem_IsValid()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        _sessionServiceMock.Setup(s => s.DeleteItem(It.IsAny<DeleteItemCommand>())).ReturnsAsync(Result.Ok());
        
        // Act
        var actual = await SessionEndpoints.DeleteItem(It.IsAny<string>(), It.IsAny<Guid>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<NoContent>(actual);
    }

    [Fact]
    public async Task DeleteItem_ServiceFail()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        _sessionServiceMock.Setup(s => s.DeleteItem(It.IsAny<DeleteItemCommand>())).ReturnsAsync(Result.Fail(string.Empty));

        // Act
        var actual = await SessionEndpoints.DeleteItem(It.IsAny<string>(), It.IsAny<Guid>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<NotFound>(actual);
    }

    [Fact]
    public async Task DeleteItem_SessionNotFound()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Fail(string.Empty));

        // Act
        var actual = await SessionEndpoints.DeleteItem(It.IsAny<string>(), It.IsAny<Guid>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<NotFound>(actual);
    }

    [Fact]
    public async Task DeleteItem_ServiceException()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        _sessionServiceMock.Setup(s => s.DeleteItem(It.IsAny<DeleteItemCommand>())).ThrowsAsync(new Exception());

        // Act
        var actual = await SessionEndpoints.DeleteItem(It.IsAny<string>(), It.IsAny<Guid>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_SessionException()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ThrowsAsync(new Exception());

        // Act
        var actual = await SessionEndpoints.DeleteItem(It.IsAny<string>(), It.IsAny<Guid>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }
}