using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using quick_share.api.Business.Commands;
using quick_share.api.Business.Contracts;
using quick_share.api.Business.Models;
using quick_share.api.Endpoints;

namespace quick_share.tests.Endpoints;

public class PostSimpleItemTests
{
    readonly Mock<ISessionService> _sessionServiceMock;

    public PostSimpleItemTests()
    {
        _sessionServiceMock = new();
    }

    [Fact]
    public async Task PostSimpleItem_IsValid()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        _sessionServiceMock.Setup(s => s.AddSimpleItem(It.IsAny<AddSimpleItemCommand>())).ReturnsAsync(Result.Ok(It.IsAny<string>()));
        
        // Act
        var actual = await SessionEndpoints.PostSimpleItem(It.IsAny<string>(), It.IsAny<string>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<Created>(actual);
    }

    [Fact]
    public async Task PostSimpleItem_ServiceFail()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        _sessionServiceMock.Setup(s => s.AddSimpleItem(It.IsAny<AddSimpleItemCommand>())).ReturnsAsync(Result.Fail(string.Empty));

        // Act
        var actual = await SessionEndpoints.PostSimpleItem(It.IsAny<string>(), It.IsAny<string>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }

    [Fact]
    public async Task PostSimpleItem_SessionNotFound()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Fail(string.Empty));

        // Act
        var actual = await SessionEndpoints.PostSimpleItem(It.IsAny<string>(), It.IsAny<string>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<NotFound>(actual);
    }

    [Fact]
    public async Task PostSimpleItem_ServiceException()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        _sessionServiceMock.Setup(s => s.EndSession(It.IsAny<EndSessionCommand>())).ThrowsAsync(new Exception());

        // Act
        var actual = await SessionEndpoints.PostSimpleItem(It.IsAny<string>(), It.IsAny<string>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }

    [Fact]
    public async Task PostSimpleItem_SessionException()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ThrowsAsync(new Exception());

        // Act
        var actual = await SessionEndpoints.PostSimpleItem(It.IsAny<string>(), It.IsAny<string>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }
}