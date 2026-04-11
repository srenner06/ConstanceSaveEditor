using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
            var window = new MainWindow();
            var fileService = new SaveFileService(
                new SaveFileCodec(),
                new SaveDocumentService(),
                new BackupService());
            window.DataContext = new MainWindowViewModel(fileService, new DialogService(window));
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}