using System.Text.Json;
using ClaudePluginManager.Models;

namespace ClaudePluginManager.Tests.Models;

public class PluginComponentsTests
{
    #region PluginComponents Tests

    [Fact]
    public void PluginComponents_HasDefaultEmptyArrays()
    {
        // Act
        var components = new PluginComponents();

        // Assert
        Assert.NotNull(components.McpServers);
        Assert.Empty(components.McpServers);
        Assert.NotNull(components.Hooks);
        Assert.Empty(components.Hooks);
        Assert.NotNull(components.Commands);
        Assert.Empty(components.Commands);
        Assert.NotNull(components.Agents);
        Assert.Empty(components.Agents);
        Assert.NotNull(components.Skills);
        Assert.Empty(components.Skills);
    }

    [Fact]
    public void PluginComponents_CanDeserializeFromJson()
    {
        // Arrange
        var json = """
            {
                "mcpServers": {
                    "test-server": {
                        "command": "node",
                        "args": ["server.js"]
                    }
                },
                "hooks": {
                    "pre-commit": {
                        "script": "echo test"
                    }
                },
                "commands": {},
                "agents": {},
                "skills": {}
            }
            """;

        // Act
        var components = JsonSerializer.Deserialize<PluginComponents>(json);

        // Assert
        Assert.NotNull(components);
        Assert.Single(components.McpServers);
        Assert.True(components.McpServers.ContainsKey("test-server"));
        Assert.Single(components.Hooks);
    }

    [Fact]
    public void PluginComponents_CanSerializeToJson()
    {
        // Arrange
        var components = new PluginComponents
        {
            McpServers = new Dictionary<string, McpServerComponent>
            {
                ["my-server"] = new McpServerComponent
                {
                    Command = "python",
                    Args = new List<string> { "-m", "mcp_server" }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(components);
        var deserialized = JsonSerializer.Deserialize<PluginComponents>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Single(deserialized.McpServers);
        Assert.Equal("python", deserialized.McpServers["my-server"].Command);
    }

    #endregion

    #region McpServerComponent Tests

    [Fact]
    public void McpServerComponent_CanSetAllProperties()
    {
        // Act
        var server = new McpServerComponent
        {
            Command = "npx",
            Args = new List<string> { "-y", "@modelcontextprotocol/server-everything" },
            Env = new Dictionary<string, string> { ["API_KEY"] = "test-key" }
        };

        // Assert
        Assert.Equal("npx", server.Command);
        Assert.Equal(2, server.Args!.Count);
        Assert.Equal("test-key", server.Env!["API_KEY"]);
    }

    [Fact]
    public void McpServerComponent_CanDeserializeFromJson()
    {
        // Arrange
        var json = """
            {
                "command": "node",
                "args": ["server.js", "--port", "8080"],
                "env": {
                    "DEBUG": "true",
                    "PORT": "8080"
                }
            }
            """;

        // Act
        var server = JsonSerializer.Deserialize<McpServerComponent>(json);

        // Assert
        Assert.NotNull(server);
        Assert.Equal("node", server.Command);
        Assert.Equal(3, server.Args!.Count);
        Assert.Equal("true", server.Env!["DEBUG"]);
    }

    [Fact]
    public void McpServerComponent_ToMcpServerConfig_ConvertsCorrectly()
    {
        // Arrange
        var component = new McpServerComponent
        {
            Command = "npx",
            Args = new List<string> { "-y", "package" },
            Env = new Dictionary<string, string> { ["KEY"] = "value" }
        };

        // Act
        var config = component.ToMcpServerConfig();

        // Assert
        Assert.Equal("npx", config.Command);
        Assert.Equal(2, config.Args!.Count);
        Assert.Equal("value", config.Env!["KEY"]);
    }

    #endregion

    #region HookComponent Tests

    [Fact]
    public void HookComponent_CanSetAllProperties()
    {
        // Act
        var hook = new HookComponent
        {
            Matcher = "*.ts",
            Script = "eslint"
        };

        // Assert
        Assert.Equal("*.ts", hook.Matcher);
        Assert.Equal("eslint", hook.Script);
    }

    [Fact]
    public void HookComponent_CanDeserializeFromJson()
    {
        // Arrange
        var json = """
            {
                "matcher": "PreToolUse",
                "script": "echo 'Running hook'"
            }
            """;

        // Act
        var hook = JsonSerializer.Deserialize<HookComponent>(json);

        // Assert
        Assert.NotNull(hook);
        Assert.Equal("PreToolUse", hook.Matcher);
        Assert.Equal("echo 'Running hook'", hook.Script);
    }

    #endregion
}
