using System.Collections.Generic;
using Credfeto.Package.Push.Extensions;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Credfeto.Package.Push.Tests.Extensions;

public sealed class PackageSearchExtensionsTests : TestBase
{
    private const string PACKAGE = "/packages/MyLib.1.0.0.nupkg";
    private const string SYMBOL_NEW = "/packages/MyLib.1.0.0.snupkg";
    private const string SYMBOL_OLD = "/packages/MyLib.1.0.0.symbols.nupkg";

    [Fact]
    public void FindMatchingSymbolPackageWithEmptySymbolListReturnsNull()
    {
        IReadOnlyList<string> symbolPackages = [];

        string? result = symbolPackages.FindMatchingSymbolPackage(package: PACKAGE, logger: NullLogger.Instance);

        Assert.Null(result);
    }

    [Fact]
    public void FindMatchingSymbolPackageWithNewFormatMatchReturnsMatchingPath()
    {
        IReadOnlyList<string> symbolPackages = [SYMBOL_NEW];

        string? result = symbolPackages.FindMatchingSymbolPackage(package: PACKAGE, logger: NullLogger.Instance);

        Assert.Equal(expected: SYMBOL_NEW, actual: result);
    }

    [Fact]
    public void FindMatchingSymbolPackageWithOldFormatMatchReturnsMatchingPath()
    {
        IReadOnlyList<string> symbolPackages = [SYMBOL_OLD];

        string? result = symbolPackages.FindMatchingSymbolPackage(package: PACKAGE, logger: NullLogger.Instance);

        Assert.Equal(expected: SYMBOL_OLD, actual: result);
    }

    [Fact]
    public void FindMatchingSymbolPackageWithBothFormatsReturnsNewFormatFirst()
    {
        IReadOnlyList<string> symbolPackages = [SYMBOL_OLD, SYMBOL_NEW];

        string? result = symbolPackages.FindMatchingSymbolPackage(package: PACKAGE, logger: NullLogger.Instance);

        Assert.Equal(expected: SYMBOL_NEW, actual: result);
    }

    [Fact]
    public void FindMatchingSymbolPackageWithNoMatchReturnsNull()
    {
        IReadOnlyList<string> symbolPackages = ["/packages/OtherLib.1.0.0.snupkg"];

        string? result = symbolPackages.FindMatchingSymbolPackage(package: PACKAGE, logger: NullLogger.Instance);

        Assert.Null(result);
    }

    [Fact]
    public void GetNewSymbolsWithEmptyListReturnsEmpty()
    {
        IReadOnlyList<string> symbolPackages = [];

        IReadOnlyList<string> result = symbolPackages.GetNewSymbols();

        Assert.Empty(result);
    }

    [Fact]
    public void GetNewSymbolsWithMixedListReturnsOnlySnupkg()
    {
        IReadOnlyList<string> symbolPackages = [PACKAGE, SYMBOL_NEW, SYMBOL_OLD];

        IReadOnlyList<string> result = symbolPackages.GetNewSymbols();

        Assert.Equal(expected: [SYMBOL_NEW], actual: result);
    }

    [Fact]
    public void GetOldSymbolsWithEmptyListReturnsEmpty()
    {
        IReadOnlyList<string> symbolPackages = [];

        IReadOnlyList<string> result = symbolPackages.GetOldSymbols();

        Assert.Empty(result);
    }

    [Fact]
    public void GetOldSymbolsWithMixedListReturnsOnlySymbolsNupkg()
    {
        IReadOnlyList<string> symbolPackages = [PACKAGE, SYMBOL_NEW, SYMBOL_OLD];

        IReadOnlyList<string> result = symbolPackages.GetOldSymbols();

        Assert.Equal(expected: [SYMBOL_OLD], actual: result);
    }
}
