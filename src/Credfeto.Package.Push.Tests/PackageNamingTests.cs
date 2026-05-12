using Credfeto.Package.Push.Constants;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Package.Push.Tests;

public sealed class PackageNamingTests : TestBase
{
    [Theory]
    [InlineData("Package.snupkg")]
    [InlineData("Package.symbols.nupkg")]
    public void IsSymbolPackageForSymbolPackageReturnsTrue(string package)
    {
        Assert.True(PackageNaming.IsSymbolPackage(package), $"{package} should be a symbol package");
    }

    [Fact]
    public void IsSymbolPackageForRegularPackageReturnsFalse()
    {
        Assert.False(PackageNaming.IsSymbolPackage("Package.nupkg"), "Regular .nupkg should not be a symbol package");
    }

    [Fact]
    public void IsNotSymbolPackageForRegularPackageReturnsTrue()
    {
        Assert.True(PackageNaming.IsNotSymbolPackage("Package.nupkg"), "Regular .nupkg should not be a symbol package");
    }

    [Theory]
    [InlineData("Package.snupkg")]
    [InlineData("Package.symbols.nupkg")]
    public void IsNotSymbolPackageForSymbolPackageReturnsFalse(string package)
    {
        Assert.False(PackageNaming.IsNotSymbolPackage(package), $"{package} should be a symbol package");
    }

    [Fact]
    public void IsNewSymbolPackageForNewSymbolPackageReturnsTrue()
    {
        Assert.True(
            PackageNaming.IsNewSymbolPackage("Package.snupkg"),
            ".snupkg should be a new-format symbol package"
        );
    }

    [Theory]
    [InlineData("Package.symbols.nupkg")]
    [InlineData("Package.nupkg")]
    public void IsNewSymbolPackageForNonNewSymbolPackageReturnsFalse(string package)
    {
        Assert.False(PackageNaming.IsNewSymbolPackage(package), $"{package} should not be a new-format symbol package");
    }

    [Fact]
    public void IsOldSymbolPackageForOldSymbolPackageReturnsTrue()
    {
        Assert.True(
            PackageNaming.IsOldSymbolPackage("Package.symbols.nupkg"),
            ".symbols.nupkg should be an old-format symbol package"
        );
    }

    [Theory]
    [InlineData("Package.snupkg")]
    [InlineData("Package.nupkg")]
    public void IsOldSymbolPackageForNonOldSymbolPackageReturnsFalse(string package)
    {
        Assert.False(
            PackageNaming.IsOldSymbolPackage(package),
            $"{package} should not be an old-format symbol package"
        );
    }
}
