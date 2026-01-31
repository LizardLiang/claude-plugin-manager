using ClaudePluginManager.Models;

namespace ClaudePluginManager.Tests.Models;

public class InstallResultTests
{
    [Fact]
    public void Ok_ReturnsSuccessResult()
    {
        // Act
        var result = InstallResult.Ok();

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Fail_ReturnsFailureResultWithMessage()
    {
        // Arrange
        var errorMessage = "Installation failed";

        // Act
        var result = InstallResult.Fail(errorMessage);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Fact]
    public void Constructor_CanCreateSuccessDirectly()
    {
        // Act
        var result = new InstallResult(true);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Constructor_CanCreateFailureDirectly()
    {
        // Act
        var result = new InstallResult(false, "Error");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error", result.ErrorMessage);
    }
}

public class UninstallResultTests
{
    [Fact]
    public void Ok_ReturnsSuccessResult()
    {
        // Act
        var result = UninstallResult.Ok();

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Fail_ReturnsFailureResultWithMessage()
    {
        // Arrange
        var errorMessage = "Uninstallation failed";

        // Act
        var result = UninstallResult.Fail(errorMessage);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Fact]
    public void Constructor_CanCreateSuccessDirectly()
    {
        // Act
        var result = new UninstallResult(true);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Constructor_CanCreateFailureDirectly()
    {
        // Act
        var result = new UninstallResult(false, "Error");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error", result.ErrorMessage);
    }
}
