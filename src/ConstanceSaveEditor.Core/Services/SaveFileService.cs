using ConstanceSaveEditor.Core.Abstractions;

namespace ConstanceSaveEditor.Core.Services;

public sealed class SaveFileService : ISaveFileService
{
    private readonly ISaveFileCodec _codec;
    private readonly ISaveDocumentService _documentService;
    private readonly IBackupService _backupService;

    public SaveFileService(ISaveFileCodec codec, ISaveDocumentService documentService, IBackupService backupService)
    {
        _codec = codec;
        _documentService = documentService;
        _backupService = backupService;
    }

    public async Task<SaveEditorSession> OpenAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = OpenForRead(path);
        var archiveData = await _codec.DecodeAsync(stream, cancellationToken);
        var document = await _documentService.LoadAsync(archiveData, sourcePath: path, cancellationToken);
        return _documentService.CreateSession(document);
    }

    public async Task SaveAsync(SaveEditorSession session, CancellationToken cancellationToken = default)
    {
        var path = session.Document.SourcePath
            ?? throw new InvalidOperationException("Document has no source path. Use SaveAsAsync for new files.");
        await WriteToPathAsync(session, path, cancellationToken);
    }

    public async Task SaveAsAsync(SaveEditorSession session, string path, CancellationToken cancellationToken = default)
    {
        session.Document.SourcePath = path;
        await WriteToPathAsync(session, path, cancellationToken);
    }

    private async Task WriteToPathAsync(SaveEditorSession session, string path, CancellationToken ct)
    {
        var archiveData = _documentService.ToArchiveData(session.Document);

        if (File.Exists(path))
            await _backupService.CreateBackupAsync(path, ct);

        var tempPath = path + ".cse-tmp";
        try
        {
            await using (var stream = OpenForWrite(tempPath))
                await _codec.EncodeAsync(stream, archiveData, ct);

            File.Move(tempPath, path, overwrite: true);
        }
        catch
        {
            TryDelete(tempPath);
            throw;
        }

        session.CommitSave();
        await _backupService.DeleteOldBackupsAsync(path, cancellationToken: ct);
    }

    private static FileStream OpenForRead(string path) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

    private static FileStream OpenForWrite(string path) =>
        new(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096,
            FileOptions.Asynchronous);

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}