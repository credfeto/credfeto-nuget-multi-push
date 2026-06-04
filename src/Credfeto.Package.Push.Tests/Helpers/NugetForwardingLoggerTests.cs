using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Credfeto.Package.Push.Helpers;
using FunFair.Test.Common;
using NSubstitute;
using NuGet.Common;
using Xunit;
using LogLevel = NuGet.Common.LogLevel;

namespace Credfeto.Package.Push.Tests.Helpers;

public sealed class NugetForwardingLoggerTests : LoggingTestBase
{
    public NugetForwardingLoggerTests(ITestOutputHelper output)
        : base(output) { }

    private NugetForwardingLogger CreateSut()
    {
        return new NugetForwardingLogger(this.GetTypedLogger<NugetForwardingLogger>());
    }

    [Fact]
    public void LogDebugDoesNotThrow()
    {
        NugetForwardingLogger sut = this.CreateSut();
        sut.LogDebug("Debug message");
    }

    [Fact]
    public void LogVerboseDoesNotThrow()
    {
        NugetForwardingLogger sut = this.CreateSut();
        sut.LogVerbose("Verbose message");
    }

    [Fact]
    public void LogInformationDoesNotThrow()
    {
        NugetForwardingLogger sut = this.CreateSut();
        sut.LogInformation("Information message");
    }

    [Fact]
    public void LogMinimalDoesNotThrow()
    {
        NugetForwardingLogger sut = this.CreateSut();
        sut.LogMinimal("Minimal message");
    }

    [Fact]
    public void LogWarningDoesNotThrow()
    {
        NugetForwardingLogger sut = this.CreateSut();
        sut.LogWarning("Warning message");
    }

    [Fact]
    public void LogErrorDoesNotThrow()
    {
        NugetForwardingLogger sut = this.CreateSut();
        sut.LogError("Error message");
    }

    [Fact]
    public void LogInformationSummaryDoesNotThrow()
    {
        NugetForwardingLogger sut = this.CreateSut();
        sut.LogInformationSummary("Information summary message");
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Verbose)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Minimal)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void LogWithValidLevelDoesNotThrow(LogLevel level)
    {
        NugetForwardingLogger sut = this.CreateSut();
        sut.Log(level: level, data: "Test message");
    }

    [Fact]
    public void LogWithInvalidLevelThrowsArgumentOutOfRangeException()
    {
        NugetForwardingLogger sut = this.CreateSut();
        bool exceptionWasThrown = false;

        try
        {
            sut.Log(level: (LogLevel)999, data: "Test message");
        }
        catch (ArgumentOutOfRangeException)
        {
            exceptionWasThrown = true;
        }

        Assert.True(exceptionWasThrown, "Expected ArgumentOutOfRangeException to be thrown for unknown log level");
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Verbose)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Minimal)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public async Task LogAsyncWithValidLevelDoesNotThrowAsync(LogLevel level)
    {
        NugetForwardingLogger sut = this.CreateSut();
        Task result = sut.LogAsync(level: level, data: "Test message");

        await result;

        Assert.True(result.IsCompleted, "Task returned by LogAsync should be completed");
    }

    [Fact]
    public Task LogAsyncWithUnknownLevelThrowsUnreachableExceptionAsync()
    {
        NugetForwardingLogger sut = this.CreateSut();

        return Assert.ThrowsAsync<UnreachableException>(() => sut.LogAsync(level: (LogLevel)999, data: "Test message"));
    }

    [Fact]
    public void LogWithILogMessageDoesNotThrow()
    {
        NugetForwardingLogger sut = this.CreateSut();
        ILogMessage message = GetSubstitute<ILogMessage>();
        message.Level.Returns(LogLevel.Information);
        message.Message.Returns("Test message");

        sut.Log(message);
    }

    [Fact]
    public async Task LogAsyncWithILogMessageDoesNotThrowAsync()
    {
        NugetForwardingLogger sut = this.CreateSut();
        ILogMessage message = GetSubstitute<ILogMessage>();
        message.Level.Returns(LogLevel.Information);
        message.Message.Returns("Test message");

        Task result = sut.LogAsync(message);

        await result;

        Assert.True(result.IsCompleted, "Task returned by LogAsync should be completed");
    }
}
