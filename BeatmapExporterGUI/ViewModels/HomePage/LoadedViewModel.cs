using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Localization;
using System.Linq;

namespace BeatmapExporterGUI.ViewModels.HomePage
{
    /// <summary>
    /// Page displayed when a lazer database is loaded, displaying basic database stats.
    /// </summary>
    public class LoadedViewModel : ViewModelBase
    {
        public LoadedViewModel()
        {
        }

        /// <summary>
        /// The LazerExporter instance currently loaded.
        /// </summary>
        public LazerExporter Lazer => Exporter.Lazer!;

        /// <summary>
        /// Reference to the filters currently applied to this LazerExporter.
        /// </summary>
        public int Filters => Lazer.Configuration.Filters.Count();

        /// <summary>
        /// The export mode currently selected on this LazerExporter.
        /// </summary>
        public string ExportMode => Lazer.Configuration.ExportFormat.UnitName();

        /// <summary>
        /// Localized display strings for home page dashboard.
        /// </summary>
        public string TotalBeatmapSets => LocalizationService.Instance.Format("Home.BeatmapSets", Lazer.TotalBeatmapSetCount);
        public string TotalBeatmapDiffs => LocalizationService.Instance.Format("Home.BeatmapDiffs", Lazer.TotalBeatmapCount);
        public string TotalCollections => LocalizationService.Instance.Format("Home.BeatmapCollections", Lazer.CollectionCount);
        public string SelectedSets => LocalizationService.Instance.Format("Home.SetsSelected", Lazer.SelectedBeatmapSetCount);
        public string SelectedDiffs => LocalizationService.Instance.Format("Home.DiffsSelected", Lazer.SelectedBeatmapCount);
        public string FiltersInfo => LocalizationService.Instance.Format("Home.FiltersApplied", Filters);
        public string ExportModeInfo => LocalizationService.Instance.Format("Home.ExportMode", Lazer.Configuration.ExportFormat.UnitName());
    }
}
