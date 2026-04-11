using ConstanceSaveEditor.Core.Documents;

namespace ConstanceSaveEditor.Core.Abstractions;

public interface ISaveFileCodec
{
    Task<SaveArchiveData> DecodeAsync(Stream source, CancellationToken cancellationToken = default);
    Task EncodeAsync(Stream destination, SaveArchiveData archiveData, CancellationToken cancellationToken = default);
}