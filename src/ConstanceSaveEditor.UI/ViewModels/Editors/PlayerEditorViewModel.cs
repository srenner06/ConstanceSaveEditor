using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConstanceSaveEditor.Core.Editing;
using ConstanceSaveEditor.Core.Services;

namespace ConstanceSaveEditor.UI.ViewModels.Editors;

public sealed partial class PlayerEditorViewModel : ObservableObject
{
    private readonly SaveEditorSession _session;

    // --- Currency ---
    [ObservableProperty]
    private decimal? _balance;

    [ObservableProperty]
    private decimal? _totalCollected;

    [ObservableProperty]
    private decimal? _allTimeGlimmer;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    // --- Player stats ---
    [ObservableProperty]
    private int _health;

    [ObservableProperty]
    private double _potionFill;

    // --- Info (read-only) ---
    public string CheckpointDisplay => _session.Document.Save.Player.CheckpointId;

    public string PlayedTimeDisplay
    {
        get
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(_session.Document.Save.Player.PlayedTime);
            return ts.TotalHours >= 1
                ? $"{(int)ts.TotalHours}h {ts.Minutes:D2}m {ts.Seconds:D2}s"
                : $"{ts.Minutes}m {ts.Seconds:D2}s";
        }
    }

    /// <summary>Raised after a successful apply so the parent ViewModel can refresh JSON text and mark dirty.</summary>
    public event Action? DataModified;

    public PlayerEditorViewModel(SaveEditorSession session)
    {
        _session = session;
        LoadFromSession();
    }

    public void LoadFromSession()
    {
        var currency = new CurrencyData(_session.Document.Save.World.Data);
        Balance = currency.Balance;
        TotalCollected = currency.TotalCollected;
        AllTimeGlimmer = currency.AllTimeGlimmer;
        ValidationMessage = string.Empty;

        Health = _session.Document.Save.Player.Health;
        PotionFill = _session.Document.Save.Player.PotionFill;
        OnPropertyChanged(nameof(CheckpointDisplay));
        OnPropertyChanged(nameof(PlayedTimeDisplay));
    }

    [RelayCommand]
    private void ApplyChanges()
    {
        long balance = (long)(Balance ?? 0);
        long totalCollected = (long)(TotalCollected ?? 0);
        long allTimeGlimmer = (long)(AllTimeGlimmer ?? 0);

        var data = new CurrencyData(_session.Document.Save.World.Data);
        bool adjusted = data.Apply(balance, totalCollected, allTimeGlimmer);

        Balance = data.Balance;
        TotalCollected = data.TotalCollected;
        AllTimeGlimmer = data.AllTimeGlimmer;

        ValidationMessage = adjusted
            ? "Lifetime totals were raised to satisfy game constraints."
            : string.Empty;

        DataModified?.Invoke();
    }

    [RelayCommand]
    private void ApplyPlayerStats()
    {
        _session.Document.Save.Player.Health = Health;
        _session.Document.Save.Player.PotionFill = (float)PotionFill;
        DataModified?.Invoke();
    }
}