using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Credfeto.Package.Push.Services;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Xunit;

namespace Credfeto.Package.Push.Tests.Services;

public sealed class UploadOrchestrationTests : LoggingTestBase
{
    private const string PRIMARY_SOURCE = "https://test.example.com";
    private const string SYMBOL_SOURCE = "https://symbols.example.com";
    private const string API_KEY = "test-api-key";
    private const string PACKAGE = "/pkgs/MyLib.1.0.0.nupkg";
    private const string SYMBOL_NEW = "/pkgs/MyLib.1.0.0.snupkg";
    private const string SYMBOL_OLD = "/pkgs/MyLib.1.0.0.symbols.nupkg";

    public UploadOrchestrationTests(ITestOutputHelper output)
        : base(output) { }

    private static SourceRepository CreateSourceRepository(string source = PRIMARY_SOURCE)
    {
        PackageSource packageSource = new(source);

        return new SourceRepository(source: packageSource, [.. Repository.Provider.GetCoreV3()]);
    }

    private static Task<PackageUpdateResource> CreatePackageUpdateResourceAsync(string source = PRIMARY_SOURCE)
    {
        return CreateSourceRepository(source)
            .GetResourceAsync<PackageUpdateResource>(TestContext.Current.CancellationToken);
    }

    private static async Task<SymbolPackageUpdateResourceV3> CreateSymbolPackageUpdateResourceAsync(
        string source = SYMBOL_SOURCE
    )
    {
        SourceRepository repo = CreateSourceRepository(source);
        HttpSourceResource httpSourceResource = await repo.GetResourceAsync<HttpSourceResource>(
            TestContext.Current.CancellationToken
        );

        return new SymbolPackageUpdateResourceV3(source: source, httpSource: httpSourceResource.HttpSource);
    }

    private UploadOrchestration CreateSut(IPackageUploader packageUploader)
    {
        ILogger<UploadOrchestration> logger = this.GetTypedLogger<UploadOrchestration>();

        return new UploadOrchestration(packageUploader: packageUploader, logger: logger);
    }

    private static IPackageUploader CreateMockedUploader()
    {
        IPackageUploader packageUploader = GetSubstitute<IPackageUploader>();
        packageUploader
            .PushOnePackageAsync(
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<string>>(),
                Arg.Any<PackageUpdateResource>(),
                Arg.Any<string>(),
                Arg.Any<SymbolPackageUpdateResourceV3?>()
            )
            .Returns(callInfo => Task.FromResult((callInfo.ArgAt<string>(0), true)));

        return packageUploader;
    }

    [Fact]
    public async Task BuildUploadTasksWithNoSymbolsCallsUploaderForEachNonSymbolPackageAsync()
    {
        IPackageUploader packageUploader = CreateMockedUploader();
        UploadOrchestration sut = this.CreateSut(packageUploader);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();

        IEnumerable<Task<(string package, bool success)>> tasks = sut.BuildUploadTasks(
            symbolSourceRepository: null,
            symbolPackageUpdateResource: null,
            symbolPackageUpdateResourceAsPackage: null,
            nonSymbolPackages: [PACKAGE],
            symbolPackages: [],
            apiKey: API_KEY,
            packageUpdateResource: packageUpdateResource
        );

        (string package, bool success)[] results = await Task.WhenAll(tasks);
        (string resultPackage, bool resultSuccess) = Assert.Single(results);
        Assert.Equal(expected: PACKAGE, actual: resultPackage);
        Assert.True(resultSuccess, "Expected upload to succeed");
    }

    [Fact]
    public async Task BuildUploadTasksWithSeparateSymbolRepoAndPackageApiUploadsSymbolsSeparatelyAsync()
    {
        IPackageUploader packageUploader = CreateMockedUploader();
        UploadOrchestration sut = this.CreateSut(packageUploader);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();
        PackageUpdateResource symbolPackageUpdateResourceAsPackage = await CreatePackageUpdateResourceAsync(
            SYMBOL_SOURCE
        );
        SourceRepository symbolSourceRepository = CreateSourceRepository(SYMBOL_SOURCE);

        IEnumerable<Task<(string package, bool success)>> tasks = sut.BuildUploadTasks(
            symbolSourceRepository: symbolSourceRepository,
            symbolPackageUpdateResource: null,
            symbolPackageUpdateResourceAsPackage: symbolPackageUpdateResourceAsPackage,
            nonSymbolPackages: [PACKAGE],
            symbolPackages: [SYMBOL_NEW],
            apiKey: API_KEY,
            packageUpdateResource: packageUpdateResource
        );

        (string package, bool success)[] results = await Task.WhenAll(tasks);
        Assert.Equal(expected: 2, actual: results.Length);
    }

