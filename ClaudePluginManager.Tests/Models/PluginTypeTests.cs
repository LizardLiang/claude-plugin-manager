using ClaudePluginManager.Models;

namespace ClaudePluginManager.Tests.Models;

public class PluginTypeTests
{
    #region ToDbString Tests

    [Theory]
    [InlineData(PluginType.McpServer, "MCP_SERVER")]
    [InlineData(PluginType.Hook, "HOOK")]
    [InlineData(PluginType.SlashCommand, "SLASH_COMMAND")]
    [InlineData(PluginType.Agent, "AGENT")]
    [InlineData(PluginType.Skill, "SKILL")]
    public void ToDbString_ReturnsCorrectString(PluginType type, string expected)
    {
        // Act
        var result = type.ToDbString();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region FromDbString Tests

    [Theory]
    [InlineData("MCP_SERVER", PluginType.McpServer)]
    [InlineData("HOOK", PluginType.Hook)]
    [InlineData("SLASH_COMMAND", PluginType.SlashCommand)]
    [InlineData("AGENT", PluginType.Agent)]
    [InlineData("SKILL", PluginType.Skill)]
    public void FromDbString_ReturnsCorrectType(string value, PluginType expected)
    {
        // Act
        var result = PluginTypeExtensions.FromDbString(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FromDbString_WhenInvalidValue_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => PluginTypeExtensions.FromDbString("INVALID"));
        Assert.Contains("Unknown plugin type", ex.Message);
    }

    [Fact]
    public void FromDbString_IsCaseInsensitive()
    {
        // Act
        var result = PluginTypeExtensions.FromDbString("mcp_server");

        // Assert
        Assert.Equal(PluginType.McpServer, result);
    }

    #endregion

    #region RoundTrip Tests

    [Theory]
    [InlineData(PluginType.McpServer)]
    [InlineData(PluginType.Hook)]
    [InlineData(PluginType.SlashCommand)]
    [InlineData(PluginType.Agent)]
    [InlineData(PluginType.Skill)]
    public void RoundTrip_ConversionIsSymmetric(PluginType original)
    {
        // Act
        var dbString = original.ToDbString();
        var roundTripped = PluginTypeExtensions.FromDbString(dbString);

        // Assert
        Assert.Equal(original, roundTripped);
    }

    #endregion
}
