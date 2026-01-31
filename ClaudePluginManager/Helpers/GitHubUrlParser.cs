namespace ClaudePluginManager.Helpers;

public static class GitHubUrlParser
{
    public static (string Owner, string Repo)? Parse(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return null;

        if (!uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
            return null;

        var path = uri.AbsolutePath.TrimStart('/').TrimEnd('/');

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 2)
            return null;

        var owner = segments[0];
        var repo = segments[1];

        // Remove .git suffix if present
        if (repo.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            repo = repo[..^4];

        if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            return null;

        return (owner, repo);
    }
}
