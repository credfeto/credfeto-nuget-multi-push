using System.Collections.Generic;
using System.IO;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Package.Push.Tests;

public sealed class SearcherTests : LoggingFolderCleanupTestBase
{
    public SearcherTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void EmptyFolderReturnsEmptyList()
    {
        IReadOnlyList<string> result = Searcher.FindMatchingPackages(this.TempFolder);

        Assert.Empty(result);
    }

    [Fact]
    public void FolderWithOneNupkgReturnsIt()
    {
        string packagePath = Path.Combine(path1: this.TempFolder, path2: "MyLib.1.0.0.nupkg");
        File.WriteAllText(path: packagePath, contents: string.Empty);

        IReadOnlyList<string> result = Searcher.FindMatchingPackages(this.TempFolder);

        Assert.Single(result);
        Assert.Equal(expected: packagePath, actual: result[0]);
    }

    [Fact]
    public void FolderWithOneSnupkgReturnsIt()
    {
        string symbolPath = Path.Combine(path1: this.TempFolder, path2: "MyLib.1.0.0.snupkg");
        File.WriteAllText(path: symbolPath, contents: string.Empty);

        IReadOnlyList<string> result = Searcher.FindMatchingPackages(this.TempFolder);

        Assert.Single(result);
        Assert.Equal(expected: symbolPath, actual: result[0]);
    }

    [Fact]
    public void FolderWithNupkgAndSnupkgReturnsBoth()
    {
        string packagePath = Path.Combine(path1: this.TempFolder, path2: "MyLib.1.0.0.nupkg");
        string symbolPath = Path.Combine(path1: this.TempFolder, path2: "MyLib.1.0.0.snupkg");
        File.WriteAllText(path: packagePath, contents: string.Empty);
        File.WriteAllText(path: symbolPath, contents: string.Empty);

        IReadOnlyList<string> result = Searcher.FindMatchingPackages(this.TempFolder);

        Assert.Equal(expected: 2, actual: result.Count);
    }
}
