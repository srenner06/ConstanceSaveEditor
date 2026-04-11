namespace ConstanceSaveEditor.Core.Documents;

public sealed class SaveArchiveData
{
    public required string SaveJsonText { get; init; }
    public byte[]? EventLog { get; init; }
    public byte[]? MapFog { get; init; }
    public SaveContainerFormat Format { get; init; }
}