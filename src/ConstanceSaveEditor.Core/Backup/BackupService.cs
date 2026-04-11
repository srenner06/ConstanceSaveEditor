using System.Globalization;
using ConstanceSaveEditor.Core.Abstractions;

namespace ConstanceSaveEditor.Core.Backup;

public sealed class BackupService : IBackupService
{
    private const string BackupFolderName = ".cse-backups";
    private const string TimestampFormat = "yyyy-MM-dd_HH-mm-ss";
    private const int TimestampLength = 19; // matches TimestampFormat exactly

    public async Task<BackupEntry> CreateBackupAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        var backupDir = GetBackupDirectory(sourcePath);
        Directory.CreateDirectory(backupDir);

        var timestamp = DateTimeOffset.Now.ToString(TimestampFormat, CultureInfo.InvariantCulture);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(sourcePath);
        var ext = Path.GetExtension(sourcePath);
        var backupPath = Path.Combine(backupDir, $"{nameWithoutExt}_{timestamp}{ext}");

        var bytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken);
        await File.WriteAllBytesAsync(backupPath, bytes, cancellationToken);

        return new BackupEntry
        {
            BackupPath = backupPath,
            OriginalFileName = Path.GetFileName(sourcePath),
            CreatedAt = DateTimeOffset.Now,
            FileSizeBytes = bytes.Length,
        };
    }

    public Task<IReadOnlyList<BackupEntry>> ListBackupsAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        var backupDir = GetBackupDirectory(sourcePath);
        if (!Directory.Exists(backupDir))
            return Task.FromResult<IReadOnlyList<BackupEntry>>([]);

        var originalFileName = Path.GetFileName(sourcePath);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(sourcePath);
        var ext = Path.GetExtension(sourcePath);

        var entries = Directory
            .GetFiles(backupDir, $"{nameWithoutExt}_*{ext}")
            .Select(path => ParseBackupEntry(path, originalFileName))
            .OfType<BackupEntry>()
            .OrderByDescending(static e => e.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<BackupEntry>>(entries);
    }

    public async Task DeleteOldBackupsAsync(string sourcePath, int maxToKeep = 20, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<BackupEntry> backups = await ListBackupsAsync(sourcePath, cancellationToken);
        foreach (BackupEntry entry in backups.Skip(maxToKeep))
            File.Delete(entry.BackupPath);
    }

    private static string GetBackupDirectory(string sourcePath) =>
        Path.Combine(Path.GetDirectoryName(sourcePath) ?? ".", BackupFolderName);

    private static BackupEntry? ParseBackupEntry(string backupPath, string originalFileName)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(backupPath);

        // Expected tail: _{yyyy-MM-dd_HH-mm-ss}
        // The underscore separator sits at position [^(TimestampLength + 1)].
        if (nameWithoutExt.Length < TimestampLength + 1 || nameWithoutExt[^(TimestampLength + 1)] != '_')
            return null;

        var timestampPart = nameWithoutExt[^TimestampLength..];
        if (!DateTimeOffset.TryParseExact(
                timestampPart,
                TimestampFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out DateTimeOffset createdAt))
            return null;

        return new BackupEntry
        {
            BackupPath = backupPath,
            OriginalFileName = originalFileName,
            CreatedAt = createdAt,
            FileSizeBytes = new FileInfo(backupPath).Length,
        };
    }
}