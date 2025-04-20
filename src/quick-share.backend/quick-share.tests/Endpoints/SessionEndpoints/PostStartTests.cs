using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using quick_share.api.Business.Contracts;
using quick_share.api.Endpoints;

namespace quick_share.tests.Endpoints;

public class PostStartTests
{
    readonly Mock<ISessionService> _sessionServiceMock;

    public PostStartTests()
    {
        _sessionServiceMock = new();
    }

    [Fact]
    public async Task PostStart_IsValid()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.StartSession()).ReturnsAsync(Result.Ok(It.IsAny<string>()));
        
        // Act
        var actual = await SessionEndpoints.PostStart(_sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<Created>(actual);
    }

    [Fact]
    public async Task PostStart_ServiceFail()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.StartSession()).ReturnsAsync(Result.Fail(string.Empty));
        
        // Act
        var actual = await SessionEndpoints.PostStart(_sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }

    [Fact]
    public async Task PostStart_ServiceException()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.StartSession()).ThrowsAsync(new Exception());
        
        // Act
        var actual = await SessionEndpoints.PostStart(_sessionServiceMock.Object);
    
        // Assert
        Assert.IsType<StatusCodeHttpResult>(actual);
        var actualResult = (StatusCodeHttpResult)actual;
        Assert.Equal(500, actualResult.StatusCode);
    }
}