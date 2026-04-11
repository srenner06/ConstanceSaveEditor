using ConstanceSaveEditor.Core.Documents;
using ConstanceSaveEditor.Core.Services;

namespace ConstanceSaveEditor.Core.Abstractions;

public interface ISaveDocumentService
{
    /// <summary>Build a <see cref="SaveDocument"/> from decoded archive data.</summary>
    Task<SaveDocument> LoadAsync(SaveArchiveData archiveData, string? sourcePath = null, CancellationToken cancellationToken = default);

    /// <summary>Serialize a document back to archive data ready for encoding.</summary>
    SaveArchiveData ToArchiveData(SaveDocument document);

    /// <summary>Create an editor session for a document, capturing an initial snapshot for revert support.</summary>
    SaveEditorSession CreateSession(SaveDocument document);
}