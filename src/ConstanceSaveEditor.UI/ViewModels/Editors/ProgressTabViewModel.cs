using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using ConstanceSaveEditor.Core.Editing;
using ConstanceSaveEditor.Core.Services;

namespace ConstanceSaveEditor.UI.ViewModels.Editors;

public sealed record RegionRow(string Name, string Counts, int Percent);

public sealed record MissingItem(string Category, string Region, string Room);

public sealed partial class ProgressTabViewModel : ObservableObject
{
    // --- Collectibles ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeartPiecesDisplay))]
    private int _heartPieces;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaintFlasksDisplay))]
    private int _paintFlasks;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PolaroidsDisplay))]
    private int _polaroids;

    [ObservableProperty]
    private int _erasers;

    [ObservableProperty]
    private int _lightStones;

    [ObservableProperty]
    private IReadOnlyList<RegionRow> _regions = [];

    [ObservableProperty]
    private IReadOnlyList<MissingItem> _missingItems = [];

    [ObservableProperty]
    private string _completionSummary = string.Empty;

    // --- Inspirations ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InspirationsSummary))]
    private int _inspirationsCollected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InspirationsSummary))]
    private int _inspirationsUpgraded;

    [ObservableProperty]
    private IReadOnlyList<InspirationEntry> _inspirations = [];

    // --- Stats ---
    [ObservableProperty]
    private int _enemiesKilled;

    [ObservableProperty]
    private int _dashCount;

    [ObservableProperty]
    private int _brushHits;

    [ObservableProperty]
    private int _splooshCount;

    [ObservableProperty]
    private int _receivedDamage;

    [ObservableProperty]
    private int _timesElectrocuted;

    [ObservableProperty]
    private int _timesPersevered;

    [ObservableProperty]
    private int _gameOversByCorruption;

    [ObservableProperty]
    private int _snapshotsTaken;

    [ObservableProperty]
    private int _clonesUsed;

    [ObservableProperty]
    private int _shrineVisits;

    [ObservableProperty]
    private int _timesPetCat;

    // --- Computed display ---
    public string HeartPiecesDisplay => $"{HeartPieces} / {ProgressData.TotalHeartPieces}";
    public string PaintFlasksDisplay => $"{PaintFlasks} / {ProgressData.TotalPaintFlasks}";
    public string PolaroidsDisplay => $"{Polaroids} / {ProgressData.TotalPolaroids}";
    public string InspirationsSummary =>
        $"{InspirationsCollected} / {ProgressData.TotalInspirations} collected, {InspirationsUpgraded} upgraded";

    public void LoadFromSession(SaveEditorSession session)
    {
        var progress = new ProgressData(session.Document.Save.World.Data);

        HeartPieces = progress.HeartPieces;
        PaintFlasks = progress.PaintFlasks;
        Polaroids = progress.Polaroids;
        Erasers = progress.Erasers;
        LightStones = progress.LightStones;

        var report = CompletionCatalog.Evaluate(session.Document.Save.World.Data);
        Regions = report.Regions
            .Select(r => new RegionRow(Localized("Region", r.Region), r.Counts, r.Percent))
            .ToList();
        CompletionSummary = $"{report.Done} / {report.Total}  ({report.Percent}%)";
        MissingItems = report.Missing
            .OrderBy(e => e.Region, StringComparer.Ordinal)
            .ThenBy(e => e.Level, StringComparer.Ordinal)
            .ThenBy(e => e.Category, StringComparer.Ordinal)
            .Select(e => new MissingItem(Localized("Category", e.Category), Localized("Region", e.Region), ShortRoom(e.Level)))
            .ToList();

        Inspirations = progress.Inspirations;
        InspirationsCollected = progress.Inspirations.Count(e => e.IsCollected);
        InspirationsUpgraded = progress.Inspirations.Count(e => e.IsUpgraded);

        EnemiesKilled = progress.EnemiesKilled;
        DashCount = progress.DashCount;
        BrushHits = progress.BrushHits;
        SplooshCount = progress.SplooshCount;
        ReceivedDamage = progress.ReceivedDamage;
        TimesElectrocuted = progress.TimesElectrocuted;
        TimesPersevered = progress.TimesPersevered;
        GameOversByCorruption = progress.GameOversByCorruption;
        SnapshotsTaken = progress.SnapshotsTaken;
        ClonesUsed = progress.ClonesUsed;
        ShrineVisits = progress.ShrineVisits;
        TimesPetCat = progress.TimespetCat;
    }

    private static string ShortRoom(string level) =>
        level.StartsWith("Prod_", StringComparison.Ordinal) ? level[5..] : level;

    // shortcut: labels are resolved once per load, so a language switch only shows through
    // after the save is reopened. Make them DynamicResource bindings if that bites.
    private static string Localized(string group, string key) =>
        Application.Current?.TryGetResource($"Progress.Missing.{group}.{key}", null, out var label) == true
            && label is string text
            ? text
            : key;
}