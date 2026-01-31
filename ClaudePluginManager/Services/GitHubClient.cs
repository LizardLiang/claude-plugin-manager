using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClaudePluginManager.Models;

namespace ClaudePluginManager.Services;

public class GitHubClient : IGitHubClient
{
    private readonly IProcessRunner _processRunner;
    private bool? _ghAvailable;

    public GitHubClient(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<bool> IsGhAvailableAsync()
    {
        if (_ghAvailable.HasValue)
            return _ghAvailable.Value;

        var result = await _processRunner.RunAsync("gh", "--version");
        _ghAvailable = result.Success;
        return _ghAvailable.Value;
    }

    public async Task<bool> RepositoryExistsAsync(string owner, string repo)
    {
        if (await IsGhAvailableAsync())
        {
            var result = await _processRunner.RunAsync("gh", "repo", "view", $"{owner}/{repo}", "--json", "name");
            return result.Success;
        }

        // Fallback to git ls-remote
        return await RepositoryExistsViaGitAsync(owner, repo);
    }

    private async Task<bool> RepositoryExistsViaGitAsync(string owner, string repo)
    {
        var result = await _processRunner.RunAsync("git", "ls-remote", $"https://github.com/{owner}/{repo}.git");
        return result.Success;
    }

    public async Task<string?> GetFileContentAsync(string owner, string repo, string path)
    {
        if (!await IsGhAvailableAsync())
        {
            throw new GitHubCliNotInstalledException();
        }

        var result = await _processRunner.RunAsync("gh", "api", $"repos/{owner}/{repo}/contents/{path}");
        if (!result.Success)
        {
            if (result.Error?.Contains("gh auth login") == true)
            {
                throw new GitHubCliNotAuthenticatedException();
            }
            return null;
        }

        return DecodeBase64Content(result.Output);
    }

    public async Task<string?> GetReadmeAsync(string owner, string repo)
    {
        if (!await IsGhAvailableAsync())
        {
            throw new GitHubCliNotInstalledException();
        }

        var result = await _processRunner.RunAsync("gh", "api", $"repos/{owner}/{repo}/readme");
        if (!result.Success)
        {
            if (result.Error?.Contains("gh auth login") == true)
            {
                throw new GitHubCliNotAuthenticatedException();
            }
            return null;
        }

        return DecodeBase64Content(result.Output);
    }

    public async Task<GitHubRelease[]> GetReleasesAsync(string owner, string repo)
    {
        if (await IsGhAvailableAsync())
        {
            var result = await _processRunner.RunAsync(
                "gh", "release", "list",
                "-R", $"{owner}/{repo}",
                "--json", "tagName,name,publishedAt,isPrerelease");

            if (!result.Success)
                return [];

            try
            {
                return JsonSerializer.Deserialize<GitHubRelease[]>(result.Output) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        // No git fallback for releases - this is GitHub-specific
        return [];
    }

    private static string? DecodeBase64Content(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("content", out var contentElement))
                return null;

            var base64Content = contentElement.GetString();
            if (string.IsNullOrEmpty(base64Content))
                return null;

            // GitHub API returns base64 with newlines, remove them
            base64Content = base64Content.Replace("\n", "").Replace("\r", "");
            var bytes = Convert.FromBase64String(base64Content);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return null;
        }
    }
}
