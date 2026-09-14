using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Jellyfin.Plugin.PromExtras.Metrics;

/// <summary>
/// Library count Metrics.
/// </summary>
public class LibraryMetrics : IHostedService
{
    private readonly ILogger<LibraryMetrics> _logger;
    private readonly ILibraryManager _libraryManager;

    private static readonly Gauge _itemCount = Prometheus.Metrics.CreateGauge("jellyfin_library_total", "Number of type library items", ["type"]);

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryMetrics"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    public LibraryMetrics(
    ILogger<LibraryMetrics> logger,
    ILibraryManager libraryManager)
    {
        _logger = logger;
        _libraryManager = libraryManager;

        UpdateValues();
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemAdded += OnLibraryChange;
        _libraryManager.ItemRemoved += OnLibraryChange;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemAdded -= OnLibraryChange;
        _libraryManager.ItemRemoved -= OnLibraryChange;
        return Task.CompletedTask;
    }

    private void OnLibraryChange(object? sender, ItemChangeEventArgs e)
    {
        UpdateValues();
    }

    private void UpdateValues()
    {
        foreach (var kind in Enum.GetValues<BaseItemKind>())
        {
            var value = GetCount(kind);
            if (value != 0)
            {
                _itemCount.WithLabels(kind.ToString()).Set(value);
            }
        }
    }

    private int GetCount(BaseItemKind itemKind)
    {
        var query = new InternalItemsQuery()
        {
            IncludeItemTypes = [itemKind],
            Limit = 0,
            Recursive = true,
            IsVirtualItem = false,
            DtoOptions = new DtoOptions(false)
            {
                EnableImages = false
            }
        };

        return _libraryManager.GetItemsResult(query).TotalRecordCount;
    }
}
