using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ConstanceSaveEditor.Core.Abstractions;
using ConstanceSaveEditor.Core.Documents;

namespace ConstanceSaveEditor.Core.Services;

public sealed class SaveDocumentService : ISaveDocumentService
{
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
    };

    public async Task<SaveDocument> LoadAsync(SaveArchiveData archiveData, string? sourcePath = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(archiveData.SaveJsonText));
        var saveJson = await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken)
            ?? throw new InvalidDataException("save.json parsed to null — the save file may be corrupted.");

        return new SaveDocument
        {
            SourcePath = sourcePath,
            SaveJson = saveJson,
            EventLog = archiveData.EventLog,
            MapFog = archiveData.MapFog,
            Format = archiveData.Format,
            LoadedAt = DateTimeOffset.UtcNow,
        };
    }

    public SaveArchiveData ToArchiveData(SaveDocument document)
    {
        return new SaveArchiveData
        {
            SaveJsonText = document.SaveJson.ToJsonString(CompactOptions),
            EventLog = document.EventLog,
            MapFog = document.MapFog,
            Format = document.Format,
        };
    }

    public SaveEditorSession CreateSession(SaveDocument document) => new(document);
}