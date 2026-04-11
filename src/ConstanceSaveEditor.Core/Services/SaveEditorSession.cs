using System.Text.Json;
using ConstanceSaveEditor.Core.Documents;
using ConstanceSaveEditor.Core.Models;

namespace ConstanceSaveEditor.Core.Services;

/// <summary>
/// Wraps a <see cref="SaveDocument"/> for an editing session.
/// Holds an immutable snapshot of the document at load time to support revert.
/// </summary>
public sealed class SaveEditorSession
{
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = false };
    private static readonly JsonSerializerOptions ReadOptions = new() { PropertyNameCaseInsensitive = true };

    public SaveDocument Document { get; private set; }

    /// <summary>State of the document at the time the session was created. Never mutated.</summary>
    public SaveDocument OriginalSnapshot { get; private set; }

    public SaveEditorSession(SaveDocument document)
    {
        Document = document;
        OriginalSnapshot = Clone(document);
    }

    /// <summary>Mark the document as having unsaved changes.</summary>
    public void MarkDirty() => Document.IsDirty = true;

    /// <summary>Discard all edits since the last load or save and restore the original snapshot.</summary>
    public void RevertUnsavedChanges()
    {
        Document = Clone(OriginalSnapshot);
        Document.IsDirty = false;
    }

    /// <summary>
    /// Call after a successful write to disk.
    /// Advances the baseline snapshot so a subsequent revert returns to the newly saved state.
    /// </summary>
    public void CommitSave()
    {
        OriginalSnapshot = Clone(Document);
        Document.IsDirty = false;
    }

    /// <summary>Create a deep copy of the current document state, e.g. for a backup pass.</summary>
    public SaveDocument CreateSnapshot() => Clone(Document);

    private static SaveDocument Clone(SaveDocument source)
    {
        // Round-trip through JSON for a simple, reliable deep clone of SaveData.
        string json = JsonSerializer.Serialize(source.Save, WriteOptions);
        SaveData clonedSave = JsonSerializer.Deserialize<SaveData>(json, ReadOptions) ?? new SaveData();

        return new SaveDocument
        {
            SourcePath = source.SourcePath,
            Save = clonedSave,
            EventLog = source.EventLog?.ToArray(),
            MapFog = source.MapFog?.ToArray(),
            Format = source.Format,
            IsDirty = source.IsDirty,
            LoadedAt = source.LoadedAt,
        };
    }
}