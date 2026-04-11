using System.Globalization;

namespace ConstanceSaveEditor.Core.Models;

/// <summary>
/// Decodes the type-prefixed value strings used in World.Data entries (e.g. "I42", "B1", "F3.14").
/// The first character is the type code; the rest is the raw value.
/// </summary>
public readonly struct WorldDataValue
{
    /// <summary>Sentinel returned when the raw string is null, empty, or malformed.</summary>
    public static readonly WorldDataValue Unknown = new('?', string.Empty);

    public char TypeCode { get; }
    public string RawValue { get; }

    private WorldDataValue(char typeCode, string rawValue)
    {
        TypeCode = typeCode;
        RawValue = rawValue;
    }

    public bool IsInt => TypeCode == 'I';
    public bool IsBool => TypeCode == 'B';
    public bool IsFloat => TypeCode == 'F';
    public bool IsLong => TypeCode == 'L';
    public bool IsString => TypeCode == 'S';
    public bool IsUnknown => TypeCode == '?';

    public int AsInt() => int.Parse(RawValue, CultureInfo.InvariantCulture);
    public bool AsBool() => RawValue == "1";
    public float AsFloat() => float.Parse(RawValue, CultureInfo.InvariantCulture);
    public long AsLong() => long.Parse(RawValue, CultureInfo.InvariantCulture);
    public string AsString() => RawValue;

    /// <summary>Re-encodes to the original wire format, e.g. "I42".</summary>
    public string Encode() => TypeCode + RawValue;

    public static WorldDataValue Parse(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
            return Unknown;
        return new WorldDataValue(raw[0], raw.Length > 1 ? raw[1..] : string.Empty);
    }

    public static WorldDataValue FromInt(int value) =>
        new('I', value.ToString(CultureInfo.InvariantCulture));

    public static WorldDataValue FromBool(bool value) =>
        new('B', value ? "1" : "0");

    public static WorldDataValue FromFloat(float value) =>
        new('F', value.ToString(CultureInfo.InvariantCulture));

    public static WorldDataValue FromLong(long value) =>
        new('L', value.ToString(CultureInfo.InvariantCulture));

    public static WorldDataValue FromString(string value) =>
        new('S', value);

    public override string ToString() => Encode();
}