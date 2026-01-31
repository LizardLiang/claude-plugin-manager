using System.Diagnostics;
using System.Text.Json;
using ClaudePluginManager.Helpers;
using ClaudePluginManager.Models;
using ClaudePluginManager.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClaudePluginManager.ViewModels;

public partial class PluginDetailsViewModel : ViewModelBase
{
    private readonly IGitHubClient _gitHubClient;

    public PluginDetailsViewModel(IGitHubClient gitHubClient)
    {
        _gitHubClient = gitHubClient;
    }

    public event EventHandler? CloseRequested;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private string _author = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _type = string.Empty;

    [ObservableProperty]
    private string _displayType = string.Empty;

    [ObservableProperty]
    private string? _repositoryUrl;

    [ObservableProperty]
    private IReadOnlyList<string> _tags = Array.Empty<string>();

    [ObservableProperty]
    private string? _readmeContent;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasNoReadme;

    [ObservableProperty]
    private bool _isInstalled;

    public bool HasRepositoryUrl => !string.IsNullOrEmpty(RepositoryUrl);

    public async Task InitializeAsync(Plugin plugin)
    {
        Name = plugin.Name;
        Version = plugin.Version ?? "Unknown";
        Author = plugin.Author ?? "Unknown";
        Description = plugin.Description ?? string.Empty;
        Type = plugin.Type;
        DisplayType = FormatType(plugin.Type);
        RepositoryUrl = plugin.Repository;
        Tags = ParseTags(plugin.Tags);

        OnPropertyChanged(nameof(HasRepositoryUrl));

        // Clear previous readme state
        ReadmeContent = null;
        HasNoReadme = false;

        await LoadReadmeAsync();
    }

    private async Task LoadReadmeAsync()
    {
        var parsed = GitHubUrlParser.Parse(RepositoryUrl);
        if (parsed == null)
        {
            HasNoReadme = true;
            return;
        }

        try
        {
            IsLoading = true;
            var readme = await _gitHubClient.GetReadmeAsync(parsed.Value.Owner, parsed.Value.Repo);

            if (string.IsNullOrEmpty(readme))
            {
                HasNoReadme = true;
            }
            else
            {
                ReadmeContent = readme;
            }
        }
        catch
        {
            HasNoReadme = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void OpenRepository()
    {
        if (string.IsNullOrEmpty(RepositoryUrl))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = RepositoryUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore errors opening URL
        }
    }

    private static string FormatType(string type) => type switch
    {
        "MCP_SERVER" => "MCP Server",
        "HOOK" => "Hook",
        "SLASH_COMMAND" => "Slash Command",
        "AGENT" => "Agent",
        "SKILL" => "Skill",
        _ => type.Replace("_", " ")
    };

    private static IReadOnlyList<string> ParseTags(string? tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
            return Array.Empty<string>();

        try
        {
            return JsonSerializer.Deserialize<string[]>(tagsJson) ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }
}
