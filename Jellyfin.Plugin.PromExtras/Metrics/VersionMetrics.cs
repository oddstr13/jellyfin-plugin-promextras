using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Jellyfin.Plugin.PromExtras.Metrics;

// dotnet_build_info{version="4.4.1.0",target_framework=".NETCoreApp,Version=v9.0",runtime_version=".NET 9.0.16",os_version="Debian GNU/Linux 13 (trixie)",process_architecture="X64",gc_mode="Workstation"} 1

/// <summary>
/// Library count Metrics.
/// </summary>
public class VersionMetrics : IHostedService
{
    private readonly ILogger<VersionMetrics> _logger;
    private readonly IPluginManager _pluginManager;
    private readonly IApplicationHost _applicationHost;

    private static readonly Gauge _jellyfinVersionInfo = Prometheus.Metrics.CreateGauge("jellyfin_info", "Jellyfin build information", ["id", "name", "version"]);
    private static readonly Gauge _pluginInfo = Prometheus.Metrics.CreateGauge("jellyfin_plugin_info", "Jellyfin plugin information", ["id", "name", "version"]);

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionMetrics"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="pluginManager">Plugin manager.</param>
    /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
    public VersionMetrics(
    ILogger<VersionMetrics> logger,
    IPluginManager pluginManager,
    IApplicationHost applicationHost)
    {
        _logger = logger;
        _pluginManager = pluginManager;
        _applicationHost = applicationHost;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _jellyfinVersionInfo.WithLabels([
            Guid.Parse(_applicationHost.SystemId).ToString("D").ToLowerInvariant(),
            _applicationHost.Name,
            _applicationHost.ApplicationVersion.ToString(4)
        ]).Set(1);

        foreach (var plugin in _pluginManager.Plugins)
        {
            _pluginInfo.WithLabels([
                plugin.Id.ToString("D").ToLowerInvariant(),
                plugin.Name,
                plugin.Version.ToString(4)
            ]).Set(1);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
