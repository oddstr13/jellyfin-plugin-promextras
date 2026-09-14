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

    private static readonly Gauge _albumCount = Prometheus.Metrics.CreateGauge("jellyfin_library_albums_total", "Number of albums in library");
    private static readonly Gauge _songCount = Prometheus.Metrics.CreateGauge("jellyfin_library_songs_total", "Number of songs in library");
    private static readonly Gauge _serieCount = Prometheus.Metrics.CreateGauge("jellyfin_library_series_total", "Number of series in library");
    private static readonly Gauge _episodeCount = Prometheus.Metrics.CreateGauge("jellyfin_library_episodes_total", "Number of episodes in library");
    private static readonly Gauge _movieCount = Prometheus.Metrics.CreateGauge("jellyfin_library_movies_total", "Number of movies in library");
    private static readonly Gauge _musicVideoCount = Prometheus.Metrics.CreateGauge("jellyfin_library_musicvideos_total", "Number of musicvideos in library");
    private static readonly Gauge _bookCount = Prometheus.Metrics.CreateGauge("jellyfin_library_books_total", "Number of books in library");
    private static readonly Gauge _boxSetCount = Prometheus.Metrics.CreateGauge("jellyfin_library_boxsets_total", "Number of boxsets in library");

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
        _albumCount.Set(GetCount(BaseItemKind.MusicAlbum));
        _songCount.Set(GetCount(BaseItemKind.Audio));
        _serieCount.Set(GetCount(BaseItemKind.Series));
        _episodeCount.Set(GetCount(BaseItemKind.Episode));
        _movieCount.Set(GetCount(BaseItemKind.Movie));
        _musicVideoCount.Set(GetCount(BaseItemKind.MusicVideo));
        _bookCount.Set(GetCount(BaseItemKind.Book));
        _boxSetCount.Set(GetCount(BaseItemKind.BoxSet));
    }

    private int GetCount(BaseItemKind itemKind)
    {
        var query = new InternalItemsQuery()
        {
            IncludeItemTypes = [itemKind],
            Limit = 0,
            Recursive = true,
            IsVirtualItem = false,
            IsFavorite = null,
            DtoOptions = new DtoOptions(false)
            {
                EnableImages = false
            }
        };

        return _libraryManager.GetItemsResult(query).TotalRecordCount;
    }
}
