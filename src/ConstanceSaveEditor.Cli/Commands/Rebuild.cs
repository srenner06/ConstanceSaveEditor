using CommandLine;
using ConstanceSaveEditor.Core.Codec;
using ConstanceSaveEditor.Core.Documents;

namespace ConstanceSaveEditor.Cli.Commands;

[Verb("rebuild", HelpText = "Encode a folder of extracted files back into a .save file.")]
internal sealed class RebuildOptions
{
    [Value(0, Required = true, MetaName = "indir", HelpText = "Input directory containing save.json (and optionally eventlog.bin, mapfog.bin).")]
    public string InputDir { get; set; } = string.Empty;

    [Value(1, Required = true, MetaName = "outfile", HelpText = "Path for the output .save file.")]
    public string OutputFile { get; set; } = string.Empty;
}

internal static class RebuildCommand
{
    internal static async Task<int> RunAsync(RebuildOptions opts)
    {
        SaveArchiveData archiveData;
        try
        {
            string jsonPath = Path.Combine(opts.InputDir, "save.json");
            if (!File.Exists(jsonPath))
            {
                Console.Error.WriteLine($"save.json not found in: {opts.InputDir}");
                return 1;
            }

            string saveJsonText = await File.ReadAllTextAsync(jsonPath);
            byte[]? eventLog = ReadOptionalBinary(Path.Combine(opts.InputDir, "eventlog.bin"));
            byte[]? mapFog = ReadOptionalBinary(Path.Combine(opts.InputDir, "mapfog.bin"));

            archiveData = new SaveArchiveData
            {
                SaveJsonText = saveJsonText,
                EventLog = eventLog,
                MapFog = mapFog,
                Format = SaveContainerFormat.Xornstance,
            };
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"Cannot read input: {ex.Message}");
            return 1;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine($"Access denied: {ex.Message}");
            return 1;
        }

        try
        {
            string? dir = Path.GetDirectoryName(opts.OutputFile);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            await using var stream = new FileStream(
                opts.OutputFile, FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize: 4096, FileOptions.Asynchronous);
            await new SaveFileCodec().EncodeAsync(stream, archiveData);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"Cannot write output: {ex.Message}");
            return 1;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine($"Access denied: {ex.Message}");
            return 1;
        }

        Console.WriteLine($"Rebuilt: {opts.OutputFile}");
        return 0;
    }

    private static byte[]? ReadOptionalBinary(string path) =>
        File.Exists(path) ? File.ReadAllBytes(path) : null;
}