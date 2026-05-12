using System.IO;
using Credfeto.Package.Push.Helpers;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Package.Push.Tests;

public sealed class PathHelpersTests : TestBase
{
    [Fact]
    public void ConvertToNativeWithNativeSeparatorRetainsPath()
    {
        string path = $"root{Path.DirectorySeparatorChar}folder{Path.DirectorySeparatorChar}file.nupkg";
        Assert.Equal(path, PathHelpers.ConvertToNative(path));
    }

    [Fact]
    public void ConvertToNativeWithAlternateSeparatorConvertsToNative()
    {
        if (Path.DirectorySeparatorChar == '\\')
        {
            Assert.Equal("root\\folder\\file.nupkg", PathHelpers.ConvertToNative("root/folder/file.nupkg"));
        }
        else
        {
            Assert.Equal("root/folder/file.nupkg", PathHelpers.ConvertToNative("root\\folder\\file.nupkg"));
        }
    }
}
