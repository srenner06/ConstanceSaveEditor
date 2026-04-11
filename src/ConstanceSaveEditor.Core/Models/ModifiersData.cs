using System.Text.Json.Serialization;

namespace ConstanceSaveEditor.Core.Models;

public sealed class ModifiersData
{
    public int GameMode { get; set; }

    public bool Permadeath { get; set; }

    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }
}