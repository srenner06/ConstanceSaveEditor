using System.Globalization;
using CommandLine;
using ConstanceSaveEditor.Core.Codec;
using ConstanceSaveEditor.Core.Documents;
using ConstanceSaveEditor.Core.Models;
using ConstanceSaveEditor.Core.Services;

namespace ConstanceSaveEditor.Cli.Commands;

[Verb("show", HelpText = "Display metadata and a structured summary for a save file.")]
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

        SaveDocument document;
        try
        {
            document = await new SaveDocumentService().LoadAsync(archiveData, opts.SaveFile);
        }
        catch (InvalidDataException ex)
        {
            Console.Error.WriteLine($"Cannot parse save data: {ex.Message}");
            return 1;
        }

        SaveData save = document.Save;
        PlayerData player = save.Player;

        Console.WriteLine($"File:    {Path.GetFullPath(opts.SaveFile)}");
        Console.WriteLine($"Format:  {archiveData.Format}");
        Console.WriteLine();

        Console.WriteLine("--- Document ---");
        Console.WriteLine($"Version: {save.Version}");
        Console.WriteLine($"ID:      {save.Id}");
        Console.WriteLine();

        Console.WriteLine("--- Player ---");
        Console.WriteLine($"Health:      {player.Health}");
        Console.WriteLine($"Potion fill: {player.PotionFill:0.0}");
        Console.WriteLine($"Checkpoint:  {player.CheckpointId}");
        Console.WriteLine($"Played time: {FormatPlayedTime(player.PlayedTime)}");
        Console.WriteLine();

        Console.WriteLine("--- World ---");
        Console.WriteLine($"Data entries: {save.World.Data.Count:N0}");
        Console.WriteLine();

        Console.WriteLine("--- Modifiers ---");
        string gameMode = save.Modifiers.GameMode == 0 ? "Normal" : $"Custom ({save.Modifiers.GameMode})";
        Console.WriteLine($"Game mode:  {gameMode}");
        Console.WriteLine($"Permadeath: {save.Modifiers.Permadeath}");
        Console.WriteLine();

        Console.WriteLine("--- Binary ---");
        Console.WriteLine($"Event log: {FormatBinary(document.EventLog)}");
        Console.WriteLine($"Map fog:   {FormatBinary(document.MapFog)}");

        return 0;
    }

    private static string FormatPlayedTime(long milliseconds)
    {
        TimeSpan ts = TimeSpan.FromMilliseconds(milliseconds);
        return ts.TotalHours >= 1
            ? $"{(int)ts.TotalHours}h {ts.Minutes:D2}m {ts.Seconds:D2}s"
            : $"{ts.Minutes}m {ts.Seconds:D2}s";
    }

    private static string FormatBinary(byte[]? data) =>
        data is { Length: > 0 } ? $"{data.Length:N0} bytes" : "not present";
}