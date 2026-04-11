using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ConstanceSaveEditor.Core.Models;

public sealed class SaveData
{
    public int Version { get; set; }

    public string Id { get; set; } = string.Empty;

    public PlayerData Player { get; set; } = new();

    public WorldData World { get; set; } = new();

    /// <summary>Array of unknown schema — preserved as raw JSON for round-trip safety.</summary>
    public JsonArray InitCommands { get; set; } = [];

    public ModifiersData Modifiers { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }
}