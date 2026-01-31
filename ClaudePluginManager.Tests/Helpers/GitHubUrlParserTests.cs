using ClaudePluginManager.Helpers;

namespace ClaudePluginManager.Tests.Helpers;

public class GitHubUrlParserTests
{
    [Theory]
    [InlineData("https://github.com/owner/repo", "owner", "repo")]
    [InlineData("http://github.com/owner/repo", "owner", "repo")]
    [InlineData("https://github.com/owner/repo.git", "owner", "repo")]
    [InlineData("https://github.com/owner/repo/", "owner", "repo")]
    [InlineData("https://github.com/owner/repo.git/", "owner", "repo")]
    [InlineData("https://github.com/my-org/my-repo", "my-org", "my-repo")]
    [InlineData("https://github.com/owner123/repo456", "owner123", "repo456")]
    public void Parse_ValidGitHubUrl_ReturnsOwnerAndRepo(string url, string expectedOwner, string expectedRepo)
    {
        var result = GitHubUrlParser.Parse(url);

        Assert.NotNull(result);
        Assert.Equal(expectedOwner, result.Value.Owner);
        Assert.Equal(expectedRepo, result.Value.Repo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_NullOrEmpty_ReturnsNull(string? url)
    {
        var result = GitHubUrlParser.Parse(url);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("https://gitlab.com/owner/repo")]
    [InlineData("https://bitbucket.org/owner/repo")]
    [InlineData("https://example.com/owner/repo")]
    [InlineData("not-a-url")]
    [InlineData("https://github.com")]
    [InlineData("https://github.com/")]
    [InlineData("https://github.com/owner")]
    [InlineData("https://github.com/owner/")]
    public void Parse_NonGitHubUrl_ReturnsNull(string url)
    {
        var result = GitHubUrlParser.Parse(url);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("https://github.com/owner/repo/tree/main")]
    [InlineData("https://github.com/owner/repo/blob/main/README.md")]
    [InlineData("https://github.com/owner/repo/issues/123")]
    public void Parse_GitHubUrlWithPath_StillReturnsOwnerAndRepo(string url)
    {
        var result = GitHubUrlParser.Parse(url);

        Assert.NotNull(result);
        Assert.Equal("owner", result.Value.Owner);
        Assert.Equal("repo", result.Value.Repo);
    }

    [Fact]
    public void Parse_CasePreserved()
    {
        var result = GitHubUrlParser.Parse("https://github.com/MyOrg/MyRepo");

        Assert.NotNull(result);
        Assert.Equal("MyOrg", result.Value.Owner);
        Assert.Equal("MyRepo", result.Value.Repo);
    }
}
