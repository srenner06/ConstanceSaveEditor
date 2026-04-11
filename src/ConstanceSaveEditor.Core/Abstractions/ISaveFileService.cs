using ConstanceSaveEditor.Core.Services;

namespace ConstanceSaveEditor.Core.Abstractions;

public interface ISaveFileService
{
    /// <summary>Open a save file and return an editing session ready for use.</summary>
    Task<SaveEditorSession> OpenAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save using the document's existing source path.
    /// Throws <see cref="InvalidOperationException"/> when <see cref="Documents.SaveDocument.SourcePath"/> is null —
    /// use <see cref="SaveAsAsync"/> for new files.
    /// </summary>
    Task SaveAsync(SaveEditorSession session, CancellationToken cancellationToken = default);

    /// <summary>Save to a new path and update the document's source path.</summary>
    Task SaveAsAsync(SaveEditorSession session, string path, CancellationToken cancellationToken = default);
}