using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConstanceSaveEditor.Core.Backup;

namespace ConstanceSaveEditor.UI.ViewModels.Editors;

public sealed partial class BackupEntryViewModel : ObservableObject
{
    public BackupEntry Entry { get; }

    public string DateDisplay => Entry.CreatedAt.ToString("yyyy-MM-dd  HH:mm:ss", CultureInfo.CurrentCulture);
    public string SizeDisplay => FormatSize(Entry.FileSizeBytes);

    public event Action<BackupEntry>? RestoreRequested;

    public BackupEntryViewModel(BackupEntry entry) => Entry = entry;

    [RelayCommand]
    private void Restore() => RestoreRequested?.Invoke(Entry);

    private static string FormatSize(long bytes) => bytes switch
    {
        >= 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F1} MB",
        >= 1024 => $"{bytes / 1024.0:F0} KB",
        _ => $"{bytes} B",
    };
}