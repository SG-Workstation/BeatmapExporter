using BeatmapExporterCore.Localization;
using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Utilities;

namespace BeatmapExporterCore.Exporters
{
    /// <summary>
    /// Base class for BeatmapExporterCore exceptions
    /// </summary>
    [Serializable]
    public class ExporterException : Exception
    {
        public ExporterException() { }
        public ExporterException(string message) : base(message) { }
        public ExporterException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Exception thrown when the osu!lazer database indicates a version mismatch
    /// </summary>
    public class LazerVersionException : ExporterException
    {
        private LazerVersionException(string message, IEnumerable<string> details) : base(message)
        {
            Details = details;
        }

        public IEnumerable<string> Details { get; }

        /// <summary>
        /// Build an exception for a database schema version mismatch with messages for the user depending on if the file is too new/old
        /// </summary>
        public static LazerVersionException Schema(int databaseVersion, string rawMessage) => new LazerVersionException(rawMessage, SchemaDetails(databaseVersion));

        private static IEnumerable<string> SchemaDetails(int fileSchema)
        {
            int exporterSchema = LazerDatabase.LazerSchemaVersion;
            string exporterVersion = ExporterUpdater.FeatureVersion;
            if (fileSchema > exporterSchema)
            {
                yield return LocalizationService.Instance.Format("Exception.DatabaseSuffix", fileSchema, exporterVersion, exporterSchema);
                yield return LocalizationService.Instance["Exception.DatabaseDefault"];
            } else
            {
                yield return LocalizationService.Instance.Format("Exception.DatabaseForced", fileSchema);
                yield return LocalizationService.Instance["Exception.DatabaseDefault"];
                yield return LocalizationService.Instance.Format("Exception.DatabaseSuffix", fileSchema);
                yield return LocalizationService.Instance["Exception.DatabaseDefault"];
                yield return LocalizationService.Instance.Format("Exception.DatabaseSuffix", fileSchema, ExporterUpdater.Releases);
            }
        }

        /// <summary>
        /// Build an exception for a database "file format" mismatch, which requires a newer version of Realm
        /// </summary>
        /// <param name="fileFormat"></param>
        /// <returns></returns>
        public static LazerVersionException FileFormat(int fileFormat, string rawMessage) => new LazerVersionException(rawMessage, UpgradeDetails(fileFormat));

        private static IEnumerable<string> UpgradeDetails(int fileFormat)
        {
            yield return LocalizationService.Instance.Format("Exception.DatabaseIncompatible", fileFormat);
            yield return LocalizationService.Instance["Exception.DatabaseDefault"];
        }
    }
}
