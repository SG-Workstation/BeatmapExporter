using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Localization;
using System.Diagnostics.CodeAnalysis;

namespace BeatmapExporterCLI.Interface
{
    /// <summary>
    /// Class containing the main application logic for CLI flow 
    /// </summary>
    public class ExporterApp
    {
        public ExporterApp(LazerExporterCLI cli)
        {
            CLI = cli;
            Exporter = cli.Exporter;
            Configuration = Exporter.Configuration;
        }

        /// <summary>
        /// Reference to the CLI-specific component container 
        /// </summary>
        public LazerExporterCLI CLI { get; }

        /// <summary>
        /// Reference to the general lazer exporter component container
        /// </summary>
        public LazerExporter Exporter { get; }

        /// <summary>
        /// Reference to the current configuration for export for the user
        /// </summary>
        public ExporterConfiguration Configuration { get; }
            
        /// <summary>
        /// Begins the infinite loop for the main application I/O flow.
        /// </summary>
        public void StartApplicationLoop()
        {
            while (true)
            {
                ApplicationLoop();
            }
        }

        /// <summary>
        /// Exits the program after blocking for user acknowledgement. 
        /// </summary>
        [DoesNotReturn]
        public static void Exit()
        {
            // keep console open
            Console.Write(LocalizationService.Instance["CLI.ExitPrompt"]);
            Console.ReadKey();
            Environment.Exit(0);
        }

        /// <summary>
        /// Primary CLI user interaction flow.
        /// </summary>
        void ApplicationLoop()
        {
            // output main application menu
            var exportMode = Configuration.ExportFormat == ExportFormat.Skins
                ? LocalizationService.Instance.Format("CLI.SkinExportDesc", Exporter.Skins.Count)
                : LocalizationService.Instance.Format("CLI.BeatmapExportDesc", Configuration.ExportFormat.UnitName(), Exporter.SelectedBeatmapSetCount, Exporter.SelectedBeatmapCount);
            Console.Write(LocalizationService.Instance.Format("CLI.MenuExportItem", exportMode));
            Console.Write(LocalizationService.Instance.Format("CLI.MenuDisplayItem", Exporter.SelectedBeatmapSetCount, Exporter.TotalBeatmapSetCount));
            Console.Write(LocalizationService.Instance.Format("CLI.MenuCollectionsItem", Exporter.CollectionCount));
            Console.Write(LocalizationService.Instance["CLI.MenuAdvancedItem"]);
            Console.Write(LocalizationService.Instance["CLI.MenuFilterItem"]);
            Console.Write(LocalizationService.Instance["CLI.MenuExitItem"]);
            Console.Write(LocalizationService.Instance["CLI.MenuSelectPrompt"]);

            string? input = Console.ReadLine();
            if (input is null)
            {
                ExporterApp.Exit();
            }

            if (!int.TryParse(input, out int op) || op is < 0 or > 5)
            {
                Console.WriteLine(LocalizationService.Instance["CLI.InvalidOperation"]);
                return;
            }

            switch (op)
            {
                case 0:
                    Environment.Exit(0);
                    break;
                case 1:
                    switch (Configuration.ExportFormat)
                    {
                        case ExportFormat.Beatmap:
                        case ExportFormat.Folder:
                            CLI.ExportBeatmaps();
                            break;
                        case ExportFormat.Audio:
                            CLI.ExportAudioFiles();
                            break;
                        case ExportFormat.Background:
                            CLI.ExportBackgroundFiles();
                            break;
                        case ExportFormat.Replay:
                            CLI.ExportReplays();
                            break;
                        case ExportFormat.Skins:
                            CLI.ExportSkins();
                            break;
                        case ExportFormat.CollectionDb:
                            CLI.ExportCollectionDb();
                            break;
                    }
                    break;
                case 2:
                    CLI.DisplaySelectedBeatmaps();
                    break;
                case 3:
                    CLI.DisplayCollections();
                    break;
                case 4:
                    CLI.StartExportConfigurator();
                    break;
                case 5:
                    CLI.BeatmapFilterSelection();
                    break;
            }
        }
    }
}
