using System.Text;
using System.Text.Json;
using ConstanceSaveEditor.Core.Abstractions;
using ConstanceSaveEditor.Core.Documents;
using ConstanceSaveEditor.Core.Models;

namespace ConstanceSaveEditor.Core.Services;

public sealed class SaveDocumentService : ISaveDocumentService
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = false,
    };

    public async Task<SaveDocument> LoadAsync(SaveArchiveData archiveData, string? sourcePath = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(archiveData.SaveJsonText));
        SaveData? save = await JsonSerializer.DeserializeAsync<SaveData>(stream, ReadOptions, cancellationToken)
            ?? throw new InvalidDataException("save.json deserialized to null — the save file may be corrupted.");

        return new SaveDocument
        {
            SourcePath = sourcePath,
            Save = save,
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
            SaveJsonText = JsonSerializer.Serialize(document.Save, WriteOptions),
            EventLog = document.EventLog,
            MapFog = document.MapFog,
            Format = document.Format,
        };
    }

    public SaveEditorSession CreateSession(SaveDocument document) => new(document);
}