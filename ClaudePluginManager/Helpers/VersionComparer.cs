namespace ClaudePluginManager.Helpers;

/// <summary>
/// Static helper class for semantic version comparison.
/// </summary>
public static class VersionComparer
{
    /// <summary>
    /// Compares two version strings.
    /// </summary>
    /// <param name="v1">First version string.</param>
    /// <param name="v2">Second version string.</param>
    /// <returns>
    /// -1 if v1 is less than v2,
    ///  0 if v1 equals v2,
    ///  1 if v1 is greater than v2.
    /// </returns>
    public static int Compare(string? v1, string? v2)
    {
        // Handle null/empty cases
        var v1Empty = string.IsNullOrWhiteSpace(v1);
        var v2Empty = string.IsNullOrWhiteSpace(v2);

        if (v1Empty && v2Empty) return 0;
        if (v1Empty) return -1;
        if (v2Empty) return 1;

        // Strip 'v' or 'V' prefix
        v1 = StripVersionPrefix(v1!);
        v2 = StripVersionPrefix(v2!);

        // Parse versions
        var parsed1 = ParseVersion(v1);
        var parsed2 = ParseVersion(v2);

        // If both are invalid, they're equal
        if (!parsed1.IsValid && !parsed2.IsValid) return 0;
        // Invalid versions are considered less than valid ones
        if (!parsed1.IsValid) return -1;
        if (!parsed2.IsValid) return 1;

        // Compare major.minor.patch.build
        for (int i = 0; i < 4; i++)
        {
            var cmp = Normalize(parsed1.Parts[i].CompareTo(parsed2.Parts[i]));
            if (cmp != 0) return cmp;
        }

        // Compare pre-release tags
        return ComparePreRelease(parsed1.PreRelease, parsed2.PreRelease);
    }

    /// <summary>
    /// Determines if an available version is newer than an installed version.
    /// </summary>
    /// <param name="available">The available version from marketplace.</param>
    /// <param name="installed">The installed version.</param>
    /// <returns>True if an update is available, false otherwise.</returns>
    public static bool IsNewer(string? available, string? installed)
    {
        // Return false for null/empty versions
        if (string.IsNullOrWhiteSpace(available) || string.IsNullOrWhiteSpace(installed))
            return false;

        // Strip prefixes and parse to check validity
        var availableClean = StripVersionPrefix(available);
        var installedClean = StripVersionPrefix(installed);

        var parsedAvailable = ParseVersion(availableClean);
        var parsedInstalled = ParseVersion(installedClean);

        // If either version is invalid, return false
        if (!parsedAvailable.IsValid || !parsedInstalled.IsValid)
            return false;

        return Compare(available, installed) > 0;
    }

    private static string StripVersionPrefix(string version)
    {
        if (version.StartsWith('v') || version.StartsWith('V'))
            return version[1..];
        return version;
    }

    private static ParsedVersion ParseVersion(string version)
    {
        var result = new ParsedVersion();

        // Split pre-release from version
        var preReleaseIndex = version.IndexOf('-');
        string versionPart;
        if (preReleaseIndex >= 0)
        {
            versionPart = version[..preReleaseIndex];
            result.PreRelease = version[(preReleaseIndex + 1)..];
        }
        else
        {
            versionPart = version;
            result.PreRelease = null;
        }

        // Parse version parts
        var parts = versionPart.Split('.');
        if (parts.Length < 1 || parts.Length > 4)
        {
            result.IsValid = false;
            return result;
        }

        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out var num) || num < 0)
            {
                result.IsValid = false;
                return result;
            }
            result.Parts[i] = num;
        }

        result.IsValid = true;
        return result;
    }

    private static int ComparePreRelease(string? pr1, string? pr2)
    {
        // No pre-release is greater than any pre-release
        // e.g., 1.0.0 > 1.0.0-beta
        if (pr1 == null && pr2 == null) return 0;
        if (pr1 == null) return 1;  // v1 is stable, v2 is pre-release
        if (pr2 == null) return -1; // v1 is pre-release, v2 is stable

        // Compare pre-release identifiers
        var parts1 = pr1.Split('.');
        var parts2 = pr2.Split('.');

        var minLength = Math.Min(parts1.Length, parts2.Length);
        for (int i = 0; i < minLength; i++)
        {
            var cmp = ComparePreReleaseIdentifier(parts1[i], parts2[i]);
            if (cmp != 0) return cmp;
        }

        // Longer pre-release has higher precedence
        return Normalize(parts1.Length.CompareTo(parts2.Length));
    }

    private static int ComparePreReleaseIdentifier(string id1, string id2)
    {
        var isNum1 = int.TryParse(id1, out var num1);
        var isNum2 = int.TryParse(id2, out var num2);

        // Numeric identifiers have lower precedence than alphanumeric
        if (isNum1 && isNum2)
            return Normalize(num1.CompareTo(num2));
        if (isNum1)
            return -1;
        if (isNum2)
            return 1;

        // Alphanumeric comparison - normalize to -1, 0, 1
        return Normalize(string.Compare(id1, id2, StringComparison.Ordinal));
    }

    private static int Normalize(int comparison)
    {
        if (comparison < 0) return -1;
        if (comparison > 0) return 1;
        return 0;
    }

    private struct ParsedVersion
    {
        public int[] Parts;
        public string? PreRelease;
        public bool IsValid;

        public ParsedVersion()
        {
            Parts = new int[4];
            PreRelease = null;
            IsValid = false;
        }
    }
}
