using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Filters;
using BeatmapExporterCore.Localization;
using System.Text;

namespace BeatmapExporterCLI.Interface
{
    /// <summary>
    /// Class wrapping a LazerExporter into a CLI interface, outputting progress and all data to console
    /// Contains methods that are used to produce CLI-specific outputs
    /// </summary>
    public class LazerExporterCLI
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public LazerExporterCLI(LazerExporter exporter)
        {
            Exporter = exporter;
        }

        public LazerExporter Exporter { get; }

        public ExporterConfiguration Configuration => Exporter.Configuration;
         
        public void ExportBeatmaps()
        {
            Exporter.SetupExport();
            int attempted = 0, exported = 0;
            int count = Exporter.SelectedBeatmapSetCount;
            Console.WriteLine(LocalizationService.Instance.Format("CLI.ExportSelectedSets", Exporter.SelectedBeatmapSetCount));
            foreach (var mapset in Exporter.SelectedBeatmapSets)
            {
                string? filename = null;
                attempted++;
                try
                {
                    // beatmap 'folder' export and '.osz' export are nearly identical processes
                    if (Configuration.ExportFormat == ExportFormat.Folder)
                    {
                        // exporting beatmap set as unarchived folder for use directly with osu! stable
                        Exporter.ExportBeatmapFolder(mapset, out filename);
                    } else
                    {
                        // exporting beatmap set as .osz archive
                        Exporter.ExportBeatmap(mapset, out filename);
                    }
                    exported++;
                    Console.WriteLine(LocalizationService.Instance.Format("Export.Progress", attempted, count, filename));
                } catch (Exception e)
                {
                    Console.WriteLine(LocalizationService.Instance.Format("Export.Error", filename ?? "?", e.Message));
                    Logger.Error(e);
                }
            };
            Console.WriteLine(LocalizationService.Instance.Format("Export.Complete", exported, count, Configuration.FullPath));
        }

        public void ExportAudioFiles()
        {
            Exporter.SetupExport();
            Console.WriteLine(LocalizationService.Instance.Format("CLI.ExportAudioIntro", Exporter.SelectedBeatmapSetCount));
            Console.WriteLine(Exporter.AudioTranscodeInfo());

            int attempted = 0, exportedAudio = 0;
            foreach (var mapset in Exporter.SelectedBeatmapSets)
            {
                var allAudio = Exporter.ExtractAudio(mapset);
                
                foreach (var audioExport in allAudio)
                {
                    attempted++;
                    string audioFile = audioExport.AudioFile.AudioFile;
                    var transcode = audioExport.TranscodeFrom != null;
                    var transcodeNotice = transcode ? LocalizationService.Instance.Format("CLI.TranscodeNotice", audioExport.TranscodeFrom) : "";
                    try
                    {
                        Console.WriteLine(LocalizationService.Instance.Format("Export.AudioProgress", attempted, audioExport.OutputFilename, transcodeNotice));
                        if (transcode && !Exporter.TranscodeAvailable)
                        {
                            Console.WriteLine(LocalizationService.Instance.Format("Export.AudioSkip", audioFile));
                            continue;
                        }

                        void metadataFailure(Exception e) => Console.WriteLine(LocalizationService.Instance.Format("Export.AudioMetaError", audioExport.OutputFilename, e.Message));
                        Exporter.ExportAudio(audioExport, metadataFailure);
                        exportedAudio++;

                    } catch (TranscodeException te)
                    {
                        Console.WriteLine(LocalizationService.Instance.Format("Export.AudioTranscodeError", audioFile, te.Message));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(LocalizationService.Instance.Format("Export.AudioFileError", audioFile, e.Message));
                        Logger.Error(e);
                    }
                }
            }
            Console.WriteLine(LocalizationService.Instance.Format("Export.AudioComplete", exportedAudio, attempted, Exporter.SelectedBeatmapCount, Configuration.FullPath));
        }

