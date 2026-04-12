using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace ConstanceSaveEditor.UI.Services;

public sealed class DialogService : IDialogService
{
    private static readonly FilePickerFileType SaveFileType =
        new("Constance Save Files") { Patterns = ["*.save"] };

    private readonly Window _owner;

    public DialogService(Window owner) => _owner = owner;

    public async Task<string?> OpenSaveFileAsync()
    {
        var files = await _owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Save File",
            AllowMultiple = false,
            FileTypeFilter = [SaveFileType],
        });
        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    public async Task<string?> SaveFileAsAsync(string? suggestedName = null)
    {
        var file = await _owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save As",
            SuggestedFileName = suggestedName,
            FileTypeChoices = [SaveFileType],
        });
        return file?.TryGetLocalPath();
    }
}