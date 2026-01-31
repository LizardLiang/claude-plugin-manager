using ClaudePluginManager.Helpers;

namespace ClaudePluginManager.Tests.Helpers;

public class RelativeTimeFormatterTests
{
    [Fact]
    public void Format_WithNull_ReturnsNeverSynced()
    {
        var result = RelativeTimeFormatter.Format(null);

        Assert.Equal("Never synced", result);
    }

    [Fact]
    public void Format_WithinSeconds_ReturnsJustNow()
    {
        var dateTime = DateTime.UtcNow.AddSeconds(-30);

        var result = RelativeTimeFormatter.Format(dateTime);

        Assert.Equal("Just now", result);
    }

    [Theory]
    [InlineData(1, "1 minute ago")]
    [InlineData(5, "5 minutes ago")]
    [InlineData(59, "59 minutes ago")]
    public void Format_Minutes_ReturnsCorrectString(int minutes, string expected)
    {
        var dateTime = DateTime.UtcNow.AddMinutes(-minutes);

        var result = RelativeTimeFormatter.Format(dateTime);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, "1 hour ago")]
    [InlineData(12, "12 hours ago")]
    [InlineData(23, "23 hours ago")]
    public void Format_Hours_ReturnsCorrectString(int hours, string expected)
    {
        var dateTime = DateTime.UtcNow.AddHours(-hours);

        var result = RelativeTimeFormatter.Format(dateTime);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, "1 day ago")]
    [InlineData(6, "6 days ago")]
    public void Format_Days_ReturnsCorrectString(int days, string expected)
    {
        var dateTime = DateTime.UtcNow.AddDays(-days);

        var result = RelativeTimeFormatter.Format(dateTime);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Format_OverOneWeek_ReturnsAbsoluteTime()
    {
        var dateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Local);

        var result = RelativeTimeFormatter.Format(dateTime);

        Assert.Equal("Jun 15, 2024 2:30 PM", result);
    }

    [Fact]
    public void Format_LocalDateTime_ConvertsToUtcForComparison()
    {
        // Create a local time that is 30 seconds ago
        var localTime = DateTime.Now.AddSeconds(-30);

        var result = RelativeTimeFormatter.Format(localTime);

        Assert.Equal("Just now", result);
    }
}
