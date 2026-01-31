namespace ClaudePluginManager.Helpers;

public static class RelativeTimeFormatter
{
    public static string Format(DateTime? dateTime)
    {
        if (dateTime == null)
            return "Never synced";

        var elapsed = DateTime.UtcNow - dateTime.Value.ToUniversalTime();

        if (elapsed.TotalSeconds < 60)
            return "Just now";

        if (elapsed.TotalMinutes < 60)
            return FormatUnit((int)elapsed.TotalMinutes, "minute");

        if (elapsed.TotalHours < 24)
            return FormatUnit((int)elapsed.TotalHours, "hour");

        if (elapsed.TotalDays < 7)
            return FormatUnit((int)elapsed.TotalDays, "day");

        return dateTime.Value.ToLocalTime().ToString("MMM d, yyyy h:mm tt");
    }

    private static string FormatUnit(int value, string unit)
    {
        return value == 1
            ? $"{value} {unit} ago"
            : $"{value} {unit}s ago";
    }
}
