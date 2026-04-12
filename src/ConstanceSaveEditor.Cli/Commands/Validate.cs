using System.Text.Json;
using System.Text.Json.Nodes;
using CommandLine;
using ConstanceSaveEditor.Core.Codec;
using ConstanceSaveEditor.Core.Documents;

namespace ConstanceSaveEditor.Cli.Commands;

[Verb("validate", HelpText = "Decode a save file and report any structural errors.")]
internal sealed class ValidateOptions
{
    [Value(0, Required = true, MetaName = "savefile", HelpText = "Path to the .save file to validate.")]
    public string SaveFile { get; set; } = string.Empty;
}

internal static class ValidateCommand
{
    internal static async Task<int> RunAsync(ValidateOptions opts)
    {
        var errors = new List<string>();

        SaveArchiveData? archiveData = null;
        try
        {
            Console.Write("Decoding...  ");
            await using var stream = File.OpenRead(opts.SaveFile);
            archiveData = await new SaveFileCodec().DecodeAsync(stream);
            Console.WriteLine("OK");
        }
        catch (IOException ex)
        {
            Console.WriteLine("FAILED");
            errors.Add($"Cannot read save file: {ex.Message}");
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine("FAILED");
            errors.Add($"Archive error: {ex.Message}");
        }

        if (archiveData is not null)
        {
            try
            {
                Console.Write("Parsing JSON...  ");
                var node = JsonNode.Parse(archiveData.SaveJsonText);
                if (node is null)
                    errors.Add("save.json parsed to null.");
                else if (node is not JsonObject)
                    errors.Add($"save.json root is not an object (got {node.GetValueKind()}).");
                Console.WriteLine("OK");
            }
            catch (JsonException ex)
            {
                Console.WriteLine("FAILED");
                errors.Add($"JSON parse error: {ex.Message}");
            }
        }

        if (errors.Count == 0)
        {
            Console.WriteLine("Validation passed.");
            return 0;
        }

        foreach (var error in errors)
            Console.Error.WriteLine($"  - {error}");
        Console.Error.WriteLine($"Validation failed ({errors.Count} error{(errors.Count == 1 ? string.Empty : "s")}).");
        return 1;
    }
}