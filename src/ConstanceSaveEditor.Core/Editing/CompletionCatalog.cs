using System.Reflection;
using System.Text.Json;
using ConstanceSaveEditor.Core.Models;

namespace ConstanceSaveEditor.Core.Editing;

/// <summary>One thing the game counts towards completion, e.g. a chest, a secret wall, a visited room.</summary>
public sealed record CompletionEntry(string Id, string Category, string Level, string Region);

/// <summary>A quest; it contributes its intel pieces plus one for being completed.</summary>
public sealed record CompletionQuest(string Name, string Level, string Region, int Intel);

public sealed record RegionCompletion(string Region, int Done, int Total)
{
    public int Percent => Total == 0 ? 0 : (int)Math.Floor(Done / (double)Total * 100.0);

    public string Counts => $"{Done} / {Total}";
}

public sealed record CompletionReport(
    IReadOnlyList<RegionCompletion> Regions,
    IReadOnlyList<CompletionEntry> Missing,
    int Done,
    int Total)
{
    public int Percent => Total == 0 ? 0 : (int)Math.Floor(Done / (double)Total * 100.0);
}

/// <summary>
/// The game's own completion checklist, lifted from the baked SConLevelInfo / SConQuest assets.
/// <see cref="Evaluate"/> mirrors ConCompletionData.Recalc, so the numbers match the in-game map.
/// </summary>
public static class CompletionCatalog
{
    private const string QuestCompleted = "I2";

    private static readonly JsonSerializerOptions ReadOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly Lazy<CatalogFile> LazyCatalog = new(Load);

    public static IReadOnlyList<CompletionEntry> Entries => LazyCatalog.Value.Entries;
    public static IReadOnlyList<CompletionQuest> Quests => LazyCatalog.Value.Quests;

    public static CompletionReport Evaluate(List<WorldDataEntry> data)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in data)
            values[entry.Id] = entry.Value;

        var done = new Dictionary<string, int>(StringComparer.Ordinal);
        var total = new Dictionary<string, int>(StringComparer.Ordinal);
        var missing = new List<CompletionEntry>();

        foreach (var entry in Entries)
        {
            total[entry.Region] = total.GetValueOrDefault(entry.Region) + 1;
            if (IsSet(values.GetValueOrDefault(entry.Id)))
                done[entry.Region] = done.GetValueOrDefault(entry.Region) + 1;
            else
                missing.Add(entry);
        }

        foreach (var quest in Quests)
        {
            // Intel is a string of one letter per piece found, e.g. "ABC"; completing the quest fills it up.
            var questTotal = quest.Intel + 1;
            var isComplete = values.GetValueOrDefault($"ps_quest_{quest.Name}_status") == QuestCompleted;
            var intel = WorldDataValue.Parse(values.GetValueOrDefault($"ps_quest_{quest.Name}_intel"));
            var questDone = isComplete
                ? questTotal
                : Math.Min(quest.Intel, intel.IsString ? intel.AsString().Length : 0);

            total[quest.Region] = total.GetValueOrDefault(quest.Region) + questTotal;
            done[quest.Region] = done.GetValueOrDefault(quest.Region) + questDone;
            if (questDone < questTotal)
                missing.Add(new CompletionEntry($"ps_quest_{quest.Name}_status", "Quest", quest.Level, quest.Region));
        }

        var regions = total
            .Select(kv => new RegionCompletion(kv.Key, done.GetValueOrDefault(kv.Key), kv.Value))
            .OrderBy(r => r.Region, StringComparer.Ordinal)
            .ToList();

        return new CompletionReport(regions, missing, regions.Sum(r => r.Done), regions.Sum(r => r.Total));
    }

    private static bool IsSet(string? raw)
    {
        var value = WorldDataValue.Parse(raw);
        return value.IsInt ? value.AsInt() > 0 : value.IsBool && value.AsBool();
    }

    private sealed record CatalogFile(List<CompletionEntry> Entries, List<CompletionQuest> Quests);

    private static CatalogFile Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("completion.json")
            ?? throw new InvalidOperationException("Embedded completion catalog is missing.");
        return JsonSerializer.Deserialize<CatalogFile>(stream, ReadOptions)
            ?? throw new InvalidOperationException("Completion catalog could not be read.");
    }
}