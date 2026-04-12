using ConstanceSaveEditor.Core.Models;

namespace ConstanceSaveEditor.Core.Editing;

public sealed record InspirationEntry(string Name, bool IsCollected, bool IsUpgraded);

/// <summary>
/// Read-only snapshot of collectible progress and stats read from <see cref="WorldData.Data"/>.
/// </summary>
public sealed class ProgressData
{
    public const int TotalHeartPieces = 9;
    public const int TotalPaintFlasks = 12;
    public const int TotalPolaroids = 30;
    public const int TotalInspirations = 16;

    private static readonly string[] InspirationKeys =
    [
        "Copycat",
        "DarkLightning",
        "DarkRevival",
        "MilkshakeRemix",
        "NaturalTwenty",
        "PaintOverload",
        "PogoWave",
        "PotentPotion",
        "PotionSynthesizer",
        "ScrewJump",
        "SliceConductor",
        "StabLifesteal",
        "StabProjectile",
        "SturdyKnight",
        "TaxRefund",
        "TeleportClone",
    ];

    // --- Collectibles ---
    public int HeartPieces { get; }
    public int PaintFlasks { get; }
    public int Polaroids { get; }
    public int Erasers { get; }
    public int LightStones { get; }

    // --- Inspirations ---
    public IReadOnlyList<InspirationEntry> Inspirations { get; }

    // --- Stats ---
    public int EnemiesKilled { get; }
    public int DashCount { get; }
    public int BrushHits { get; }
    public int SplooshCount { get; }
    public int ReceivedDamage { get; }
    public int TimesElectrocuted { get; }
    public int TimesPersevered { get; }
    public int GameOversByCorruption { get; }
    public int SnapshotsTaken { get; }
    public int ClonesUsed { get; }
    public int ShrineVisits { get; }
    public int TimespetCat { get; }

    public ProgressData(List<WorldDataEntry> data)
    {
        HeartPieces = GetInt(data, "ps_item_HeartPiece");
        PaintFlasks = GetInt(data, "ps_item_PaintPiece");
        Polaroids = GetInt(data, "ps_item_Polaroid");
        Erasers = GetInt(data, "ps_item_Eraser");
        LightStones = GetInt(data, "ps_item_LightStone");

        Inspirations = InspirationKeys
            .Select(key => new InspirationEntry(
                SplitPascalCase(key),
                GetInt(data, $"ps_inspDrawing_{key}") == 1,
                GetBool(data, $"ps_inspDrawing_{key}_Upgraded")))
            .ToList();

        EnemiesKilled = GetInt(data, "ps_StatId_EnemiesKilled");
        DashCount = GetInt(data, "ps_StatId_DashCount");
        BrushHits = GetInt(data, "ps_StatId_BrushHits");
        SplooshCount = GetInt(data, "ps_StatId_SplooshCount");
        ReceivedDamage = GetInt(data, "ps_StatId_ReceivedDamage");
        TimesElectrocuted = GetInt(data, "ps_StatId_Electrocuted");
        TimesPersevered = GetInt(data, "ps_StatId_Persevered");
        GameOversByCorruption = GetInt(data, "ps_StatId_GameOverByCorruption");
        SnapshotsTaken = GetInt(data, "ps_StatId_SnapshotsTaken");
        ClonesUsed = GetInt(data, "ps_StatId_CloneCount");
        ShrineVisits = GetInt(data, "ps_StatId_BackToShrine");
        TimespetCat = GetInt(data, "ps_StatId_PetTheCat");
    }

    private static int GetInt(List<WorldDataEntry> data, string id)
    {
        var entry = data.FirstOrDefault(e => e.Id == id);
        if (entry is null)
            return 0;
        var wv = entry.GetValue();
        return wv.IsInt ? wv.AsInt() : 0;
    }

    private static bool GetBool(List<WorldDataEntry> data, string id)
    {
        var entry = data.FirstOrDefault(e => e.Id == id);
        if (entry is null)
            return false;
        var wv = entry.GetValue();
        return wv.IsBool && wv.AsBool();
    }

    private static string SplitPascalCase(string s)
    {
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < s.Length; i++)
        {
            if (i > 0 && char.IsUpper(s[i]))
                sb.Append(' ');
            sb.Append(s[i]);
        }
        return sb.ToString();
    }
}