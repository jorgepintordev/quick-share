using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using quick_share.api.Business.Commands;
using quick_share.api.Business.Contracts;
using quick_share.api.Business.Models;
using quick_share.api.Endpoints;

namespace quick_share.tests.Endpoints;

public class GetBinaryItemTests
{
    readonly Mock<ISessionService> _sessionServiceMock;

    public GetBinaryItemTests()
    {
        _sessionServiceMock = new();
    }

    [Fact]
    public async Task GetBinaryItem_IsValid()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        var sharedItemBinaryResult = new SharedItemBinaryResult {
            Data = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test data")),
            Filename = "testFile.txt"
        };
        _sessionServiceMock.Setup(s => s.GetBinaryItem(It.IsAny<GetBinaryItemCommand>())).Returns(Result.Ok(sharedItemBinaryResult));
        
        // Act
        var actual = await SessionEndpoints.GetBinaryItem(It.IsAny<string>(), It.IsAny<Guid>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<FileStreamHttpResult>(actual);
    }

    [Fact]
    public async Task GetBinaryItem_ServiceFail()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        _sessionServiceMock.Setup(s => s.GetBinaryItem(It.IsAny<GetBinaryItemCommand>())).Returns(Result.Fail(string.Empty));

        // Act
        var actual = await SessionEndpoints.GetBinaryItem(It.IsAny<string>(), It.IsAny<Guid>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<NotFound>(actual);
    }

    [Fact]
    public async Task GetBinaryItem_SessionNotFound()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Fail(string.Empty));

        // Act
        var actual = await SessionEndpoints.GetBinaryItem(It.IsAny<string>(), It.IsAny<Guid>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<NotFound>(actual);
    }

    [Fact]
    public async Task GetBinaryItem_ServiceException()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));
        _sessionServiceMock.Setup(s => s.GetBinaryItem(It.IsAny<GetBinaryItemCommand>())).Throws(new Exception());

        // Act
        var actual = await SessionEndpoints.GetBinaryItem(It.IsAny<string>(), It.IsAny<Guid>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }

    [Fact]
    public async Task GetBinaryItem_SessionException()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ThrowsAsync(new Exception());

        // Act
        var actual = await SessionEndpoints.GetBinaryItem(It.IsAny<string>(), It.IsAny<Guid>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }
}