        public void ExportBackgroundFiles()
        {
            Exporter.SetupExport();
            Console.WriteLine(LocalizationService.Instance.Format("Export.BackgroundIntro", Exporter.SelectedBeatmapSetCount));

            int attempted = 0, exported = 0;
            foreach (var mapset in Exporter.SelectedBeatmapSets)
            {
                var allImages = Exporter.ExtractBackgrounds(mapset);

                foreach (var imageExport in allImages)
                {
                    attempted++;
                    var backgroundFile = imageExport.BackgroundFile.BackgroundFile;

                    try
                    {
                        Console.WriteLine(LocalizationService.Instance.Format("Export.BackgroundProgress", attempted, imageExport.OutputFilename));
                        Exporter.ExportBackground(imageExport);
                        exported++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(LocalizationService.Instance.Format("Export.BackgroundError", backgroundFile, e.Message));
                        Logger.Error(e);
                    }
                }
            }
            Console.WriteLine(LocalizationService.Instance.Format("Export.BackgroundComplete", exported, attempted, Exporter.SelectedBeatmapCount, Configuration.FullPath));
        }
        
        public void ExportReplays()
        {
            Exporter.SetupExport();
            int exported = 0;

            var selectedReplays = Exporter.GetSelectedReplays();
            var replayCount = selectedReplays.Count();

            Console.WriteLine(LocalizationService.Instance.Format("Export.ReplayIntro", replayCount, Exporter.SelectedBeatmapCount));
            foreach (var replay in selectedReplays)
            {
                string? filename = null;
                try
                {
                    Exporter.ExportReplay(replay, out filename);
                    exported++;
                    Console.WriteLine(LocalizationService.Instance.Format("Export.ReplayProgress", exported, replayCount, filename));
                } catch (Exception e)
                {
                    Console.WriteLine(LocalizationService.Instance.Format("Export.ReplayError", filename ?? "?", e.Message));
                    Logger.Error(e);
                }
            }
            Console.WriteLine(LocalizationService.Instance.Format("Export.ReplayComplete", exported, replayCount, Exporter.SelectedBeatmapCount, Configuration.FullPath));
        }

        public void ExportSkins()
        {
            Exporter.SetupExport();
            int exported = 0;
            var skins = Exporter.Skins;
            var skinCount = skins.Count;
            
            Console.WriteLine(LocalizationService.Instance.Format("Export.SkinIntro", skinCount));
            foreach (var skin in skins)
            {
                string? filename = null;
                try
                {
                    Exporter.ExportSkin(skin, out filename);
                    exported++;
                    Console.WriteLine(LocalizationService.Instance.Format("Export.SkinProgress", exported, skinCount, filename));
                }
                catch (Exception e)
                {
                    Console.WriteLine(LocalizationService.Instance.Format("Export.SkinError", filename ?? "?", e.Message));
                    Logger.Error(e);
                }
            }
            Console.WriteLine(LocalizationService.Instance.Format("Export.SkinComplete", exported, skinCount, Configuration.FullPath));
        }

        public void ExportCollectionDb()
        {
            Exporter.SetupExport();
            try
            {
                var steps = Exporter.ExportCollectionDb();
                foreach (var step in steps)
                {
                    Console.WriteLine(LocalizationService.Instance.Format("Export.CollectionDbProgress", step.Name, step.IncludedDiffs, step.OriginalDiffs));
                }
                Console.WriteLine(LocalizationService.Instance.Format("Export.CollectionDbComplete", steps.Count));
            } catch (Exception e)
            {
                Console.WriteLine(LocalizationService.Instance.Format("Export.CollectionDbError", e.Message));
                Logger.Error(e);
                return;
            }
        }

        public void DisplaySelectedBeatmaps()
        {
            foreach (var map in Exporter.SelectedBeatmapSets)
            {
                Console.WriteLine(map.DiffSummary());
            }
        }

        public void DisplayCollections()
        {
            Console.Write($"{LocalizationService.Instance["CLI.CollectionsHeader"]}\n\n");
            foreach (var (name, (index, maps)) in Exporter.Collections)
            {
                Console.WriteLine(LocalizationService.Instance.Format("CLI.CollectionItem", index, name, maps.Count));
            }
            Console.Write($"\n{LocalizationService.Instance["CLI.CollectionFilterHint"]}\n");
        }

