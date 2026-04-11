using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConstanceSaveEditor.Core.Backup;
using ConstanceSaveEditor.Core.Services;

namespace ConstanceSaveEditor.UI.ViewModels.Editors;

public sealed partial class BackupsTabViewModel : ObservableObject
{
    private readonly SaveEditorSession _session;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText), nameof(ShowList))]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<BackupEntryViewModel> _entries = [];

    public string StatusText => IsLoading
        ? "Loading…"
        : Entries.Count == 0 ? "No backups found for this file." : string.Empty;

    public bool ShowList => !IsLoading && Entries.Count > 0;

    public event Action<BackupEntry>? RestoreRequested;

    public BackupsTabViewModel(SaveEditorSession session) => _session = session;

    partial void OnEntriesChanged(ObservableCollection<BackupEntryViewModel> value)
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(ShowList));
    }

    public async Task LoadAsync()
    {
        string? sourcePath = _session.Document.SourcePath;
        if (sourcePath is null)
        {
            Entries = [];
            return;
        }

        IsLoading = true;
        try
        {
            var service = new BackupService();
            IReadOnlyList<BackupEntry> backups = await service.ListBackupsAsync(sourcePath);
            Entries = new ObservableCollection<BackupEntryViewModel>(backups.Select(CreateEntryVm));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private Task Refresh() => LoadAsync();

    private BackupEntryViewModel CreateEntryVm(BackupEntry entry)
    {
        var vm = new BackupEntryViewModel(entry);
        vm.RestoreRequested += e => RestoreRequested?.Invoke(e);
        return vm;
    }
}