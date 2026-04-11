using System.Text.Json.Serialization;

namespace ConstanceSaveEditor.Core.Models;

public sealed class WorldDataEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    public WorldDataValue GetValue() => WorldDataValue.Parse(Value);

    public void SetValue(WorldDataValue value) => Value = value.Encode();
}