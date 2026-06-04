using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using ILogger = NuGet.Common.ILogger;

namespace Credfeto.Package.Push.Services;

public sealed class NuGetPackagePushGateway : IPackagePushGateway
{
    private readonly ILogger _nugetLogger;

    public NuGetPackagePushGateway(ILogger nugetLogger)
    {
        this._nugetLogger = nugetLogger;
    }

    public Task PushAsync(
        PackageUpdateResource packageUpdateResource,
        IList<string> packagePaths,
        string? symbolSource,
        string apiKey,
        SymbolPackageUpdateResourceV3? symbolPackageUpdateResource
    )
    {
        return packageUpdateResource.Push(
            packagePaths: packagePaths,
            symbolSource: symbolSource,
            timeoutInSecond: 800,
            disableBuffering: false,
            getApiKey: _ => apiKey,
            getSymbolApiKey: _ => apiKey,
            noServiceEndpoint: false,
            skipDuplicate: true,
            symbolPackageUpdateResource: symbolPackageUpdateResource,
            log: this._nugetLogger
        );
    }
}
