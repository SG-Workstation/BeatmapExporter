using BeatmapExporterCLI.Interface;
using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using BeatmapExporterCore.Localization;
using BeatmapExporterCore.Utilities;
using Realms;

namespace BeatmapExporterCLI.Data
{
    public static class LazerLoader
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Attempt to locate and load the lazer database. May prompt user for the database path.
        /// </summary>
        public static ExporterApp Load(string? userDir)
        {
            // osu!lazer has been selected at this point. 
            // load the osu!lazer database here, can operate on lazer-specific objects
            Console.Write($"{LocalizationService.Instance["CLI.LazerTitle"]}\n\n");
            Console.Write(LocalizationService.Instance.Format("CLI.AppDataDir", ClientSettings.APPDIR));

            ClientSettings settings;
            try
            {
                // Load any previous user settings from file, or use defaults if this file does not exist.
                settings = ClientSettings.LoadFromFile();
            } catch (Exception e)
            {
                Console.Write($"\n{LocalizationService.Instance.Format("App.LoadSettingsError", e.Message)}\n");
                Console.Write(LocalizationService.Instance["CLI.UsingDefaults"]);
                settings = new();
            }
            
            // Attempt to load FFmpeg transcoder
            var transcoder = new Transcoder();
            Console.Write(transcoder.Available
                ? $"{LocalizationService.Instance["App.FFmpegLoaded"]}\n\n"
                : $"{LocalizationService.Instance["App.FFmpegNotFound"]}\n\n");
            

            Console.Write($"{LocalizationService.Instance["App.CheckingLocations"]}\n\n");
            List<string?> userDirs = [userDir, settings.DatabasePath];
            var checkDirs = userDirs.Concat(LazerDatabase.GetDefaultDirectories());
            string? dbFile = null;
            foreach (var dir in checkDirs)
            {
                // check each provided or default lazer directory
                if (dir is null) continue;
                Console.WriteLine(LocalizationService.Instance.Format("App.CheckingDir", dir));
                dbFile = LazerDatabase.GetDatabaseFile(dir);
                if (dbFile is null)
                {
                    Console.WriteLine(LocalizationService.Instance.Format("App.DirNotFound", dir));
                } else
                {
                    break; // database found, do not check more locations
                }
            }

            if (dbFile is null)
            {
                // fallback: prompt user for directory to check
                Console.Write($"{LocalizationService.Instance["App.DbNotFound"]}\n{LocalizationService.Instance["App.DbNotFoundHint"]}\n\n{LocalizationService.Instance["CLI.FolderPrompt"]}");
                string? input = Console.ReadLine();
                if (input is not null)
                {
                    dbFile = LazerDatabase.GetDatabaseFile(input);
                }
            }

            if (dbFile is null)
            {
                // failed to find lazer database
                Console.WriteLine(LocalizationService.Instance["CLI.DatabaseNotFoundDefault"]);
                ExporterApp.Exit();
            }
            LazerDatabase database = new LazerDatabase(dbFile);

            Realm? realm = null;
            try
            {
                realm = database!.Open();
                if (realm is null)
                    throw new IOException("Unable to open osu! database.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{LocalizationService.Instance.Format("App.DbOpenError", e.Message)}");
                Logger.Error(e, "Error opening database");
                if (e is LazerVersionException version)
                {
                    foreach (var message in version.Details)
                    {
                        Console.Write($"\n{LocalizationService.Instance["Export.ErrorPrefix"]} {message}\n");
                    }
                }
                else
                {
                    Console.WriteLine($"\n{LocalizationService.Instance["App.DbAbnormalError"]}");
                }
                ExporterApp.Exit();
            }

            Console.Write($"\n{LocalizationService.Instance["CLI.DatabaseOpened"]}\n{LocalizationService.Instance["CLI.LoadingContent"]}\n");
            settings.SaveDatabase(dbFile);

            // load beatmaps into memory for filtering/export later
            List<BeatmapSet> beatmaps = realm!.All<BeatmapSet>().ToList();
            List<BeatmapCollection> collections = realm.All<BeatmapCollection>().ToList();
            List<Skin> skins = realm.All<Skin>().ToList();
            Console.WriteLine(LocalizationService.Instance.Format("App.LoadComplete", beatmaps.Count, collections.Count, skins.Count));

            // start console i/o loop
            LazerExporter exporter = new(database, settings, beatmaps, collections, skins, transcoder);
            LazerExporterCLI cli = new(exporter);
            return new ExporterApp(cli);
        }
    }
}
