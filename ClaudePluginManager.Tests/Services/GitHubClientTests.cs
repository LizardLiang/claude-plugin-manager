using ClaudePluginManager.Services;
using NSubstitute;

namespace ClaudePluginManager.Tests.Services;

public class GitHubClientTests
{
    private readonly IProcessRunner _processRunner;
    private readonly IGitHubClient _gitHubClient;

    public GitHubClientTests()
    {
        _processRunner = Substitute.For<IProcessRunner>();
        _gitHubClient = new GitHubClient(_processRunner);
    }

    private void SetupGhAvailable()
    {
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "--version"))
            .Returns(new ProcessResult(0, "gh version 2.40.0", ""));
    }

    private void SetupGhNotAvailable()
    {
        _processRunner.RunAsync("gh", Arg.Any<string[]>())
            .Returns(new ProcessResult(-1, "", "gh: command not found"));
    }

    #region RepositoryExistsAsync Tests

    [Fact]
    public async Task RepositoryExistsAsync_WithGhAvailable_ReturnsTrue_WhenRepoExists()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 1 && args[0] == "repo" && args[1] == "view"))
            .Returns(new ProcessResult(0, "{\"name\":\"test-repo\"}", ""));

        // Act
        var result = await _gitHubClient.RepositoryExistsAsync("owner", "test-repo");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RepositoryExistsAsync_WithGhAvailable_ReturnsFalse_WhenRepoNotFound()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 1 && args[0] == "repo" && args[1] == "view"))
            .Returns(new ProcessResult(1, "", "Could not resolve to a Repository"));

        // Act
        var result = await _gitHubClient.RepositoryExistsAsync("owner", "nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RepositoryExistsAsync_FallsBackToGit_WhenGhNotAvailable()
    {
        // Arrange
        SetupGhNotAvailable();
        _processRunner.RunAsync("git", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "ls-remote"))
            .Returns(new ProcessResult(0, "abc123\tHEAD", ""));

        // Act
        var result = await _gitHubClient.RepositoryExistsAsync("owner", "repo");

        // Assert
        Assert.True(result);
        await _processRunner.Received().RunAsync("git", Arg.Any<string[]>());
    }

    [Fact]
    public async Task RepositoryExistsAsync_GitFallback_ReturnsFalse_WhenRepoNotFound()
    {
        // Arrange
        SetupGhNotAvailable();
        _processRunner.RunAsync("git", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "ls-remote"))
            .Returns(new ProcessResult(128, "", "repository not found"));

        // Act
        var result = await _gitHubClient.RepositoryExistsAsync("owner", "nonexistent");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetFileContentAsync Tests

    [Fact]
    public async Task GetFileContentAsync_WithGhAvailable_ReturnsDecodedContent()
    {
        // Arrange
        SetupGhAvailable();
        var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("file content"));
        var json = $"{{\"content\":\"{base64Content}\",\"encoding\":\"base64\"}}";
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "api"))
            .Returns(new ProcessResult(0, json, ""));

        // Act
        var result = await _gitHubClient.GetFileContentAsync("owner", "repo", "path/to/file.txt");

        // Assert
        Assert.Equal("file content", result);
    }

    [Fact]
    public async Task GetFileContentAsync_ReturnsNull_WhenFileNotFound()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "api"))
            .Returns(new ProcessResult(1, "", "Not Found"));

        // Act
        var result = await _gitHubClient.GetFileContentAsync("owner", "repo", "nonexistent.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFileContentAsync_ThrowsGitHubCliNotInstalledException_WhenGhNotAvailable()
    {
        // Arrange
        SetupGhNotAvailable();

        // Act & Assert
        await Assert.ThrowsAsync<GitHubCliNotInstalledException>(
            () => _gitHubClient.GetFileContentAsync("owner", "repo", "file.txt"));
    }

    #endregion

    #region GetReadmeAsync Tests

    [Fact]
    public async Task GetReadmeAsync_WithGhAvailable_ReturnsDecodedContent()
    {
        // Arrange
        SetupGhAvailable();
        var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("# README"));
        var json = $"{{\"content\":\"{base64Content}\",\"encoding\":\"base64\"}}";
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args =>
            args.Length > 1 && args[0] == "api" && args[1].Contains("/readme")))
            .Returns(new ProcessResult(0, json, ""));

        // Act
        var result = await _gitHubClient.GetReadmeAsync("owner", "repo");

        // Assert
        Assert.Equal("# README", result);
    }

    [Fact]
    public async Task GetReadmeAsync_ReturnsNull_WhenNoReadme()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "api"))
            .Returns(new ProcessResult(1, "", "Not Found"));

        // Act
        var result = await _gitHubClient.GetReadmeAsync("owner", "repo");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetReadmeAsync_ThrowsGitHubCliNotInstalledException_WhenGhNotAvailable()
    {
        // Arrange
        SetupGhNotAvailable();

        // Act & Assert
        await Assert.ThrowsAsync<GitHubCliNotInstalledException>(
            () => _gitHubClient.GetReadmeAsync("owner", "repo"));
    }

    #endregion

    #region GetReleasesAsync Tests

    [Fact]
    public async Task GetReleasesAsync_WithGhAvailable_ReturnsReleases()
    {
        // Arrange
        SetupGhAvailable();
        var json = """
        [
            {"tagName":"v1.0.0","name":"Release 1.0","publishedAt":"2024-01-01T00:00:00Z","isPrerelease":false},
            {"tagName":"v0.9.0","name":"Beta","publishedAt":"2023-12-01T00:00:00Z","isPrerelease":true}
        ]
        """;
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "release"))
            .Returns(new ProcessResult(0, json, ""));

        // Act
        var result = await _gitHubClient.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal("v1.0.0", result[0].TagName);
        Assert.Equal("Release 1.0", result[0].Name);
        Assert.False(result[0].IsPrerelease);
        Assert.Equal("v0.9.0", result[1].TagName);
        Assert.True(result[1].IsPrerelease);
    }

    [Fact]
    public async Task GetReleasesAsync_ReturnsEmptyArray_WhenNoReleases()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "release"))
            .Returns(new ProcessResult(0, "[]", ""));

        // Act
        var result = await _gitHubClient.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReleasesAsync_ReturnsEmptyArray_WhenGhFails()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "release"))
            .Returns(new ProcessResult(1, "", "error"));

        // Act
        var result = await _gitHubClient.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReleasesAsync_ReturnsEmptyArray_WhenGhNotAvailable()
    {
        // Arrange
        SetupGhNotAvailable();

        // Act
        var result = await _gitHubClient.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReleasesAsync_ReturnsEmptyArray_WhenInvalidJson()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "release"))
            .Returns(new ProcessResult(0, "not valid json", ""));

        // Act
        var result = await _gitHubClient.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region DecodeBase64Content Edge Cases

    [Fact]
    public async Task GetFileContentAsync_ReturnsNull_WhenInvalidJson()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "api"))
            .Returns(new ProcessResult(0, "not valid json", ""));

        // Act
        var result = await _gitHubClient.GetFileContentAsync("owner", "repo", "file.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFileContentAsync_ReturnsNull_WhenNoContentProperty()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "api"))
            .Returns(new ProcessResult(0, "{\"name\":\"file.txt\"}", ""));

        // Act
        var result = await _gitHubClient.GetFileContentAsync("owner", "repo", "file.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFileContentAsync_ReturnsNull_WhenContentIsEmpty()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "api"))
            .Returns(new ProcessResult(0, "{\"content\":\"\"}", ""));

        // Act
        var result = await _gitHubClient.GetFileContentAsync("owner", "repo", "file.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFileContentAsync_ReturnsNull_WhenContentIsNull()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "api"))
            .Returns(new ProcessResult(0, "{\"content\":null}", ""));

        // Act
        var result = await _gitHubClient.GetFileContentAsync("owner", "repo", "file.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFileContentAsync_ReturnsNull_WhenInvalidBase64()
    {
        // Arrange
        SetupGhAvailable();
        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "api"))
            .Returns(new ProcessResult(0, "{\"content\":\"not-valid-base64!!!\"}", ""));

        // Act
        var result = await _gitHubClient.GetFileContentAsync("owner", "repo", "file.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFileContentAsync_HandlesBase64WithNewlines()
    {
        // Arrange
        SetupGhAvailable();
        var contentBytes = System.Text.Encoding.UTF8.GetBytes("Hello World");
        var base64 = Convert.ToBase64String(contentBytes);
        // Add newlines like GitHub API does
        var base64WithNewlines = string.Join("\n", Enumerable.Range(0, base64.Length / 4 + 1)
            .Select(i => base64.Substring(i * 4, Math.Min(4, base64.Length - i * 4))));

        _processRunner.RunAsync("gh", Arg.Is<string[]>(args => args.Length > 0 && args[0] == "api"))
            .Returns(new ProcessResult(0, $"{{\"content\":\"{base64WithNewlines.Replace("\n", "\\n")}\"}}", ""));

        // Act
        var result = await _gitHubClient.GetFileContentAsync("owner", "repo", "file.txt");

        // Assert
        Assert.Equal("Hello World", result);
    }

    #endregion

    #region IsGhAvailableAsync Tests

    [Fact]
    public async Task IsGhAvailableAsync_ReturnsTrue_WhenGhInstalled()
    {
        // Arrange
        SetupGhAvailable();

        // Act
        var result = await _gitHubClient.IsGhAvailableAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsGhAvailableAsync_ReturnsFalse_WhenGhNotInstalled()
    {
        // Arrange
        SetupGhNotAvailable();

        // Act
        var result = await _gitHubClient.IsGhAvailableAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsGhAvailableAsync_CachesResult()
    {
        // Arrange
        SetupGhAvailable();

        // Act
        await _gitHubClient.IsGhAvailableAsync();
        await _gitHubClient.IsGhAvailableAsync();

        // Assert - should only call --version once
        await _processRunner.Received(1).RunAsync("gh", Arg.Is<string[]>(args => args[0] == "--version"));
    }

    #endregion
}
