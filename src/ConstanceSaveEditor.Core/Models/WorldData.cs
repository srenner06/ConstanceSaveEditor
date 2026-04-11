using System.Text.Json.Serialization;

namespace ConstanceSaveEditor.Core.Models;

public sealed class WorldData
{
    public List<WorldDataEntry> Data { get; set; } = [];

    public List<WorldDataEntry> TmpLevelPersistence { get; set; } = [];

    public List<WorldDataEntry> TmpMeditationPersistence { get; set; } = [];

    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }
}