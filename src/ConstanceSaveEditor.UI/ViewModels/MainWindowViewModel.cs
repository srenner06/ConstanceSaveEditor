using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConstanceSaveEditor.Core.Abstractions;

namespace ConstanceSaveEditor.UI.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly ISaveFileService _fileService;
    private readonly Services.IDialogService _dialogService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOpenFile), nameof(NoFileOpen), nameof(WindowTitle), nameof(FileStatusText))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(SaveAsCommand), nameof(CloseCommand))]
    private SaveEditorViewModel? _currentEditor;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public bool HasOpenFile => CurrentEditor is not null;
    public bool NoFileOpen => CurrentEditor is null;

    public string WindowTitle
    {
        get
        {
            if (CurrentEditor is null)
                return "Constance Save Editor";
            string dirty = CurrentEditor.IsDirty ? " *" : string.Empty;
            return $"Constance Save Editor \u2014 {CurrentEditor.FileName}{dirty}";
        }
    }

    public string FileStatusText =>
        CurrentEditor?.SourcePath ?? "No file open";

    public MainWindowViewModel(ISaveFileService fileService, Services.IDialogService dialogService)
    {
        _fileService = fileService;
        _dialogService = dialogService;
    }

    partial void OnCurrentEditorChanged(SaveEditorViewModel? oldValue, SaveEditorViewModel? newValue)
    {
        if (oldValue is not null)
            oldValue.PropertyChanged -= OnEditorPropertyChanged;
        if (newValue is not null)
            newValue.PropertyChanged += OnEditorPropertyChanged;
    }

    private void OnEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SaveEditorViewModel.IsDirty))
        {
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(FileStatusText));
        }
    }

    [RelayCommand]
    private async Task OpenAsync()
    {
        string? path = await _dialogService.OpenSaveFileAsync();
        if (path is null)
            return;
        try
        {
            var session = await _fileService.OpenAsync(path);
            CurrentEditor = new SaveEditorViewModel(session);
            StatusMessage = string.Empty;
        }
        catch (IOException ex)
        {
            StatusMessage = $"Cannot open file: {ex.Message}";
        }
        catch (InvalidDataException ex)
        {
            StatusMessage = $"Invalid save file: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(HasOpenFile))]
    private async Task SaveAsync()
    {
        if (CurrentEditor is null)
            return;
        if (!CurrentEditor.TryApplyJsonText(out string? parseError))
        {
            StatusMessage = $"Cannot save \u2014 invalid JSON: {parseError}";
            return;
        }
        try
        {
            await _fileService.SaveAsync(CurrentEditor.Session);
            CurrentEditor.RefreshFromSession();
            OnPropertyChanged(nameof(WindowTitle));
            StatusMessage = "Saved.";
        }
        catch (IOException ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(HasOpenFile))]
    private async Task SaveAsAsync()
    {
        if (CurrentEditor is null)
            return;
        if (!CurrentEditor.TryApplyJsonText(out string? parseError))
        {
            StatusMessage = $"Cannot save \u2014 invalid JSON: {parseError}";
            return;
        }
        string? path = await _dialogService.SaveFileAsAsync(CurrentEditor.FileName);
        if (path is null)
            return;
        try
        {
            await _fileService.SaveAsAsync(CurrentEditor.Session, path);
            CurrentEditor.RefreshFromSession();
            OnPropertyChanged(nameof(WindowTitle));
            StatusMessage = "Saved.";
        }
        catch (IOException ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(HasOpenFile))]
    private void Close()
    {
        CurrentEditor = null;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private static void Exit() => Environment.Exit(0);
}