using System.Threading.Tasks;
using ClaudePluginManager.Models;

namespace ClaudePluginManager.Services;

public interface IGitHubClient
{
    Task<string?> GetFileContentAsync(string owner, string repo, string path);
    Task<string?> GetReadmeAsync(string owner, string repo);
    Task<GitHubRelease[]> GetReleasesAsync(string owner, string repo);
    Task<bool> RepositoryExistsAsync(string owner, string repo);
    Task<bool> IsGhAvailableAsync();
}
