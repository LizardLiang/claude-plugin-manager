using ClaudePluginManager.Helpers;

namespace ClaudePluginManager.Tests.Helpers;

public class VersionComparerTests
{
    #region Compare Tests

    [Theory]
    [InlineData("1.0.0", "1.0.0", 0)]
    [InlineData("2.0.0", "1.0.0", 1)]
    [InlineData("1.0.0", "2.0.0", -1)]
    [InlineData("1.1.0", "1.0.0", 1)]
    [InlineData("1.0.1", "1.0.0", 1)]
    [InlineData("1.0.0", "1.1.0", -1)]
    [InlineData("1.0.0", "1.0.1", -1)]
    public void Compare_SemverVersions_ReturnsCorrectResult(string v1, string v2, int expected)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("v1.0.0", "1.0.0", 0)]
    [InlineData("1.0.0", "v1.0.0", 0)]
    [InlineData("v2.0.0", "v1.0.0", 1)]
    [InlineData("V1.0.0", "1.0.0", 0)]
    public void Compare_WithVPrefix_StripsPrefix(string v1, string v2, int expected)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1.0.0.0", "1.0.0.0", 0)]
    [InlineData("1.0.0.1", "1.0.0.0", 1)]
    [InlineData("1.0.0.0", "1.0.0.1", -1)]
    [InlineData("1.0.0", "1.0.0.0", 0)]
    [InlineData("1.0.0.0", "1.0.0", 0)]
    public void Compare_FourPartVersions_ReturnsCorrectResult(string v1, string v2, int expected)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0-beta", 1)]
    [InlineData("1.0.0-beta", "1.0.0", -1)]
    [InlineData("1.0.0-alpha", "1.0.0-beta", -1)]
    [InlineData("1.0.0-beta", "1.0.0-alpha", 1)]
    [InlineData("1.0.0-rc.1", "1.0.0-beta.2", 1)]
    [InlineData("1.0.0-beta.1", "1.0.0-beta.2", -1)]
    [InlineData("1.0.0-beta.2", "1.0.0-beta.1", 1)]
    public void Compare_PreReleaseVersions_ReturnsCorrectResult(string v1, string v2, int expected)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null, 0)]
    [InlineData("1.0.0", null, 1)]
    [InlineData(null, "1.0.0", -1)]
    [InlineData("", "", 0)]
    [InlineData("1.0.0", "", 1)]
    [InlineData("", "1.0.0", -1)]
    public void Compare_NullOrEmpty_ReturnsCorrectResult(string? v1, string? v2, int expected)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("invalid", "1.0.0", -1)]
    [InlineData("1.0.0", "invalid", 1)]
    [InlineData("invalid", "invalid", 0)]
    [InlineData("not.a.version", "also.not", 0)]
    public void Compare_InvalidVersions_TreatsAsLowerThanValid(string v1, string v2, int expected)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("10.0.0", "9.0.0", 1)]
    [InlineData("1.10.0", "1.9.0", 1)]
    [InlineData("1.0.10", "1.0.9", 1)]
    public void Compare_MultiDigitVersions_ComparesNumerically(string v1, string v2, int expected)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.Equal(expected, result);
    }

    #endregion

    #region IsNewer Tests

    [Theory]
    [InlineData("2.0.0", "1.0.0", true)]
    [InlineData("1.1.0", "1.0.0", true)]
    [InlineData("1.0.1", "1.0.0", true)]
    public void IsNewer_WhenAvailableIsHigher_ReturnsTrue(string available, string installed, bool expected)
    {
        var result = VersionComparer.IsNewer(available, installed);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1.0.0", "2.0.0", false)]
    [InlineData("1.0.0", "1.0.0", false)]
    [InlineData("1.0.0", "1.0.1", false)]
    public void IsNewer_WhenAvailableIsNotHigher_ReturnsFalse(string available, string installed, bool expected)
    {
        var result = VersionComparer.IsNewer(available, installed);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "1.0.0", false)]
    [InlineData("1.0.0", null, false)]
    [InlineData(null, null, false)]
    [InlineData("", "1.0.0", false)]
    [InlineData("1.0.0", "", false)]
    public void IsNewer_WithNullOrEmpty_ReturnsFalse(string? available, string? installed, bool expected)
    {
        var result = VersionComparer.IsNewer(available, installed);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("invalid", "1.0.0", false)]
    [InlineData("1.0.0", "invalid", false)]
    public void IsNewer_WithInvalidVersion_ReturnsFalse(string available, string installed, bool expected)
    {
        var result = VersionComparer.IsNewer(available, installed);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("v2.0.0", "v1.0.0", true)]
    [InlineData("v1.0.0", "v1.0.0", false)]
    public void IsNewer_WithVPrefix_HandlesCorrectly(string available, string installed, bool expected)
    {
        var result = VersionComparer.IsNewer(available, installed);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsNewer_PreReleaseToStable_ReturnsTrue()
    {
        var result = VersionComparer.IsNewer("1.0.0", "1.0.0-beta");

        Assert.True(result);
    }

    [Fact]
    public void IsNewer_StableToPreRelease_ReturnsFalse()
    {
        var result = VersionComparer.IsNewer("1.0.0-beta", "1.0.0");

        Assert.False(result);
    }

    #endregion
}
