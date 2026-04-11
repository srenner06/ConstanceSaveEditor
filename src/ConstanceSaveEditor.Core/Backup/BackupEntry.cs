namespace ConstanceSaveEditor.Core.Backup;

public sealed class BackupEntry
{
    public required string BackupPath { get; init; }
    public required string OriginalFileName { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}