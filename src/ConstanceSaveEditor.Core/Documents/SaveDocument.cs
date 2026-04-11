using System.Text.Json.Nodes;
using ConstanceSaveEditor.Core.Documents;

namespace ConstanceSaveEditor.Core.Documents;

public sealed class SaveDocument
{
    /// <summary>Absolute path to the file this document was loaded from, or null if unsaved.</summary>
    public string? SourcePath { get; set; }

    /// <summary>Parsed save.json. Mutate this to edit the save.</summary>
    public JsonNode SaveJson { get; set; } = new JsonObject();

    /// <summary>Raw eventlog.bin bytes, or null if the save did not contain one.</summary>
    public byte[]? EventLog { get; set; }

    /// <summary>Raw mapfog.bin bytes, or null if the save did not contain one.</summary>
    public byte[]? MapFog { get; set; }

    /// <summary>True when the document has unsaved changes relative to the last load or save.</summary>
    public bool IsDirty { get; set; }

    /// <summary>UTC time at which this document was created from a file or archive.</summary>
    public DateTimeOffset LoadedAt { get; init; }

    /// <summary>Container format to use when re-encoding this document.</summary>
    public SaveContainerFormat Format { get; init; }
}