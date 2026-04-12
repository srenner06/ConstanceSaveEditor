using CommunityToolkit.Mvvm.ComponentModel;
using ConstanceSaveEditor.Core.Editing;
using ConstanceSaveEditor.Core.Services;

namespace ConstanceSaveEditor.UI.ViewModels.Editors;

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
}