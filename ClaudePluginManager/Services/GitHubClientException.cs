namespace ClaudePluginManager.Services;

public class GitHubClientException : Exception
{
    public GitHubClientException(string message) : base(message)
    {
    }

    public GitHubClientException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class GitHubCliNotInstalledException : GitHubClientException
{
    public GitHubCliNotInstalledException()
        : base("GitHub CLI (gh) is not installed. Please install it from https://cli.github.com/")
    {
    }
}

public class GitHubCliNotAuthenticatedException : GitHubClientException
{
    public GitHubCliNotAuthenticatedException()
        : base("GitHub CLI is not authenticated. Please run 'gh auth login' in your terminal.")
    {
    }
}
