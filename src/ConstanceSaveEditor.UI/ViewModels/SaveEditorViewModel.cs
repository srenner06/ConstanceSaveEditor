using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConstanceSaveEditor.Core.Backup;
using ConstanceSaveEditor.Core.Models;
using ConstanceSaveEditor.Core.Services;
using ConstanceSaveEditor.UI.ViewModels.Editors;

namespace ConstanceSaveEditor.UI.ViewModels;

public sealed partial class SaveEditorViewModel : ObservableObject
{
    private static readonly JsonSerializerOptions PrettyOptions = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions ParseOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly SaveEditorSession _session;
    private bool _isRefreshing;

    [ObservableProperty]
    private string _jsonText = string.Empty;

    // --- JSON search ---
    [ObservableProperty]
    private string _jsonSearch = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchResultText))]
    private int _searchMatchCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchResultText))]
    private int _searchMatchIndex;

    [ObservableProperty]
    private int _jsonSelectionStart;

    [ObservableProperty]
    private int _jsonSelectionEnd;

    private readonly List<int> _matchPositions = [];

    public string SearchResultText => SearchMatchCount == 0
        ? (string.IsNullOrEmpty(JsonSearch) ? string.Empty : "No results")
        : $"{SearchMatchIndex + 1} / {SearchMatchCount}";

    // --- Editor identity ---
    public bool IsDirty => _session.Document.IsDirty;
    public string? SourcePath => _session.Document.SourcePath;
    public string FileName => Path.GetFileName(_session.Document.SourcePath) ?? "Unsaved";
    public string FormatDisplay => _session.Document.Format.ToString();
    public string LoadedAtDisplay => $"Loaded {_session.Document.LoadedAt:HH:mm:ss}";

    // --- Overview ---
    public string SaveId => _session.Document.Save.Id;
    public int SaveVersion => _session.Document.Save.Version;
    public int WorldDataEntryCount => _session.Document.Save.World.Data.Count;
    public string GameModeDisplay => _session.Document.Save.Modifiers.GameMode == 0 ? "Normal" : $"Custom ({_session.Document.Save.Modifiers.GameMode})";
    public bool Permadeath => _session.Document.Save.Modifiers.Permadeath;
    public bool HasEventLog => _session.Document.EventLog is not null;
    public bool HasMapFog => _session.Document.MapFog is not null;

    public SaveEditorSession Session => _session;
    public PlayerEditorViewModel PlayerEditor { get; }
    public ProgressTabViewModel ProgressTab { get; }
    public BackupsTabViewModel BackupsTab { get; }

    /// <summary>Raised when the user restores a backup and the editor should be replaced.</summary>
    public event Action<BackupEntry>? RestoreRequested;

    public SaveEditorViewModel(SaveEditorSession session)
    {
        _session = session;
        PlayerEditor = new PlayerEditorViewModel(session);
        PlayerEditor.DataModified += OnPlayerDataModified;
        ProgressTab = new ProgressTabViewModel();
        BackupsTab = new BackupsTabViewModel(session);
        BackupsTab.RestoreRequested += entry => RestoreRequested?.Invoke(entry);
        RefreshFromSession();
    }

    partial void OnJsonTextChanged(string value)
    {
        if (!_isRefreshing)
        {
            if (!_session.Document.IsDirty)
            {
                _session.MarkDirty();
                OnPropertyChanged(nameof(IsDirty));
            }
        }
        RefreshSearch();
    }

    partial void OnJsonSearchChanged(string value) => RefreshSearch();

    private void RefreshSearch()
    {
        _matchPositions.Clear();
        var term = JsonSearch;
        if (!string.IsNullOrEmpty(term))
        {
            var text = JsonText;
            var idx = 0;
            while (true)
            {
                var pos = text.IndexOf(term, idx, StringComparison.OrdinalIgnoreCase);
                if (pos < 0)
                    break;
                _matchPositions.Add(pos);
                idx = pos + 1;
            }
        }
        SearchMatchCount = _matchPositions.Count;
        SearchMatchIndex = 0;
        OnPropertyChanged(nameof(SearchResultText));
        NavigateToCurrentMatch();
    }

    private void NavigateToCurrentMatch()
    {
        if (_matchPositions.Count == 0)
        {
            JsonSelectionStart = 0;
            JsonSelectionEnd = 0;
            return;
        }
        var pos = _matchPositions[SearchMatchIndex];
        JsonSelectionStart = pos;
        JsonSelectionEnd = pos + JsonSearch.Length;
    }

    [RelayCommand]
    private void FindNext()
    {
        if (_matchPositions.Count == 0)
            return;
        SearchMatchIndex = (SearchMatchIndex + 1) % _matchPositions.Count;
        OnPropertyChanged(nameof(SearchResultText));
        NavigateToCurrentMatch();
    }

    [RelayCommand]
    private void FindPrevious()
    {
        if (_matchPositions.Count == 0)
            return;
        SearchMatchIndex = (SearchMatchIndex - 1 + _matchPositions.Count) % _matchPositions.Count;
        OnPropertyChanged(nameof(SearchResultText));
        NavigateToCurrentMatch();
    }

    public void RefreshFromSession()
    {
        _isRefreshing = true;
        JsonText = JsonSerializer.Serialize(_session.Document.Save, PrettyOptions);
        _isRefreshing = false;
        PlayerEditor.LoadFromSession();
        ProgressTab.LoadFromSession(_session);
        _ = BackupsTab.LoadAsync();
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(SourcePath));
        OnPropertyChanged(nameof(FileName));
        OnPropertyChanged(nameof(SaveId));
        OnPropertyChanged(nameof(SaveVersion));
        OnPropertyChanged(nameof(WorldDataEntryCount));
        OnPropertyChanged(nameof(GameModeDisplay));
        OnPropertyChanged(nameof(Permadeath));
        OnPropertyChanged(nameof(HasEventLog));
        OnPropertyChanged(nameof(HasMapFog));
    }

    public bool TryApplyJsonText(out string? error)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<SaveData>(JsonText, ParseOptions);
            if (parsed is null)
            {
                error = "Parsed to null.";
                return false;
            }
            _session.Document.Save = parsed;
            error = null;
            return true;
        }
        catch (JsonException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    [RelayCommand]
    private void Revert()
    {
        _session.RevertUnsavedChanges();
        RefreshFromSession();
    }

    private void OnPlayerDataModified()
    {
        // Re-serialize so the JSON tab stays in sync.
        _isRefreshing = true;
        JsonText = JsonSerializer.Serialize(_session.Document.Save, PrettyOptions);
        _isRefreshing = false;

        if (!_session.Document.IsDirty)
        {
            _session.MarkDirty();
            OnPropertyChanged(nameof(IsDirty));
        }
    }
}