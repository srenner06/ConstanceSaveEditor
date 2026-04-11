using System.IO.Compression;
using System.Text;
using ConstanceSaveEditor.Core.Abstractions;
using ConstanceSaveEditor.Core.Documents;

namespace ConstanceSaveEditor.Core.Codec;

public sealed class SaveFileCodec : ISaveFileCodec
{
    // "Xornstance!" is both the file header and the XOR key.
    private static readonly byte[] Header = "Xornstance!"u8.ToArray();

    public async Task<SaveArchiveData> DecodeAsync(Stream source, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, cancellationToken);
        byte[] allBytes = buffer.ToArray();

        if (allBytes.AsSpan().StartsWith(Header.AsSpan()))
        {
            byte[] zipBytes = XorObfuscation.Apply(allBytes.AsSpan(Header.Length), Header);
            return await ReadZipArchiveAsync(zipBytes, cancellationToken);
        }

        // Legacy / debug saves stored as plain JSON with no header or encryption.
        return new SaveArchiveData
        {
            SaveJsonText = Encoding.UTF8.GetString(allBytes),
            Format = SaveContainerFormat.PlainJson,
        };
    }

    public async Task EncodeAsync(Stream destination, SaveArchiveData archiveData, CancellationToken cancellationToken = default)
    {
        if (archiveData.Format == SaveContainerFormat.PlainJson)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(archiveData.SaveJsonText);
            await destination.WriteAsync(jsonBytes, cancellationToken);
            return;
        }

        byte[] zipBytes = await WriteZipArchiveAsync(archiveData, cancellationToken);
        byte[] encodedPayload = XorObfuscation.Apply(zipBytes, Header);

        await destination.WriteAsync(Header, cancellationToken);
        await destination.WriteAsync(encodedPayload, cancellationToken);
    }

    private static async Task<SaveArchiveData> ReadZipArchiveAsync(byte[] zipBytes, CancellationToken ct)
    {
        using var ms = new MemoryStream(zipBytes, writable: false);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

        string saveJsonText = await ReadTextEntryAsync(zip, "save.json", ct)
            ?? throw new InvalidDataException("save.json not found in save archive.");

        return new SaveArchiveData
        {
            SaveJsonText = saveJsonText,
            EventLog = await ReadBinaryEntryAsync(zip, "eventlog.bin", ct),
            MapFog = await ReadBinaryEntryAsync(zip, "mapfog.bin", ct),
            Format = SaveContainerFormat.Xornstance,
        };
    }

    private static async Task<byte[]> WriteZipArchiveAsync(SaveArchiveData data, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            await WriteTextEntryAsync(zip, "save.json", data.SaveJsonText, ct);
            if (data.EventLog is { Length: > 0 })
                await WriteBinaryEntryAsync(zip, "eventlog.bin", data.EventLog, ct);
            if (data.MapFog is { Length: > 0 })
                await WriteBinaryEntryAsync(zip, "mapfog.bin", data.MapFog, ct);
        }
        return ms.ToArray();
    }

    private static async Task<string?> ReadTextEntryAsync(ZipArchive zip, string name, CancellationToken ct)
    {
        ZipArchiveEntry? entry = zip.GetEntry(name);
        if (entry is null)
            return null;
        using var stream = entry.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync(ct);
    }

    private static async Task<byte[]?> ReadBinaryEntryAsync(ZipArchive zip, string name, CancellationToken ct)
    {
        ZipArchiveEntry? entry = zip.GetEntry(name);
        if (entry is null)
            return null;
        using var entryStream = entry.Open();
        using var buffer = new MemoryStream();
        await entryStream.CopyToAsync(buffer, ct);
        return buffer.ToArray();
    }

    private static async Task WriteTextEntryAsync(ZipArchive zip, string name, string text, CancellationToken ct)
    {
        ZipArchiveEntry entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        // Explicit no-BOM UTF-8 so the JSON round-trips byte-for-byte with game saves.
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        await writer.WriteAsync(text.AsMemory(), ct);
    }

    private static async Task WriteBinaryEntryAsync(ZipArchive zip, string name, byte[] data, CancellationToken ct)
    {
        ZipArchiveEntry entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        await using var stream = entry.Open();
        await stream.WriteAsync(data, ct);
    }
}