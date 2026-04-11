using System.Text.Json.Nodes;
using CommandLine;
using ConstanceSaveEditor.Core.Codec;
using ConstanceSaveEditor.Core.Documents;

namespace ConstanceSaveEditor.Cli.Commands;

[Verb("show", HelpText = "Display metadata and structure summary for a save file.")]
internal sealed class ShowOptions
{
    [Value(0, Required = true, MetaName = "savefile", HelpText = "Path to the .save file.")]
    public string SaveFile { get; set; } = string.Empty;
}

internal static class ShowCommand
{
    internal static async Task<int> RunAsync(ShowOptions opts)
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

        int topLevelKeyCount = -1;
        if (JsonNode.Parse(archiveData.SaveJsonText) is JsonObject obj)
            topLevelKeyCount = obj.Count;

        Console.WriteLine($"File:      {Path.GetFullPath(opts.SaveFile)}");
        Console.WriteLine($"Format:    {archiveData.Format}");
        Console.WriteLine($"save.json: {archiveData.SaveJsonText.Length:N0} chars" +
                          (topLevelKeyCount >= 0 ? $", {topLevelKeyCount:N0} top-level keys" : string.Empty));
        Console.WriteLine($"eventlog:  {FormatBinaryEntry(archiveData.EventLog)}");
        Console.WriteLine($"mapfog:    {FormatBinaryEntry(archiveData.MapFog)}");

        return 0;
    }

    private static string FormatBinaryEntry(byte[]? data) =>
        data is { Length: > 0 } ? $"{data.Length:N0} bytes" : "not present";
}