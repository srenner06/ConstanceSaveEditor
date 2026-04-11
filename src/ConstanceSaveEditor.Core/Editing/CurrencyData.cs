using ConstanceSaveEditor.Core.Models;

namespace ConstanceSaveEditor.Core.Editing;

/// <summary>
/// Reads and writes the three currency fields stored in <see cref="WorldData.Data"/>.
/// </summary>
public sealed class CurrencyData
{
    private const string KeyBalance = "ps_item_Currency";
    private const string KeyTotalCollected = "ps_TotalCollectCount_Currency";
    private const string KeyAllTimeGlimmer = "ps_StatId_AllTimeCollectedGlimmer";

    private readonly List<WorldDataEntry> _data;

    public CurrencyData(List<WorldDataEntry> data) => _data = data;

    /// <summary>Current held amount (ps_item_Currency).</summary>
    public long Balance
    {
        get => GetValue(KeyBalance);
        set => SetValue(KeyBalance, value);
    }

    /// <summary>Total ever collected (ps_TotalCollectCount_Currency). Must be ≥ Balance.</summary>
    public long TotalCollected
    {
        get => GetValue(KeyTotalCollected);
        set => SetValue(KeyTotalCollected, value);
    }

    /// <summary>Lifetime stat tracker (ps_StatId_AllTimeCollectedGlimmer).</summary>
    public long AllTimeGlimmer
    {
        get => GetValue(KeyAllTimeGlimmer);
        set => SetValue(KeyAllTimeGlimmer, value);
    }

    /// <summary>True when at least the balance entry exists in World.Data.</summary>
    public bool IsPresent => _data.Any(e => e.Id == KeyBalance);

    /// <summary>
    /// Write all three fields and raise totals to satisfy game constraints.
    /// Returns true when any total was adjusted upward.
    /// </summary>
    public bool Apply(long balance, long totalCollected, long allTimeGlimmer)
    {
        var safeTotal = Math.Max(totalCollected, balance);
        var safeGlimmer = Math.Max(allTimeGlimmer, safeTotal);

        Balance = balance;
        TotalCollected = safeTotal;
        AllTimeGlimmer = safeGlimmer;

        return safeTotal != totalCollected || safeGlimmer != allTimeGlimmer;
    }

    private long GetValue(string id)
    {
        var entry = _data.FirstOrDefault(e => e.Id == id);
        if (entry is null)
            return 0;
        var wv = entry.GetValue();
        return wv.IsInt ? wv.AsInt() : wv.IsLong ? wv.AsLong() : 0;
    }

    private void SetValue(string id, long value)
    {
        var entry = _data.FirstOrDefault(e => e.Id == id);
        if (entry is null)
            return;
        var current = entry.GetValue();
        // Preserve the original type code when re-encoding.
        var updated = current.TypeCode switch
        {
            'L' => WorldDataValue.FromLong(value),
            _ => WorldDataValue.FromInt((int)Math.Clamp(value, int.MinValue, int.MaxValue)),
        };
        entry.SetValue(updated);
    }
}