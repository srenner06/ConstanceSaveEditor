using System.Text.Json;
using System.Text.Json.Nodes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConstanceSaveEditor.Core.Services;

namespace ConstanceSaveEditor.UI.ViewModels;

public sealed partial class SaveEditorViewModel : ObservableObject
{
    private static readonly JsonSerializerOptions PrettyOptions = new() { WriteIndented = true };

    private readonly SaveEditorSession _session;
    private bool _isRefreshing;

    [ObservableProperty]
    private string _jsonText = string.Empty;

    public bool IsDirty => _session.Document.IsDirty;
    public string? SourcePath => _session.Document.SourcePath;
    public string FileName => Path.GetFileName(_session.Document.SourcePath) ?? "Unsaved";
    public string FormatDisplay => _session.Document.Format.ToString();
    public string LoadedAtDisplay => $"Loaded {_session.Document.LoadedAt:HH:mm:ss}";

    public SaveEditorSession Session => _session;

    public SaveEditorViewModel(SaveEditorSession session)
    {
        _session = session;
        RefreshFromSession();
    }

    partial void OnJsonTextChanged(string value)
    {
        if (_isRefreshing)
            return;
        if (!_session.Document.IsDirty)
        {
            _session.MarkDirty();
            OnPropertyChanged(nameof(IsDirty));
        }
    }

    public void RefreshFromSession()
    {
        _isRefreshing = true;
        JsonText = _session.Document.SaveJson.ToJsonString(PrettyOptions);
        _isRefreshing = false;
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(SourcePath));
        OnPropertyChanged(nameof(FileName));
    }

    public bool TryApplyJsonText(out string? error)
    {
        try
        {
            JsonNode? node = JsonNode.Parse(JsonText);
            if (node is null)
            {
                error = "Parsed to null.";
                return false;
            }
            _session.Document.SaveJson = node;
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
}