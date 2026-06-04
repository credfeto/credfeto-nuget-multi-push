using Credfeto.Package.Push.Configuration;
using Credfeto.Package.Push.Helpers;
using Credfeto.Package.Push.Services;
using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using NuGetILogger = NuGet.Common.ILogger;

namespace Credfeto.Package.Push.Tests.Configuration;

public sealed class ServiceConfigurationTests : DependencyInjectionTestsBase
{
    public ServiceConfigurationTests(ITestOutputHelper output)
        : base(output: output, dependencyInjectionRegistration: Configure) { }

    private static IServiceCollection Configure(IServiceCollection services)
    {
        return services
            .AddServices()
            .AddMockedService<ILogger<NugetForwardingLogger>>()
            .AddMockedService<ILogger<PackageUploader>>()
            .AddMockedService<ILogger<UploadOrchestration>>();
    }

    [Fact]
    public void NuGetILoggerIsRegistered()
    {
        this.RequireService<NuGetILogger>();
    }

    [Fact]
    public void IUploadOrchestrationIsRegistered()
    {
        this.RequireService<IUploadOrchestration>();
    }

    [Fact]
    public void IPackageUploaderIsRegistered()
    {
        this.RequireService<IPackageUploader>();
    }
}
