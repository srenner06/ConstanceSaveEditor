using System.Text.Json.Serialization;

namespace ConstanceSaveEditor.Core.Models;

public sealed class PlayerData
{
    public int Health { get; set; }

    public float PotionFill { get; set; }

    public string CheckpointId { get; set; } = string.Empty;

    public string PersevereId { get; set; } = string.Empty;

    public string ShrineId { get; set; } = string.Empty;

    public long PlayedTime { get; set; }

    public long AfkTime { get; set; }

    public long SaveTime { get; set; }

    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }
}