using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Filters;
using BeatmapExporterCore.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.Settings
{
    /// <summary>
    /// Page allowing users to configure the beatmap export. Currently contains beatmap filtering on the left half and general/advanced export options on the right half.
    /// </summary>
    public partial class ExportConfigViewModel : ViewModelBase
    {
        private readonly OuterViewModel outerViewModel;

        public ExportConfigViewModel(OuterViewModel outer)
        {
            outerViewModel = outer;
            ExportModes = ExportFormats.All().Select(format => format.UnitName());
            BeatmapFilters = new List<string>();
            Task.Run(() => UpdateBeatmapFilters());
            SelectedFilterIndex = -1;
        }

        /// <summary>
        /// Reference to the currently loaded <see cref="LazerExporter" />
        /// </summary>
        public LazerExporter Lazer => Exporter.Lazer!;

        protected ExporterConfiguration Config => Exporter.Configuration!;

        #region Beatmap Filters
        /// <summary>
        /// String representations for all the currently applied beatmap filters on the exporter.
        /// </summary>
        [ObservableProperty]
        private IEnumerable<string> _BeatmapFilters;

        /// <summary>
        /// Updates current beatmap filter list (computation must be done on Realm thread)
        /// </summary>
        private async Task UpdateBeatmapFilters()
        {
            await Exporter.RealmScheduler.Schedule(() =>
            {
                BeatmapFilters = Exporter.Lazer!.Filters()
                    .Select(filter => $"+ {filter.Description} ({filter.DiffCount} beatmaps)")
                    .ToList(); // ToList is essential to build filter list on the RealmScheduler thread only

                Exporter.Lazer.UpdateSelectedBeatmaps();
            });

            OnPropertyChanged(nameof(ShouldDisplayFilterMode));
            OnPropertyChanged(nameof(SelectionSummary));
            ResetFiltersCommand.NotifyCanExecuteChanged();
            RemoveSelectedFilterCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// String describing the currently selected beatmap set and diff counts versus the total counts, on two lines.
        /// </summary>
        public string SelectionSummary => LocalizationService.Instance.Format("Config.SelectionSummary",
            Lazer.SelectedBeatmapSetCount, Lazer.TotalBeatmapSetCount,
            Lazer.SelectedBeatmapCount, Lazer.TotalBeatmapCount);

        /// <summary>
        /// The currently selected beatmap filter, indexed 1:1 to <see cref="ExporterConfiguration.Filters" /> for this exporter
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveSelectedFilterCommand))]
        private int _SelectedFilterIndex;

        /// <summary>
        /// If a filter is currently selected by the user for deletion.
        /// </summary>
        public bool IsFilterSelected => SelectedFilterIndex != -1;

        /// <summary>
        /// If any filters are currently registered.
        /// </summary>
        public bool IsResettable => Config.Filters.Count > 0;

        /// <summary>
        /// User-requested action to remove the currently selected beatmap filter.
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(IsFilterSelected))]
        private async Task RemoveSelectedFilter()
        {
            Config.Filters.RemoveAt(SelectedFilterIndex);
            await UpdateBeatmapFilters();
        }

        /// <summary>
        /// User-requested action to remove all active beatmap filters.
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(IsResettable))]
        private async Task ResetFilters()
        {
            Config.Filters.Clear();
            await UpdateBeatmapFilters();
        }

        /// <summary>
        /// User-requested action to change the application view to the beatmap list.
        /// </summary>
        [RelayCommand]
        private void ListBeatmaps() => outerViewModel.ListBeatmaps();

        /// <summary>
        /// The current beatmap filter 'builder' displayed to the user, if one exists.
        /// </summary>
        [ObservableProperty]
        private NewFilterViewModel? _CurrentFilterCreationControl;

        /// <summary>
        /// User-requested action to create a new filter 'builder'. Any existing builder will be lost.
        /// </summary>
        public void CreateFilterBuilder() => CurrentFilterCreationControl = new(this);

        /// <summary>
        /// User-requested action to delete the active filter 'builder'.
        /// </summary>
        public void CancelFilterBuilder() => CurrentFilterCreationControl = null;

        /// <summary>
        /// User-requested action to save the current filter 'builder' as a real active beatmap filter.
        /// </summary>
        public async Task ApplyFilterBuilder(BeatmapFilter filter)
        {
            Config.Filters.Add(filter);
            await UpdateBeatmapFilters();
            CancelFilterBuilder();
        }

        /// <summary>
        /// If the beatmap filter logic mode is currently set to AND by the user.
        /// </summary>
        public bool CombineFilterMode
        {
            get => Config.CombineFilterMode;
            set
            {
                Config.CombineFilterMode = value;
                Task.Run(() => UpdateBeatmapFilters());
            }
        }

        public bool ShouldDisplayFilterMode => Config.Filters.Count > 1;
        #endregion

        #region Advanced Export Settings
        /// <summary>
        /// Current export format selected.
        /// Export format in UI will be kept 1:1 to ExportFormat enum's int value
        /// </summary>
        public int SelectedExportIndex
        {
            get => (int)Config.ExportFormat;
            set
            {
                Config.ExportFormat = (ExportFormat)value;
                OnPropertyChanged(nameof(ModeDescriptor));
                OnPropertyChanged(nameof(ExportUnit));
                OnPropertyChanged(nameof(ExportUnitInfo));
                OnPropertyChanged(nameof(ExportPath));
                OnPropertyChanged(nameof(CompressionAvailable));
                OnPropertyChanged(nameof(IsAudioExport));
                OnPropertyChanged(nameof(IsCollectionDbExport));
                OnPropertyChanged(nameof(ShouldDisplayMergeOptions));
            }
        }

        /// <summary>
        /// List of all supported export formats in user friendly form
        /// </summary>
        [ObservableProperty]
        private IEnumerable<string> _ExportModes = [];

        /// <summary>
        /// Descriptor string for the currently selected export format 
        /// </summary>
        public string ModeDescriptor
        {
            get => Config.ExportFormat.Descriptor();
        }

        /// <summary>
        /// Reference to the current full file export directory.
        /// </summary>
        public string ExportPath => Config.FullPath;

        /// <summary>
        /// User-requested action to change the current export path. Opens an additional dialog for directory selection.
        /// </summary>
        public async Task SelectExportPath()
        {
            var selectDir = await App.Current.DialogService.SelectDirectoryAsync(ExportPath);
            if (selectDir != null)
            {
                Config.ExportPath = selectDir;
                OnPropertyChanged(nameof(ExportPath));
            }
        }

        public void OpenExportDirectory() => Exporter.Lazer.SetupExport();

        /// <summary>
        /// If compression is an available option for the current export mode.
        /// </summary>
        public bool CompressionAvailable => Config.CompressionAvailable;

        /// <summary>
        /// If beatmap export compression is currently enabled by the user.
        /// </summary>
        public bool CompressionEnabled
        {
            get => Config.CompressionEnabled;
            set
            {
                Config.CompressionEnabled = value;
                OnPropertyChanged(nameof(CompressionDescriptor));
            }
        }

        /// <summary>
        /// Description of the current <see cref="CompressionEnabled" /> setting, suitable for user display.
        /// </summary>
        public string CompressionDescriptor => CompressionEnabled
            ? LocalizationService.Instance["Config.CompressionSlow"]
            : LocalizationService.Instance["Config.CompressionFast"];
        
        /// <summary>
        /// If the export mode is currently set to export audio files.
        /// </summary>
        public bool IsAudioExport => Config.ExportFormat == ExportFormat.Audio;

        /// <summary>
        /// If audio export is currently set to only use .mp3 files.
        /// </summary>
        public bool Mp3ExportEnabled
        {
            get => Config.ExportMp3;
            set
            {
                Config.ExportMp3 = value;
                OnPropertyChanged(nameof(AudioExportDescriptor));
            }
        }

        /// <summary>
        /// Description of the current <see cref="Mp3ExportEnabled" /> setting, suitable for user display.
        /// </summary>
        public string AudioExportDescriptor => Lazer.AudioTranscodeInfo();

        /// <summary>
        /// If the export mode is currently set to export a collection.db file.
        /// </summary>
        public bool IsCollectionDbExport => Config.ExportFormat == ExportFormat.CollectionDb;

        /// <summary>
        /// If collection.db export merging is currently enabled by the user.
        /// </summary>
        public bool MergeCollectionsEnabled
        {
            get => Config.MergeCollections;
            set
            {
                Config.MergeCollections = value;
                OnPropertyChanged(nameof(MergeCollectionsDescriptor));
                OnPropertyChanged(nameof(ShouldDisplayMergeOptions));
            }
        }

        /// <summary>
        /// Description of the current <see cref="MergeCollectionsEnabled" /> setting, suitable for user display.
        /// </summary>
        public string MergeCollectionsDescriptor => MergeCollectionsEnabled
            ? LocalizationService.Instance["Config.MergeDescriptor"]
            : LocalizationService.Instance["Config.MergeCollectionsDisabled"];

        /// <summary>
        /// If additional options for collection.db export should be displayed
        /// </summary>
        public bool ShouldDisplayMergeOptions => IsCollectionDbExport && MergeCollectionsEnabled;

        /// <summary>
        /// If collection.db export is set as case insensitive by the user.
        /// </summary>
        public bool MergeCaseInsensitive
        {
            get => Config.MergeCaseInsensitive;
            set
            {
                Config.MergeCaseInsensitive = value;
                OnPropertyChanged(nameof(MergeCaseDescriptor));
            }
        }

        /// <summary>
        /// Description of the current <see cref="MergeCaseInsensitive" /> setting, suitable for user display.
        /// </summary>
        public string MergeCaseDescriptor => MergeCaseInsensitive
            ? LocalizationService.Instance["Config.MergeCaseDescriptor"]
            : LocalizationService.Instance["Config.MergeCaseDisabled"];

        /// <summary>
        /// String containing the current type of file that will be exported
        /// </summary>
        public string ExportUnit => Config.ExportFormat.UnitName();

        #region Language Selection
        /// <summary>
        /// All available locale options for the user.
        /// </summary>
        public IReadOnlyList<LocaleInfo> AvailableLanguages => LocalizationService.Instance.AvailableLocales;

        /// <summary>
        /// Index of the currently selected language.
        /// </summary>
        public int SelectedLanguageIndex
        {
            get
            {
                var current = LocalizationService.Instance.CurrentLocale;
                for (int i = 0; i < AvailableLanguages.Count; i++)
                {
                    if (AvailableLanguages[i].Equals(current))
                        return i;
                }
                return 0;
            }
            set
            {
                if (value >= 0 && value < AvailableLanguages.Count)
                {
                    var locale = AvailableLanguages[value];
                    LocalizationService.Instance.CurrentLocale = locale;
                    Exporter.Configuration.ClientSettings.SaveLanguage(locale.Culture);
                    RefreshLocalizedContent();
                }
            }
        }

        private void RefreshLocalizedContent()
        {
            ExportModes = ExportFormats.All().Select(format => format.UnitName()).ToList();
            OnPropertyChanged(nameof(SelectedExportIndex));
            OnPropertyChanged(nameof(ModeDescriptor));
            OnPropertyChanged(nameof(ExportUnit));
            OnPropertyChanged(nameof(ExportUnitInfo));
            OnPropertyChanged(nameof(SelectionSummary));
            OnPropertyChanged(nameof(CompressionDescriptor));
            OnPropertyChanged(nameof(AudioExportDescriptor));
            OnPropertyChanged(nameof(MergeCollectionsDescriptor));
            OnPropertyChanged(nameof(MergeCaseDescriptor));
            OnPropertyChanged(nameof(SelectedLanguageIndex));
        }
        #endregion

        public string ExportUnitInfo => LocalizationService.Instance.Format("Config.ExportSelection", ExportUnit);

        /// <summary>
        /// Reference to the Export Beatmaps command functionality for this page to allow an alternate method to begin exporting.
        /// </summary>
        public IAsyncRelayCommand ExportBeatmapsCommand => outerViewModel.MenuRow.ExportCommand;
        #endregion
    }
}
