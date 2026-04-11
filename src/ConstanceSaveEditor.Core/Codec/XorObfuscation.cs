namespace ConstanceSaveEditor.Core.Codec;

internal static class XorObfuscation
{
    internal static byte[] Apply(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        var result = new byte[data.Length];
        Apply(data, key, result);
        return result;
    }

    internal static void Apply(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key, Span<byte> output)
    {
        for (var i = 0; i < data.Length; i++)
            output[i] = (byte)(data[i] ^ key[i % key.Length]);
    }
}