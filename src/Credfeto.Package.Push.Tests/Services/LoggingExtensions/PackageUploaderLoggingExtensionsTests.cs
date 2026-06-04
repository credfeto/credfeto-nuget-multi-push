using System;
using Credfeto.Package.Push.Services;
using Credfeto.Package.Push.Services.LoggingExtensions;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Credfeto.Package.Push.Tests.Services.LoggingExtensions;

public sealed class PackageUploaderLoggingExtensionsTests : LoggingTestBase
{
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(5);

    public PackageUploaderLoggingExtensionsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void FailedToUploadPackageDoesNotThrow()
    {
        ILogger<PackageUploader> logger = this.GetTypedLogger<PackageUploader>();

        logger.FailedToUploadPackage(
            package: "MyLib.1.0.0.nupkg",
            type: nameof(InvalidOperationException),
            message: "Test upload failure",
            exception: new InvalidOperationException("Test exception")
        );
    }

    [Fact]
    public void TransientExceptionWhenLoggerEnabledForInformationDoesNotThrow()
    {
        ILogger<PackageUploader> logger = this.GetTypedLogger<PackageUploader>();

        logger.TransientException(
            retryCount: 1,
            maxRetries: 5,
            delay: Delay,
            details: "Test details",
            exception: new InvalidOperationException("Test exception")
        );
    }

    [Fact]
    public void TransientExceptionWhenLoggerNotEnabledForInformationDoesNotThrow()
    {
        ILogger<PackageUploader> logger = this.GetTypedLogger<PackageUploader>();

        logger.TransientException(
            retryCount: 2,
            maxRetries: 5,
            delay: Delay,
            details: "Test details for second call",
            exception: new InvalidOperationException("Another test exception")
        );
    }
}
