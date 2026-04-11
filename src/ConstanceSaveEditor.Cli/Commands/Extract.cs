using System.Text.Json;
using System.Text.Json.Nodes;
using CommandLine;
using ConstanceSaveEditor.Core.Codec;
using ConstanceSaveEditor.Core.Documents;

namespace ConstanceSaveEditor.Cli.Commands;

[Verb("extract", HelpText = "Decode a save file and write its contents to a folder.")]
internal sealed class ExtractOptions
{
    [Value(0, Required = true, MetaName = "savefile", HelpText = "Path to the .save file to extract.")]
    public string SaveFile { get; set; } = string.Empty;

    [Value(1, Required = true, MetaName = "outdir", HelpText = "Output directory. Created if it does not exist.")]
    public string OutputDir { get; set; } = string.Empty;
}

internal static class ExtractCommand
{
    private static readonly JsonSerializerOptions PrettyOptions = new() { WriteIndented = true };

    internal static async Task<int> RunAsync(ExtractOptions opts)
    {
        SaveArchiveData archiveData;
        try
        {
            await using var stream = File.OpenRead(opts.SaveFile);
            archiveData = await new SaveFileCodec().DecodeAsync(stream);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"Cannot read save file: {ex.Message}");
            return 1;
        }
        catch (InvalidDataException ex)
        {
            Console.Error.WriteLine($"Invalid save file: {ex.Message}");
            return 1;
        }

        try
        {
            Directory.CreateDirectory(opts.OutputDir);

            string prettyJson = JsonNode.Parse(archiveData.SaveJsonText)
                ?.ToJsonString(PrettyOptions) ?? archiveData.SaveJsonText;
            await File.WriteAllTextAsync(Path.Combine(opts.OutputDir, "save.json"), prettyJson);

            if (archiveData.EventLog is { Length: > 0 })
                await File.WriteAllBytesAsync(Path.Combine(opts.OutputDir, "eventlog.bin"), archiveData.EventLog);

            if (archiveData.MapFog is { Length: > 0 })
                await File.WriteAllBytesAsync(Path.Combine(opts.OutputDir, "mapfog.bin"), archiveData.MapFog);
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

        Console.WriteLine($"Extracted to: {opts.OutputDir}");
        return 0;
    }
}