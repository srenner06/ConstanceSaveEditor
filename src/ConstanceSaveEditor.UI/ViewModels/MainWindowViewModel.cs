using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConstanceSaveEditor.Core.Abstractions;
using ConstanceSaveEditor.Core.Backup;
using ConstanceSaveEditor.UI.Services;

namespace ConstanceSaveEditor.UI.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly ISaveFileService _fileService;
    private readonly IDialogService _dialogService;
    private readonly RecentFilesService _recentFilesService;
    private readonly SettingsService _settingsService;
    private AppSettings _settings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOpenFile), nameof(NoFileOpen), nameof(WindowTitle), nameof(FileStatusText))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(SaveAsCommand), nameof(CloseCommand))]
    private SaveEditorViewModel? _currentEditor;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public bool HasOpenFile => CurrentEditor is not null;
    public bool NoFileOpen => CurrentEditor is null;
    public bool HasRecentFiles => RecentFiles.Count > 0;

    public ObservableCollection<MenuItem> RecentFiles { get; } = [];

    public string WindowTitle
    {
        get
        {
            if (CurrentEditor is null)
                return "Constance Save Editor";
            var dirty = CurrentEditor.IsDirty ? " *" : string.Empty;
            return $"Constance Save Editor \u2014 {CurrentEditor.FileName}{dirty}";
        }
    }

    public string FileStatusText =>
        CurrentEditor?.SourcePath ?? "No file open";

    public MainWindowViewModel(
        ISaveFileService fileService,
        IDialogService dialogService,
        RecentFilesService recentFilesService,
        SettingsService settingsService,
        AppSettings initialSettings)
    {
        _fileService = fileService;
        _dialogService = dialogService;
        _recentFilesService = recentFilesService;
        _settingsService = settingsService;
        _settings = initialSettings;
        LoadRecentFiles();
    }

    private void LoadRecentFiles()
    {
        RecentFiles.Clear();
        foreach (var path in _recentFilesService.Load())
        {
            var capturedPath = path;
            RecentFiles.Add(new MenuItem
            {
                Header = Path.GetFileName(path),
                Command = new AsyncRelayCommand(() => OpenFileAsync(capturedPath)),
            });
        }
        OnPropertyChanged(nameof(HasRecentFiles));
    }

    partial void OnCurrentEditorChanged(SaveEditorViewModel? oldValue, SaveEditorViewModel? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= OnEditorPropertyChanged;
            oldValue.RestoreRequested -= OnRestoreRequestedAsync;
        }
        if (newValue is not null)
        {
            newValue.PropertyChanged += OnEditorPropertyChanged;
            newValue.RestoreRequested += OnRestoreRequestedAsync;
        }
    }

    private void OnEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SaveEditorViewModel.IsDirty))
        {
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(FileStatusText));
        }
    }

    private async void OnRestoreRequestedAsync(BackupEntry entry)
    {
        var originalPath = CurrentEditor?.SourcePath;
        if (originalPath is null)
            return;
        try
        {
            var session = await _fileService.OpenAsync(entry.BackupPath);
            session.Document.SourcePath = originalPath;
            session.MarkDirty();
            CurrentEditor = new SaveEditorViewModel(session);
            StatusMessage = $"Backup from {entry.CreatedAt:yyyy-MM-dd HH:mm:ss} loaded \u2014 save to write to disk.";
        }
        catch (IOException ex)
        {
            StatusMessage = $"Restore failed: {ex.Message}";
        }
        catch (InvalidDataException ex)
        {
            StatusMessage = $"Restore failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task OpenAsync()
    {
        var path = await _dialogService.OpenSaveFileAsync();
        if (path is null)
            return;
        await OpenFileAsync(path);
    }

    private async Task OpenFileAsync(string path)
    {
        try
        {
            var session = await _fileService.OpenAsync(path);
            CurrentEditor = new SaveEditorViewModel(session);
            _recentFilesService.Add(path);
            LoadRecentFiles();
            StatusMessage = string.Empty;
        }
        catch (IOException ex)
        {
            StatusMessage = $"Cannot open file: {ex.Message}";
            _recentFilesService.Remove(path);
            LoadRecentFiles();
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
        if (!CurrentEditor.TryApplyJsonText(out var parseError))
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
        if (!CurrentEditor.TryApplyJsonText(out var parseError))
        {
            StatusMessage = $"Cannot save \u2014 invalid JSON: {parseError}";
            return;
        }
        var path = await _dialogService.SaveFileAsAsync(CurrentEditor.FileName);
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

    [RelayCommand]
    private void SetTheme(string theme)
    {
        ((App)Application.Current!).ApplyTheme(theme);
        _settings = _settings with { Theme = theme };
        _settingsService.Save(_settings);
    }

    [RelayCommand]
    private void SetLanguage(string language)
    {
        ((App)Application.Current!).ApplyLanguage(language);
        _settings = _settings with { Language = language };
        _settingsService.Save(_settings);
    }
}