        public void StartExportConfigurator()
        {
            while (true)
            {
                StringBuilder settings = new();
                settings
                    .Append(LocalizationService.Instance["CLI.AdvancedSettings"])
                    .Append(LocalizationService.Instance["CLI.SettingNum1"]);

                var exportBeatmaps = Configuration.ExportFormat == ExportFormat.Beatmap;
                var formatId = (int)Configuration.ExportFormat + 1;
                var edited = exportBeatmaps ? "" : "*";

                settings
                    .Append(LocalizationService.Instance.Format("CLI.SettingExportFormat", formatId, Configuration.ExportFormat.Descriptor(), edited))
                    .Append(LocalizationService.Instance["CLI.SettingExportPathPrefix"])
                    .Append(Path.GetFullPath(Configuration.ExportPath));
                if (Configuration.ExportPath != Configuration.DefaultExportPath)
                    settings.Append('*');

                if (Configuration.CompressionAvailable)
                {
                    settings.Append(LocalizationService.Instance["CLI.SettingNum3"]);
                    if (Configuration.CompressionEnabled)
                        settings.Append(LocalizationService.Instance["CLI.CompressionEnabled"]);
                    else
                        settings.Append(LocalizationService.Instance["CLI.CompressionDisabled"]);
                }

                var exportCollectionDb = Configuration.ExportFormat == ExportFormat.CollectionDb;
                if (exportCollectionDb)
                {
                    settings.Append(LocalizationService.Instance["CLI.SettingNum3"]);
                    if (Configuration.MergeCollections)
                        settings.Append(LocalizationService.Instance["CLI.MergeEnabled"]);
                    else
                        settings.Append(LocalizationService.Instance["CLI.MergeDisabled"]);

                    settings.Append(LocalizationService.Instance["CLI.SettingNum4"]);
                    if (Configuration.MergeCaseInsensitive)
                        settings.Append(LocalizationService.Instance["CLI.MergeCaseInsensitive"]);
                    else
                        settings.Append(LocalizationService.Instance["CLI.MergeCaseSensitive"]);
                }

                var exportAudio = Configuration.ExportFormat == ExportFormat.Audio;
                if (exportAudio)
                {
                    settings.Append(LocalizationService.Instance["CLI.SettingNum3"]);
                    if (Configuration.ExportMp3)
                        settings.Append(LocalizationService.Instance["CLI.AudioMp3Only"]);
                    else 
                        settings.Append(LocalizationService.Instance["CLI.AudioOriginal"]);
                }

                settings.Append(LocalizationService.Instance["CLI.EditSettingPrompt"]);

                Console.Write(settings.ToString());
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int op) || op < 1 || op > (exportBeatmaps || exportCollectionDb || exportAudio ? 4 : 2))
                {
                    Console.Write(LocalizationService.Instance["CLI.InvalidOperationFull"]);
                    return;
                }

