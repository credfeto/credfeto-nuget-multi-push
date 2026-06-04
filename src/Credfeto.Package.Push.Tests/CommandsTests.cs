using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Package.Push.Exceptions;
using FunFair.Test.Common;
using NSubstitute;
using Xunit;

namespace Credfeto.Package.Push.Tests;

public sealed class CommandsTests : LoggingFolderCleanupTestBase
{
    private const string SOURCE = "https://test.example";
    private const string API_KEY = "test-key";

    private readonly IUploadOrchestration _uploadOrchestration;

    public CommandsTests(ITestOutputHelper output)
        : base(output)
    {
        this._uploadOrchestration = GetSubstitute<IUploadOrchestration>();
    }

    [Fact]
    public Task UploadPackagesWithEmptyFolderThrowsUploadConfigurationErrorsExceptionAsync()
    {
        Commands commands = new(this._uploadOrchestration);

        return Assert.ThrowsAsync<UploadConfigurationErrorsException>(() =>
            commands.UploadPackagesAsync(source: SOURCE, folder: this.TempFolder, apiKey: API_KEY, symbolSource: null)
        );
    }

    [Fact]
    public async Task UploadPackagesWithSuccessfulResultsCompletesWithoutExceptionAsync()
    {
        string packagePath = Path.Combine(path1: this.TempFolder, path2: "MyLib.1.0.0.nupkg");
        await File.WriteAllTextAsync(
            path: packagePath,
            contents: string.Empty,
            cancellationToken: TestContext.Current.CancellationToken
        );

        this._uploadOrchestration.PushAllAsync(
                source: Arg.Any<string>(),
                symbolSource: Arg.Any<string?>(),
                packages: Arg.Any<IReadOnlyList<string>>(),
                apiKey: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult<IReadOnlyList<(string package, bool success)>>([(packagePath, true)]));

        Commands commands = new(this._uploadOrchestration);

        await commands.UploadPackagesAsync(
            source: SOURCE,
            folder: this.TempFolder,
            apiKey: API_KEY,
            symbolSource: null
        );
    }

    [Fact]
    public async Task UploadPackagesWithFailedResultsThrowsUploadFailedExceptionAsync()
    {
        string packagePath = Path.Combine(path1: this.TempFolder, path2: "MyLib.1.0.0.nupkg");
        await File.WriteAllTextAsync(
            path: packagePath,
            contents: string.Empty,
            cancellationToken: TestContext.Current.CancellationToken
        );

        this._uploadOrchestration.PushAllAsync(
                source: Arg.Any<string>(),
                symbolSource: Arg.Any<string?>(),
                packages: Arg.Any<IReadOnlyList<string>>(),
                apiKey: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult<IReadOnlyList<(string package, bool success)>>([(packagePath, false)]));

        Commands commands = new(this._uploadOrchestration);

        await Assert.ThrowsAsync<UploadFailedException>(() =>
            commands.UploadPackagesAsync(source: SOURCE, folder: this.TempFolder, apiKey: API_KEY, symbolSource: null)
        );
    }

    [Fact]
    public async Task UploadPackagesWithTeamCityVersionSetAndSuccessCompletesWithoutExceptionAsync()
    {
        string packagePath = Path.Combine(path1: this.TempFolder, path2: "MyLib.1.0.0.nupkg");
        await File.WriteAllTextAsync(
            path: packagePath,
            contents: string.Empty,
            cancellationToken: TestContext.Current.CancellationToken
        );

        this._uploadOrchestration.PushAllAsync(
                source: Arg.Any<string>(),
                symbolSource: Arg.Any<string?>(),
                packages: Arg.Any<IReadOnlyList<string>>(),
                apiKey: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult<IReadOnlyList<(string package, bool success)>>([(packagePath, true)]));

        Environment.SetEnvironmentVariable(variable: "TEAMCITY_VERSION", value: "9.1");

        try
        {
            Commands commands = new(this._uploadOrchestration);

            await commands.UploadPackagesAsync(
                source: SOURCE,
                folder: this.TempFolder,
                apiKey: API_KEY,
                symbolSource: null
            );
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: "TEAMCITY_VERSION", value: null);
        }
    }
}
