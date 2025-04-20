using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using quick_share.api.Business.Commands;
using quick_share.api.Business.Contracts;
using quick_share.api.Business.Models;
using quick_share.api.Endpoints;

namespace quick_share.tests.Endpoints;

public class PostBinaryItemTests
{
    readonly Mock<ISessionService> _sessionServiceMock;

    public PostBinaryItemTests()
    {
        _sessionServiceMock = new();
    }

    [Fact]
    public async Task PostBinaryItem_IsValid()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        _sessionServiceMock.Setup(s => s.AddBinaryItem(It.IsAny<AddBinaryItemCommand>())).ReturnsAsync(Result.Ok(It.IsAny<string>()));
        
        // Act
        var actual = await SessionEndpoints.PostBinaryItem(It.IsAny<string>(), It.IsAny<IFormFile>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<Created>(actual);
    }

    [Fact]
    public async Task PostBinaryItem_ServiceFail()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        _sessionServiceMock.Setup(s => s.AddBinaryItem(It.IsAny<AddBinaryItemCommand>())).ReturnsAsync(Result.Fail(string.Empty));

        // Act
        var actual = await SessionEndpoints.PostBinaryItem(It.IsAny<string>(), It.IsAny<IFormFile>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }

    [Fact]
    public async Task PostBinaryItem_SessionNotFound()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Fail(string.Empty));

        // Act
        var actual = await SessionEndpoints.PostBinaryItem(It.IsAny<string>(), It.IsAny<IFormFile>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<NotFound>(actual);
    }

    [Fact]
    public async Task PostBinaryItem_ServiceException()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        _sessionServiceMock.Setup(s => s.AddBinaryItem(It.IsAny<AddBinaryItemCommand>())).ThrowsAsync(new Exception());

        // Act
        var actual = await SessionEndpoints.PostBinaryItem(It.IsAny<string>(), It.IsAny<IFormFile>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }

    [Fact]
    public async Task PostBinaryItem_SessionException()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ThrowsAsync(new Exception());

        // Act
        var actual = await SessionEndpoints.PostBinaryItem(It.IsAny<string>(), It.IsAny<IFormFile>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }
}