                switch (op)
                {
                    case 1:
                        Configuration.ExportFormat = Configuration.ExportFormat.Next();
                        break;
                    case 2:
                        Console.Write(LocalizationService.Instance.Format("CLI.ChangePathPrompt", Configuration.DefaultExportPath, Configuration.ExportPath));
                        string? pathInput = Console.ReadLine();
                        if (string.IsNullOrEmpty(pathInput))
                            continue;
                        Configuration.ExportPath = pathInput;
                        Console.WriteLine(LocalizationService.Instance.Format("CLI.ChangedExportPath", Path.GetFullPath(Configuration.ExportPath)));
                        break;
                    case 3:
                        if (Configuration.CompressionAvailable)
                        {
                            if (Configuration.CompressionEnabled)
                            {
                                Console.WriteLine(LocalizationService.Instance["CLI.CompressionDisabledMsg"]);
                                Configuration.CompressionEnabled = false;
                            }
                            else
                            {
                                Console.WriteLine(LocalizationService.Instance["CLI.CompressionEnabledMsg"]);
                                Configuration.CompressionEnabled = true;
                            }
                        } else if (exportCollectionDb)
                        {
                            if (Configuration.MergeCollections)
                            {
                                Console.WriteLine(LocalizationService.Instance["CLI.MergeDisabledMsg"]);
                                Configuration.MergeCollections = false;
                            } else
                            {
                                Console.WriteLine(LocalizationService.Instance["CLI.MergeEnabledMsg"]);
                                Configuration.MergeCollections = true;
                            }
                        }
                        else if (exportAudio)
                        {
                            if (Configuration.ExportMp3)
                            {
                                Console.WriteLine(LocalizationService.Instance["CLI.AudioAsIsMsg"]);
                                Configuration.ExportMp3 = false;
                            }
                            else
                            {
                                Console.WriteLine(LocalizationService.Instance["CLI.AudioMp3OnlyMsg"]);
                                Configuration.ExportMp3 = true;
                            }
                        }
                        break;
                    case 4:
                        if (exportCollectionDb)
                        {
                            if (Configuration.MergeCaseInsensitive)
                            {
                                Console.WriteLine(LocalizationService.Instance["CLI.MergeCaseSensitiveMsg"]);
                                Configuration.MergeCaseInsensitive = false;
                            }
                            else
                            {
                                Console.WriteLine(LocalizationService.Instance["CLI.MergeCaseInsensitiveMsg"]);
                                Configuration.MergeCaseInsensitive = true;
                            }
                        }
                        break;
                }
            }
        }

        #region Beatmap Filters
        public void BeatmapFilterSelection()
        {
            Console.Write(LocalizationService.Instance["CLI.BeatmapFilterHeader"]);

            Console.Write(LocalizationService.Instance["CLI.FilterHelpText"]);

            while (true)
            {
                var filters = Configuration.Filters;
                if (filters.Count > 0)
                {
                    Console.Write(LocalizationService.Instance["CLI.FilterSection"]);
                    Console.Write(FilterDetail());
                    Console.Write(LocalizationService.Instance.Format("CLI.MatchedSets", Exporter.SelectedBeatmapSetCount, Exporter.TotalBeatmapSetCount));
                }
                else
                {
                    Console.Write(LocalizationService.Instance["CLI.NoActiveFilters"]);
                }

                // start filter selection ui mode
                Console.Write(LocalizationService.Instance["CLI.SelectFilterPrompt"]);

                string? input = Console.ReadLine()?.ToLower();
                if (string.IsNullOrEmpty(input))
                {
                    return;
                }

                string[] command = input.Split(" ");
                // check for filter "remove" operations, otherwise pass to parse filter 
                switch (command[0])
                {
                    case "remove":
                        string? idArg = command.ElementAtOrDefault(1);
                        TryRemoveBeatmapFilter(idArg);
                        break;
                    case "reset":
                        ResetBeatmapFilters();
                        break;
                    case "exit":
                        return;
                    default:
                        // parse as new filter
                        BeatmapFilter? filter;
                        try
                        {
                            filter = new FilterParser(input).Parse();
                        } catch (ArgumentException ae)
                        {
                            Console.WriteLine(LocalizationService.Instance.Format("CLI.FilterInputError", ae.Message));
                            break;
                        }
                        if (filter is not null)
                        {
                            filters.Add(filter);
                            static void collectionFailure(string filter) => Console.WriteLine(LocalizationService.Instance.Format("CLI.CollectionNotFound", filter));
                            Exporter.UpdateSelectedBeatmaps(collectionFailure);
                            Console.Write(LocalizationService.Instance["CLI.FilterAdded"]);
                        }
                        else
                        {
                            Console.WriteLine(LocalizationService.Instance.Format("CLI.InvalidFilter", command[0]));
                        }
                        break;
                }
            }
        }

        public string FilterDetail()
        {
            var filterInfo = Exporter.Filters().Select(filter => LocalizationService.Instance.Format("CLI.FilterDetailItem", filter.Id, filter.Description, filter.DiffCount));
            return string.Join("\n", filterInfo);
        }

        void TryRemoveBeatmapFilter(string? idArg)
        {
            var filters = Configuration.Filters;
            if (idArg is null || !int.TryParse(idArg, out int id) || id < 1 || id > filters.Count)
            {
                Console.WriteLine(LocalizationService.Instance.Format("CLI.InvalidRuleId", idArg ?? "?"));
                return;
            }

            filters.RemoveAt(id - 1);
            Console.WriteLine(LocalizationService.Instance["CLI.FilterRemoved"]);
            Exporter.UpdateSelectedBeatmaps();
            return;
        }

        void ResetBeatmapFilters()
        {
            Configuration.Filters.Clear();
            Exporter.UpdateSelectedBeatmaps();
        }
        #endregion
    }
}
