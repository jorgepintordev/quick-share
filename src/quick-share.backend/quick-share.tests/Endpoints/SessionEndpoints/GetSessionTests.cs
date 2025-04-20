using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using quick_share.api.Business.Commands;
using quick_share.api.Business.Contracts;
using quick_share.api.Business.Models;
using quick_share.api.Endpoints;

namespace quick_share.tests.Endpoints;

public class GetSessionTests
{
    readonly Mock<ISessionService> _sessionServiceMock;

    public GetSessionTests()
    {
        _sessionServiceMock = new();
    }

    [Fact]
    public async Task GetSession_IsValid()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Ok(It.IsAny<Session>()));

        // Act
        var actual = await SessionEndpoints.GetSession(It.IsAny<string>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<Ok<Session>>(actual);
    }

    [Fact]
    public async Task GetSession_ServiceFail()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ReturnsAsync(Result.Fail(string.Empty));

        // Act
        var actual = await SessionEndpoints.GetSession(It.IsAny<string>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<NotFound>(actual);
    }

    [Fact]
    public async Task GetSession_ServiceException()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetSession(It.IsAny<GetSessionCommand>())).ThrowsAsync(new Exception());
        
        // Act
        var actual = await SessionEndpoints.GetSession(It.IsAny<string>(), _sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }
}