using Jellyfin.Plugin.PromExtras.Metrics;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.PromExtras;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddHostedService<LibraryMetrics>();
        serviceCollection.AddHostedService<VersionMetrics>();
    }
}
