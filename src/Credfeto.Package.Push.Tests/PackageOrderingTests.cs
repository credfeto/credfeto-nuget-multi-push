using System;
using Credfeto.Package.Push.Helpers;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Package.Push.Tests;

public sealed class PackageOrderingTests : TestBase
{
    [Theory]
    [InlineData("FunFair.all.1.2.3.nupkg")]
    [InlineData("FunFair.ALL.1.2.3.nupkg")]
    [InlineData("FunFair.All.4.5.6.nupkg")]
    public void IsMetaPackageForMetaPackageReturnsTrue(string packageId)
    {
        Assert.True(PackageOrdering.IsMetaPackage(packageId), $"{packageId} should be identified as a meta-package");
    }

    [Theory]
    [InlineData("FunFair.Common.1.2.3.nupkg")]
    [InlineData("FunFair.nupkg")]
    [InlineData("1.2.3.nupkg")]
    [InlineData("FunFair.all.nupkg")]
    public void IsMetaPackageForNonMetaPackageReturnsFalse(string packageId)
    {
        Assert.False(
            PackageOrdering.IsMetaPackage(packageId),
            $"{packageId} should not be identified as a meta-package"
        );
    }

    [Theory]
    [InlineData("all")]
    [InlineData("ALL")]
    [InlineData("All")]
    public void IsAllTagForAllTagReturnsTrue(string value)
    {
        Assert.True(PackageOrdering.IsAllTag(value.AsSpan()), $"\"{value}\" should be identified as the all tag");
    }

    [Theory]
    [InlineData("other")]
    [InlineData("allx")]
    [InlineData("")]
    public void IsAllTagForNonAllTagReturnsFalse(string value)
    {
        Assert.False(PackageOrdering.IsAllTag(value.AsSpan()), $"\"{value}\" should not be identified as the all tag");
    }
}
