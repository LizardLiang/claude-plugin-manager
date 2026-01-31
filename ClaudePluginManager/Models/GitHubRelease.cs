using System;
using System.Text.Json.Serialization;

namespace ClaudePluginManager.Models;

public record GitHubRelease
{
    [JsonPropertyName("tagName")]
    public string TagName { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("publishedAt")]
    public DateTime? PublishedAt { get; init; }

    [JsonPropertyName("isPrerelease")]
    public bool IsPrerelease { get; init; }
}
