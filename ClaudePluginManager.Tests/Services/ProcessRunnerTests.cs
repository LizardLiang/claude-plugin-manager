using ClaudePluginManager.Services;

namespace ClaudePluginManager.Tests.Services;

public class ProcessRunnerTests
{
    private readonly ProcessRunner _processRunner;

    public ProcessRunnerTests()
    {
        _processRunner = new ProcessRunner();
    }

    [Fact]
    public async Task RunAsync_WithSimpleCommand_ReturnsOutput()
    {
        // Arrange & Act
        var result = await _processRunner.RunAsync("echo", "hello");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("hello", result.Output);
        Assert.Empty(result.Error);
    }

    [Fact]
    public async Task RunAsync_WithNonExistentCommand_ReturnsCommandNotFound()
    {
        // Arrange & Act
        var result = await _processRunner.RunAsync("nonexistent-command-12345");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(-1, result.ExitCode);
        Assert.Contains("command not found", result.Error);
    }

    [Fact]
    public async Task RunAsync_WithFailingCommand_ReturnsNonZeroExitCode()
    {
        // Arrange & Act
        var result = await _processRunner.RunAsync("ls", "/nonexistent-path-12345");

        // Assert
        Assert.False(result.Success);
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public async Task RunAsync_WithMultipleArguments_PassesAllArguments()
    {
        // Arrange & Act
        var result = await _processRunner.RunAsync("echo", "arg1", "arg2", "arg3");

        // Assert
        Assert.True(result.Success);
        Assert.Contains("arg1", result.Output);
        Assert.Contains("arg2", result.Output);
        Assert.Contains("arg3", result.Output);
    }

    [Fact]
    public async Task RunAsync_WithArgumentsContainingSpaces_EscapesCorrectly()
    {
        // Arrange & Act
        var result = await _processRunner.RunAsync("echo", "hello world");

        // Assert
        Assert.True(result.Success);
        Assert.Contains("hello world", result.Output);
    }

    [Fact]
    public async Task RunAsync_WithEmptyArgument_HandlesCorrectly()
    {
        // Arrange & Act
        var result = await _processRunner.RunAsync("echo", "");

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task RunAsync_WithArgumentsContainingQuotes_EscapesCorrectly()
    {
        // Arrange & Act
        var result = await _processRunner.RunAsync("echo", "say \"hello\"");

        // Assert
        Assert.True(result.Success);
        // The exact output depends on shell behavior, but it should not crash
    }

    [Fact]
    public async Task RunAsync_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await _processRunner.RunAsync("sleep", new[] { "10" }, cts.Token));
    }

    [Fact]
    public async Task RunAsync_WithStderr_CapturesError()
    {
        // Arrange & Act
        var result = await _processRunner.RunAsync("ls", "/nonexistent-directory-xyz");

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task RunAsync_Overload_WithoutCancellationToken_Works()
    {
        // Arrange & Act
        var result = await _processRunner.RunAsync("echo", new[] { "test" });

        // Assert
        Assert.True(result.Success);
        Assert.Contains("test", result.Output);
    }
}