    [Fact]
    public async Task BuildUploadTasksWithSeparateSymbolRepoButNoPackageApiUploadsAllToPrimaryAsync()
    {
        IPackageUploader packageUploader = CreateMockedUploader();
        UploadOrchestration sut = this.CreateSut(packageUploader);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();
        SourceRepository symbolSourceRepository = CreateSourceRepository(SYMBOL_SOURCE);

        IEnumerable<Task<(string package, bool success)>> tasks = sut.BuildUploadTasks(
            symbolSourceRepository: symbolSourceRepository,
            symbolPackageUpdateResource: null,
            symbolPackageUpdateResourceAsPackage: null,
            nonSymbolPackages: [PACKAGE],
            symbolPackages: [SYMBOL_NEW],
            apiKey: API_KEY,
            packageUpdateResource: packageUpdateResource
        );

        (string package, bool success)[] results = await Task.WhenAll(tasks);
        Assert.Equal(expected: 2, actual: results.Length);
    }

    [Fact]
    public async Task BuildUploadTasksWithSameSymbolRepoAndOldSymbolsOnlyUsesSymbolApiAsync()
    {
        IPackageUploader packageUploader = CreateMockedUploader();
        UploadOrchestration sut = this.CreateSut(packageUploader);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();
        SymbolPackageUpdateResourceV3 symbolPackageUpdateResource = await CreateSymbolPackageUpdateResourceAsync();

        IEnumerable<Task<(string package, bool success)>> tasks = sut.BuildUploadTasks(
            symbolSourceRepository: null,
            symbolPackageUpdateResource: symbolPackageUpdateResource,
            symbolPackageUpdateResourceAsPackage: null,
            nonSymbolPackages: [PACKAGE],
            symbolPackages: [SYMBOL_OLD],
            apiKey: API_KEY,
            packageUpdateResource: packageUpdateResource
        );

        (string package, bool success)[] results = await Task.WhenAll(tasks);
        Assert.Single(results);
    }

    [Fact]
    public async Task BuildUploadTasksWithSameSymbolRepoAndNewSymbolsOnlyUploadsAllToPrimaryAsync()
    {
        IPackageUploader packageUploader = CreateMockedUploader();
        UploadOrchestration sut = this.CreateSut(packageUploader);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();
        SymbolPackageUpdateResourceV3 symbolPackageUpdateResource = await CreateSymbolPackageUpdateResourceAsync();

        IEnumerable<Task<(string package, bool success)>> tasks = sut.BuildUploadTasks(
            symbolSourceRepository: null,
            symbolPackageUpdateResource: symbolPackageUpdateResource,
            symbolPackageUpdateResourceAsPackage: null,
            nonSymbolPackages: [PACKAGE],
            symbolPackages: [SYMBOL_NEW],
            apiKey: API_KEY,
            packageUpdateResource: packageUpdateResource
        );

        (string package, bool success)[] results = await Task.WhenAll(tasks);
        Assert.Equal(expected: 2, actual: results.Length);
    }

    [Fact]
    public async Task BuildUploadTasksWithSameSymbolRepoAndMixedSymbolsRoutesOldViaSymbolApiAndNewToPrimaryAsync()
    {
        IPackageUploader packageUploader = CreateMockedUploader();
        UploadOrchestration sut = this.CreateSut(packageUploader);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();
        SymbolPackageUpdateResourceV3 symbolPackageUpdateResource = await CreateSymbolPackageUpdateResourceAsync();

        IEnumerable<Task<(string package, bool success)>> tasks = sut.BuildUploadTasks(
            symbolSourceRepository: null,
            symbolPackageUpdateResource: symbolPackageUpdateResource,
            symbolPackageUpdateResourceAsPackage: null,
            nonSymbolPackages: [PACKAGE],
            symbolPackages: [SYMBOL_OLD, SYMBOL_NEW],
            apiKey: API_KEY,
            packageUpdateResource: packageUpdateResource
        );

        (string package, bool success)[] results = await Task.WhenAll(tasks);
        Assert.Equal(expected: 2, actual: results.Length);
    }

    [Fact]
    public async Task BuildUploadTasksWithNoSymbolRepoAndSymbolPackagesUploadsAllToPrimaryAsync()
    {
        IPackageUploader packageUploader = CreateMockedUploader();
        UploadOrchestration sut = this.CreateSut(packageUploader);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();

        IEnumerable<Task<(string package, bool success)>> tasks = sut.BuildUploadTasks(
            symbolSourceRepository: null,
            symbolPackageUpdateResource: null,
            symbolPackageUpdateResourceAsPackage: null,
            nonSymbolPackages: [PACKAGE],
            symbolPackages: [SYMBOL_NEW],
            apiKey: API_KEY,
            packageUpdateResource: packageUpdateResource
        );

        (string package, bool success)[] results = await Task.WhenAll(tasks);
        Assert.Equal(expected: 2, actual: results.Length);
    }
}
