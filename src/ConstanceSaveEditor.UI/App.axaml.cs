using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using ConstanceSaveEditor.Core.Backup;
using ConstanceSaveEditor.Core.Codec;
using ConstanceSaveEditor.Core.Services;
using ConstanceSaveEditor.UI.Services;
using ConstanceSaveEditor.UI.ViewModels;

namespace ConstanceSaveEditor.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settingsService = new SettingsService();
            var settings = settingsService.Load();

            ApplyTheme(settings.Theme);
            ApplyLanguage(settings.Language);

            var window = new MainWindow();
            var fileService = new SaveFileService(
                new SaveFileCodec(),
                new SaveDocumentService(),
                new BackupService());
            window.DataContext = new MainWindowViewModel(
                fileService,
                new DialogService(window),
                new RecentFilesService(),
                settingsService,
                settings);
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    internal void ApplyTheme(string theme)
    {
        RequestedThemeVariant = theme switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }

    internal void ApplyLanguage(string language)
    {
        var uri = language == "de"
            ? "avares://ConstanceSaveEditor.UI/Assets/Strings/de.axaml"
            : "avares://ConstanceSaveEditor.UI/Assets/Strings/en.axaml";
        var dicts = Resources.MergedDictionaries;
        var include = new ResourceInclude((Uri?)null) { Source = new Uri(uri) };
        if (dicts.Count > 0)
            dicts[0] = include;
        else
            dicts.Add(include);
    }
}