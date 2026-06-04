using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;

namespace Credfeto.Package.Push;

public interface IPackagePushGateway
{
    Task PushAsync(
        PackageUpdateResource packageUpdateResource,
        IList<string> packagePaths,
        string? symbolSource,
        string apiKey,
        SymbolPackageUpdateResourceV3? symbolPackageUpdateResource
    );
}
