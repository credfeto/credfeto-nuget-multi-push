using System;
using Microsoft.Extensions.Logging;

namespace Credfeto.Package.Push.Services.LoggingExtensions;

internal static partial class PackageUploaderLoggingExtensions
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Failed to upload {package}: {type} {message}")]
    public static partial void FailedToUploadPackage(
        this ILogger<PackageUploader> logger,
        string package,
        string type,
        string message,
        Exception exception
    );

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Retrying transient exception {typeName}, on attempt {retryCount} of {maxRetries}. Current delay is {delay}: {details}"
    )]
    public static partial void TransientException(
        this ILogger<PackageUploader> logger,
        string typeName,
        int retryCount,
        int maxRetries,
        TimeSpan delay,
        string details,
        Exception exception
    );

    public static  void TransientException(
        this ILogger<PackageUploader> logger,
        int retryCount,
        int maxRetries,
        in TimeSpan delay,
        string details,
        Exception exception)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.TransientException(typeName: exception.GetType()
                                                         .Name,
                                      retryCount: retryCount,
                                      maxRetries: maxRetries,
                                      delay: delay,
                                      details: details,
                                      exception: exception);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Uploading {filename} (attempt attempt {attempt} of {maxRetries})"
    )]
    public static partial void UploadingPackage(
        this ILogger<PackageUploader> logger,
        string filename,
        int attempt,
        int maxRetries
    );
}
