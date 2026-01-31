# Task: Implement GitHubClient Service

## What To Do
Create a service for interacting with the GitHub API to fetch marketplace data and plugin information.

## How To Do
1. Add HTTP client packages:
   - `System.Net.Http.Json`
   - Consider `Octokit` for GitHub-specific operations
2. Create `IGitHubClient` interface:
   ```csharp
   public interface IGitHubClient
   {
       Task<string> GetFileContentAsync(string owner, string repo, string path);
       Task<string> GetReadmeAsync(string owner, string repo);
       Task<GitHubRelease[]> GetReleasesAsync(string owner, string repo);
       Task<bool> RepositoryExistsAsync(string owner, string repo);
   }
   ```
3. Implement `GitHubClient` class:
   - Use HttpClient with proper headers (User-Agent required)
   - Implement rate limiting awareness
   - Handle API errors gracefully
4. Configure GitHub API base URL:
   - `https://api.github.com` for API calls
   - `https://raw.githubusercontent.com` for raw file content
5. Add response caching to reduce API calls
6. Implement retry logic for transient failures
7. Register service in DI container

## Acceptance Criteria
- [ ] `IGitHubClient` interface defined with required methods
- [ ] `GitHubClient` implementation works with unauthenticated requests
- [ ] Can fetch file content from a public repository
- [ ] Can fetch README content from a repository
- [ ] Proper error handling for 404, rate limits, network errors
- [ ] User-Agent header set correctly
- [ ] Service registered in DI container
