using BeatmapExporterCore.Localization;
using BeatmapExporterGUI.Exporter;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BeatmapExporterGUI.ViewModels;

/// <summary>
/// Base ViewModel class which all BeatmapExporter pages should inherit from.
/// Provides access to the ExporterApp instance and localization service.
/// </summary>
public class ViewModelBase : ObservableObject
{
    public ViewModelBase()
    {
    }

    public ExporterApp Exporter => App.Current.Exporter;

    public LocalizationService Loc => LocalizationService.Instance;
}