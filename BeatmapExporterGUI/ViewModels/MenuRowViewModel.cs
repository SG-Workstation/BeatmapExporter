using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Localization;
using BeatmapExporterCore.Utilities;
using BeatmapExporterGUI.Exporter;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels;

/// <summary>
/// The top menu row/bar providing user access to all functions of the program.
/// </summary>
public partial class MenuRowViewModel : ViewModelBase
{
    private readonly OuterViewModel outer;

    public MenuRowViewModel(OuterViewModel outer)
    {
        this.outer = outer;
    }

    /// <summary>
    /// If an osu! database is currently loaded into the application.
    /// </summary>
    private bool DatabaseLoaded => Exporter.Lazer != null;

    /// <summary>
    /// If user navigation around the program should be allowed.
    /// </summary>
    private bool CanNavigate => !outer.IsExporting;

    /// <summary>
    /// If the user should be able to unload the osu! database or start an export.
    /// </summary>
    private bool CanExport => DatabaseLoaded && CanNavigate;

    /// <summary>
    /// User-requested action to exit the entire BeatmapExporter program.
    /// </summary>
    [RelayCommand]
    private void Exit() => ExporterApp.Exit();

    /// <summary>
    /// User-requested action to unload the currently loaded osu! database.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Close()
    {
        Exporter.Unload();
        outer.Home.SetNotLoaded();
        outer.NavigateHome();
    }
    
    // The below commands are user-requested navigation to specific program pages/functionality.

    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private void Home() => outer.NavigateHome();

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Beatmaps() => outer.ListBeatmaps();

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Collections() => outer.ListCollections();

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Configuration() => outer.EditFilters();

    [RelayCommand(IncludeCancelCommand = true, CanExecute = nameof(CanExport))]
    private async Task Export(CancellationToken token) => await outer.Export(token);

    // The below properties are references to the relevant BeatmapExporter version numbers

    public string VersionInfo => LocalizationService.Instance.Format("Menu.VersionInfo", Exporter.Lazer!.LazerVersionString);
    public string LazerReleaseVersion => LocalizationService.Instance.Format("Menu.LazerRelease", Exporter.Lazer!.LazerVersionString);
    public string DatabaseVersionInfo => LocalizationService.Instance.Format("Menu.LazerDbVersion", Exporter.Lazer!.DataSchemaVersion);

    // The below commands are all web links available for the user to open in browser.

    public void GitHub() => PlatformUtil.Open(ExporterUpdater.Project);

    public void Releases() => PlatformUtil.Open(ExporterUpdater.Releases);

    public void Osu() => PlatformUtil.Open("https://github.com/ppy/osu/releases");


    /// <summary>
    /// User-requested action to view the BeatmapExporter application data
    /// </summary>
    public void ApplicationData() => PlatformUtil.Open(ClientSettings.APPDIR);

    /// <summary>
    /// User-requested action to reset the program preferences to defaults
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private void Reset()
    {
        var defaults = new ClientSettings();
        // Overwrite config on disk with defaults
        defaults.TrySave();

        Exporter.AddSystemMessage(LocalizationService.Instance["App.SettingsReset"]);

        if (DatabaseLoaded)
        {
            Close();
        }
    }
}