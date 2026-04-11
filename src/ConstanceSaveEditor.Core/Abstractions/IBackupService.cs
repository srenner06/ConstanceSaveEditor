using ConstanceSaveEditor.Core.Backup;

namespace ConstanceSaveEditor.Core.Abstractions;

public interface IBackupService
{
    /// <summary>Copy the current file to the backup folder before overwriting it.</summary>
    Task<BackupEntry> CreateBackupAsync(string sourcePath, CancellationToken cancellationToken = default);

    /// <summary>Return all backups for a given file, newest first.</summary>
    Task<IReadOnlyList<BackupEntry>> ListBackupsAsync(string sourcePath, CancellationToken cancellationToken = default);

    /// <summary>Delete the oldest backups so that at most <paramref name="maxToKeep"/> remain.</summary>
    Task DeleteOldBackupsAsync(string sourcePath, int maxToKeep = 20, CancellationToken cancellationToken = default);
}