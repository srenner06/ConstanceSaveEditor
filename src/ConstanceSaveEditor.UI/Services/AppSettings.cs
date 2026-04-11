namespace ConstanceSaveEditor.UI.Services;

public sealed record AppSettings
{
    public string Language { get; init; } = "en";
    public string Theme { get; init; } = "System";
}