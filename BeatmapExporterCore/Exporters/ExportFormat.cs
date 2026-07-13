using BeatmapExporterCore.Localization;

namespace BeatmapExporterCore.Exporters
{
    /// <summary>
    /// All available modes of exporting.
    /// </summary>
    public enum ExportFormat { Beatmap, Audio, Background, Replay, Skins, Folder, CollectionDb };

    public static class ExportFormatExtensions
    {
        /// <summary>
        /// A string describing what this export format targets. Simple, for inlining, for example: "beatmap backgrounds"
        /// </summary>
        public static string UnitName(this ExportFormat format) => format switch
        {
            ExportFormat.Beatmap => LocalizationService.Instance["ExportFormat.Beatmap"],
            ExportFormat.Audio => LocalizationService.Instance["ExportFormat.Audio"],
            ExportFormat.Background => LocalizationService.Instance["ExportFormat.Background"],
            ExportFormat.Replay => LocalizationService.Instance["ExportFormat.Replay"],
            ExportFormat.Skins => LocalizationService.Instance["ExportFormat.Skins"],
            ExportFormat.Folder => LocalizationService.Instance["ExportFormat.Folder"],
            ExportFormat.CollectionDb => LocalizationService.Instance["ExportFormat.CollectionDb"],
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// A string describing the actions that the export mode will perform.
        /// </summary>
        public static string Descriptor(this ExportFormat format) => format switch
        {
            ExportFormat.Beatmap => LocalizationService.Instance["ExportFormat.BeatmapDesc"],
            ExportFormat.Audio => LocalizationService.Instance["ExportFormat.AudioDesc"],
            ExportFormat.Background => LocalizationService.Instance["ExportFormat.BackgroundDesc"],
            ExportFormat.Replay => LocalizationService.Instance["ExportFormat.ReplayDesc"],
            ExportFormat.Skins => LocalizationService.Instance["ExportFormat.SkinsDesc"],
            ExportFormat.Folder => LocalizationService.Instance["ExportFormat.FolderDesc"],
            ExportFormat.CollectionDb => LocalizationService.Instance["ExportFormat.CollectionDbDesc"],
            _ => throw new NotImplementedException() 
        };

        /// <summary>
        /// The 'next' ExportFormat, based on the natural ordering of the ExportFormat enum.
        /// </summary>
        public static ExportFormat Next(this ExportFormat format)
        {
            var max = Enum.GetValues(typeof(ExportFormat)).Length;
            var iNext = ((int)format + 1) % max;
            return (ExportFormat)iNext;
        }
    }

    public static class ExportFormats
    {
        /// <summary>
        /// Array of all ExportFormat enum values.
        /// </summary>
        public static IEnumerable<ExportFormat> All() => (ExportFormat[])Enum.GetValues(typeof(ExportFormat));
    }
}
