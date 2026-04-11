using System.Text.Json;
using System.Text.Json.Nodes;
using ConstanceSaveEditor.Core.Documents;

namespace ConstanceSaveEditor.Core.Services;

/// <summary>
/// Wraps a <see cref="SaveDocument"/> for an editing session.
/// Holds an immutable snapshot of the document at load time to support revert.
/// </summary>
public sealed class SaveEditorSession
{
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
        // Round-trip through a JSON string for a simple, reliable deep clone of JsonNode.
        JsonNode? clonedJson = JsonNode.Parse(source.SaveJson.ToJsonString());

        return new SaveDocument
        {
            SourcePath = source.SourcePath,
            SaveJson = clonedJson ?? new JsonObject(),
            EventLog = source.EventLog?.ToArray(),
            MapFog = source.MapFog?.ToArray(),
            Format = source.Format,
            IsDirty = source.IsDirty,
            LoadedAt = source.LoadedAt,
        };
    }
}