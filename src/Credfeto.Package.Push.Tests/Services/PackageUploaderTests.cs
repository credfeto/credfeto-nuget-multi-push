using System;
using System.Collections.Generic;
using System.IO;
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

public sealed class PackageUploaderTests : LoggingTestBase
{
    private const string PACKAGE = "/pkgs/MyLib.1.0.0.nupkg";
    private const string SYMBOL_NEW = "/pkgs/MyLib.1.0.0.snupkg";
    private const string API_KEY = "test-api-key";

    public PackageUploaderTests(ITestOutputHelper output)
        : base(output) { }

    private static SourceRepository CreateSourceRepository()
    {
        PackageSource packageSource = new("https://test.example.com");

        return new SourceRepository(source: packageSource, [.. Repository.Provider.GetCoreV3()]);
    }

    private static Task<PackageUpdateResource> CreatePackageUpdateResourceAsync()
    {
        return CreateSourceRepository().GetResourceAsync<PackageUpdateResource>(TestContext.Current.CancellationToken);
    }

    private PackageUploader CreateSut(IPackagePushGateway packagePushGateway)
    {
        ILogger<PackageUploader> logger = this.GetTypedLogger<PackageUploader>();

        return new PackageUploader(packagePushGateway: packagePushGateway, logger: logger);
    }

    [Fact]
    public async Task PushOnePackageWithEmptySymbolListAndGatewaySucceedsReturnsTrueAsync()
    {
        IPackagePushGateway packagePushGateway = GetSubstitute<IPackagePushGateway>();
        packagePushGateway
            .PushAsync(
                Arg.Any<PackageUpdateResource>(),
                Arg.Any<IList<string>>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<SymbolPackageUpdateResourceV3?>()
            )
            .Returns(Task.CompletedTask);

        PackageUploader sut = this.CreateSut(packagePushGateway);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();

        (string package, bool success) = await sut.PushOnePackageAsync(
            package: PACKAGE,
            symbolPackages: [],
            packageUpdateResource: packageUpdateResource,
            apiKey: API_KEY,
            symbolPackageUpdateResource: null
        );

        Assert.Equal(expected: PACKAGE, actual: package);
        Assert.True(success, "Expected push to succeed");
    }

    [Fact]
    public async Task PushOnePackageWithNonTransientExceptionReturnsFalseAsync()
    {
        IPackagePushGateway packagePushGateway = GetSubstitute<IPackagePushGateway>();
        packagePushGateway
            .PushAsync(
                Arg.Any<PackageUpdateResource>(),
                Arg.Any<IList<string>>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<SymbolPackageUpdateResourceV3?>()
            )
            .Returns(callInfo => throw new InvalidOperationException("non-transient error"));

        PackageUploader sut = this.CreateSut(packagePushGateway);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();

        (string package, bool success) = await sut.PushOnePackageAsync(
            package: PACKAGE,
            symbolPackages: [],
            packageUpdateResource: packageUpdateResource,
            apiKey: API_KEY,
            symbolPackageUpdateResource: null
        );

        Assert.Equal(expected: PACKAGE, actual: package);
        Assert.False(success, "Expected push to fail for non-transient exception");
    }

    [Fact]
    public async Task PushOnePackageWithTransientExceptionThenSuccessReturnsTrueAsync()
    {
        IPackagePushGateway packagePushGateway = GetSubstitute<IPackagePushGateway>();
        int callCount = 0;
        packagePushGateway
            .PushAsync(
                Arg.Any<PackageUpdateResource>(),
                Arg.Any<IList<string>>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<SymbolPackageUpdateResourceV3?>()
            )
            .Returns(callInfo =>
            {
                if (++callCount == 1)
                {
                    throw new IOException("transient error");
                }

                return Task.CompletedTask;
            });

        PackageUploader sut = this.CreateSut(packagePushGateway);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();

        (string package, bool success) = await sut.PushOnePackageAsync(
            package: PACKAGE,
            symbolPackages: [],
            packageUpdateResource: packageUpdateResource,
            apiKey: API_KEY,
            symbolPackageUpdateResource: null
        );

        Assert.Equal(expected: PACKAGE, actual: package);
        Assert.True(success, "Expected push to succeed after retry");
        Assert.True(callCount >= 2, "Expected gateway to be called at least twice due to retry");
    }

    [Fact]
    public async Task PushOnePackageWithMatchingSymbolPackagePassesSymbolSourceToGatewayAsync()
    {
        IPackagePushGateway packagePushGateway = GetSubstitute<IPackagePushGateway>();
        string? capturedSymbolSource = null;
        packagePushGateway
            .PushAsync(
                Arg.Any<PackageUpdateResource>(),
                Arg.Any<IList<string>>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<SymbolPackageUpdateResourceV3?>()
            )
            .Returns(callInfo =>
            {
                capturedSymbolSource = callInfo.ArgAt<string?>(2);

                return Task.CompletedTask;
            });

        PackageUploader sut = this.CreateSut(packagePushGateway);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();

        (string package, bool success) = await sut.PushOnePackageAsync(
            package: PACKAGE,
            symbolPackages: [SYMBOL_NEW],
            packageUpdateResource: packageUpdateResource,
            apiKey: API_KEY,
            symbolPackageUpdateResource: null
        );

        Assert.Equal(expected: PACKAGE, actual: package);
        Assert.True(success, "Expected push to succeed");
        Assert.Equal(expected: SYMBOL_NEW, actual: capturedSymbolSource);
    }

    [Fact]
    public async Task PushOnePackageWithAllTransientExceptionsExhaustingRetriesReturnsFalseAsync()
    {
        IPackagePushGateway packagePushGateway = GetSubstitute<IPackagePushGateway>();
        packagePushGateway
            .PushAsync(
                Arg.Any<PackageUpdateResource>(),
                Arg.Any<IList<string>>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<SymbolPackageUpdateResourceV3?>()
            )
            .Returns(callInfo => throw new IOException("always transient"));

        PackageUploader sut = this.CreateSut(packagePushGateway);
        PackageUpdateResource packageUpdateResource = await CreatePackageUpdateResourceAsync();

        (string package, bool success) = await sut.PushOnePackageAsync(
            package: PACKAGE,
            symbolPackages: [],
            packageUpdateResource: packageUpdateResource,
            apiKey: API_KEY,
            symbolPackageUpdateResource: null
        );

        Assert.Equal(expected: PACKAGE, actual: package);
        Assert.False(success, "Expected push to fail after exhausting all retries");
    }
}
