using System.Text.Json;

namespace ConstanceSaveEditor.UI.Services;

public sealed class RecentFilesService
{
    private const int MaxCount = 10;
    private readonly string _filePath;

    public RecentFilesService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ConstanceSaveEditor");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "recent.json");
    }

    public IReadOnlyList<string> Load()
    {
        if (!File.Exists(_filePath))
            return [];
        try
        {
            return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_filePath)) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Add(string path)
    {
        var current = Load().ToList();
        current.Remove(path);
        current.Insert(0, path);
        if (current.Count > MaxCount)
            current.RemoveRange(MaxCount, current.Count - MaxCount);
        File.WriteAllText(_filePath, JsonSerializer.Serialize(current));
    }

    public void Remove(string path)
    {
        var current = Load().ToList();
        if (current.Remove(path))
            File.WriteAllText(_filePath, JsonSerializer.Serialize(current));
    }
}