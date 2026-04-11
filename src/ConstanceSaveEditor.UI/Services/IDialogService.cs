namespace ConstanceSaveEditor.UI.Services;

public interface IDialogService
{
    Task<string?> OpenSaveFileAsync();
    Task<string?> SaveFileAsAsync(string? suggestedName = null);
}