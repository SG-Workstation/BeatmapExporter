using BeatmapExporterCore.Localization;
using BeatmapExporterCore.Utilities;
using System.Reflection;

namespace BeatmapExporterCore.Filters
{
    /// <summary>
    /// Contains reference to all pre-defined filter types.
    /// </summary>
    public static class FilterTypes
    {
        static FilterTypes()
        {
            AllTypes = typeof(FilterTemplate)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.FieldType == typeof(FilterTemplate))
                .Select(p => (FilterTemplate)p.GetValue(null)!)
                .ToList();
        }

        /// <summary>
        /// Generated list of all filter types as pre-defined within this class.
        /// The list is built and stored such that the indicies can be relied on at least throughout the lifetime of the application.
        /// </summary>
        public static List<FilterTemplate> AllTypes { get; }
    }

    /// <summary>
    /// Describes a pre-defined type of filter that is available to be selected (and further configured) by the user.
    /// </summary>
    public class FilterTemplate
    {
        /// <summary>
        /// Delegate function generating a BeatmapFilter for a type of filter from a user input string and the filter negation state at the time of the input.
        /// </summary>
        public delegate BeatmapFilter FilterConstructor(string userInput, bool negate);

        /// <summary>
        /// The type of data that this filter template will collect from the user. 
        /// May be used to determine if there is a superior interface available than text input.
        /// </summary>
        public enum Input { RawText, Played, Gamemode, Status, Collection };

        private FilterTemplate(string shortName, string fullName, string normalInput, string negatedInput, string detail, Input inputType, FilterConstructor constructor)
        {
            ShortName = shortName;
            FullName = fullName;
            NormalInputDescriptor = normalInput;
            NegatedInputDescriptor = negatedInput;
            FilterDetail = detail;
            InputType = inputType;
            Constructor = constructor;
        }

        /// <summary>
        /// Filter name which may be used for finding filter types from user input.
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Full filter name, suitable for use in user-viewed outputs.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Defines the type of input the user should be providing for this filter, ex. 'maximum'. 
        /// </summary>
        public string NormalInputDescriptor { get; }

        /// <summary>
        /// Defines the type of input the user should provide if this filter is negated, ex. 'minimum'.
        /// </summary>
        public string NegatedInputDescriptor { get; }

        /// <summary>
        /// A detailed description of the filter's functionality, suitable for display to the user directly.
        /// </summary>
        public string FilterDetail { get; }

        /// <summary>
        /// The Input type needed from the user to create a filter of this type. Use RawText if not creating a more precise input type.
        /// This is additional info useful for creating a better interface, may be ignored (ex. CLI).
        /// </summary>
        public Input InputType { get; }

        /// <summary>
        /// Function which constructs a filter of this type from a user input.
        /// </summary>
        public FilterConstructor Constructor { get; }

        /// <summary>
        /// A string briefly defining the type of input the user should provide, using the full filter name and appropriate input descriptor.
        /// </summary>
        public string InputDescription(bool negated) => $"{FullName} {(negated ? NegatedInputDescriptor : NormalInputDescriptor)}";

        // All BeatmapExporter-supported filters are defined below.

        #region Filters
        public static FilterTemplate StarRating = new(
            "stars",
            LocalizationService.Instance["Filter.Stars"],
            LocalizationService.Instance["Filter.InputMinimum"],
            LocalizationService.Instance["Filter.InputMaximum"],
            LocalizationService.Instance["Filter.StarRatingDetail"],
            Input.RawText,
            (input, negate) =>
            {
                // 6.3
                if (!float.TryParse(input, out float starRating))
                    throw new ArgumentException(LocalizationService.Instance["Filter.ErrorStarRating"]);

                return new(input, negate,
                    b => b.StarRating >= starRating,
                    StarRating!);
            });

        public static FilterTemplate Length = new(
            "length",
            LocalizationService.Instance["Filter.Length"],
            LocalizationService.Instance["Filter.InputLonger"],
            LocalizationService.Instance["Filter.InputShorter"],
            LocalizationService.Instance["Filter.LengthDetail"],
            Input.RawText,
            (input, negate) =>
            {
                // 90
                if (!int.TryParse(input, out int duration))
                    throw new ArgumentException(LocalizationService.Instance["Filter.ErrorLength"]);
                int millis = duration * 1000;

                return new(input, negate,
                    b => b.Length >= millis,
                    Length!);
            });

        public static FilterTemplate Author = new(
            "author",
            LocalizationService.Instance["Filter.Author"],
            LocalizationService.Instance["Filter.InputIs"],
            LocalizationService.Instance["Filter.InputIsNot"],
            LocalizationService.Instance["Filter.AuthorDetail"],
            Input.RawText,
            (input, negate) =>
            {
                // RLC, Nathan
                string[] authors = input.ToLower().CommaSeparatedArg();

                return new(input, negate,
                    b => authors.Contains(b.Metadata.Author.Username.ToLower()),
                    Author!);
            });

        public static FilterTemplate Id = new(
            "id",
            LocalizationService.Instance["Filter.Id"],
            LocalizationService.Instance["Filter.InputIs"],
            LocalizationService.Instance["Filter.InputIsNot"],
            LocalizationService.Instance["Filter.IdDetail"],
            Input.RawText,
            (input, negate) =>
            {
                // 1, 2, 3
                var beatmapIds = input.CommaSeparatedArg().Select(int.Parse);

                return new(input, negate,
                    b =>
                    {
                        var beatmapSet = b.BeatmapSet;
                        return beatmapSet != null && beatmapIds.Contains(beatmapSet.OnlineID);
                    },
                    Id!);
            });

        public static FilterTemplate BPM = new(
            "bpm",
            LocalizationService.Instance["Filter.BPM"],
            LocalizationService.Instance["Filter.InputMinimum"],
            LocalizationService.Instance["Filter.InputMaximum"],
            LocalizationService.Instance["Filter.BPMDetail"],
            Input.RawText,
            (input, negate) =>
            {
                // 180
                if (!int.TryParse(input, out int bpm))
                    throw new ArgumentException(LocalizationService.Instance["Filter.ErrorBPM"]);

                return new(input, negate,
                    b => b.BPM >= bpm,
                    BPM!);
            });

        public static FilterTemplate AddedSince = new(
            "since",
            LocalizationService.Instance["Filter.Since"],
            LocalizationService.Instance["Filter.InputLast"],
            LocalizationService.Instance["Filter.InputBefore"],
            LocalizationService.Instance["Filter.SinceDetail"],
            Input.RawText,
            (input, negate) =>
            {
                // 2:00
                if (!TimeSpan.TryParse(input, out TimeSpan since))
                    throw new ArgumentException(LocalizationService.Instance["Filter.ErrorTimeInterval"]);

                return new(input, negate,
                    b =>
                    {
                        if (b.BeatmapSet is null) return false; // if map doesn't have a set at all, we can't get the date added
                        var lifetime = DateTime.Now - b.BeatmapSet.DateAdded;
                        return lifetime < since;
                    },
                    AddedSince!);
            });

        public static FilterTemplate RankedSince = new(
            "ranked",
            LocalizationService.Instance["Filter.Ranked"],
            LocalizationService.Instance["Filter.InputLast"],
            LocalizationService.Instance["Filter.InputBefore"],
            LocalizationService.Instance["Filter.RankedDetail"],
            Input.RawText,
            (input, negate) =>
            {
                // 30
                if (!TimeSpan.TryParse(input, out TimeSpan since))
                    throw new ArgumentException(LocalizationService.Instance["Filter.ErrorTimeInterval"]);

                return new(input, negate,
                    b =>
                    {
                        var ranked = b.BeatmapSet?.DateRanked;
                        if (ranked == null) return false; // beatmap is not ranked
                        return DateTime.Now - ranked < since;
                    },
                    RankedSince!);
            });

        public static FilterTemplate Artist = new(
            "artist",
            LocalizationService.Instance["Filter.Artist"],
            LocalizationService.Instance["Filter.InputIs"],
            LocalizationService.Instance["Filter.InputIsNot"],
            LocalizationService.Instance["Filter.ArtistDetail"],
            Input.RawText,
            (input, negate) =>
            {
                // Camellia, Nanahira
                string[] artists = input.ToLower().CommaSeparatedArg();

                return new(input, negate,
                    b => artists.Contains(b.Metadata.Artist.ToLower()),
                    Artist!);
            });

        public static FilterTemplate Tag = new(
            "tag",
            LocalizationService.Instance["Filter.Tag"],
            LocalizationService.Instance["Filter.InputContain"],
            LocalizationService.Instance["Filter.InputNotContain"],
            LocalizationService.Instance["Filter.TagDetail"],
            Input.RawText,
            (input, negate) =>
            {
                // touhou
                string[] tags = input.ToLower().CommaSeparatedArg();

                return new(input, negate,
                    b =>
                    {
                        string? beatmapTags = b.Metadata.Tags?.ToLower();
                        return beatmapTags != null && tags.Any(t => beatmapTags.Contains(t));
                    },
                    Tag!);
            });

        public static FilterTemplate Gamemode = new(
            "gamemode",
            LocalizationService.Instance["Filter.Gamemode"],
            LocalizationService.Instance["Filter.InputIs"],
            LocalizationService.Instance["Filter.InputIsNot"],
            LocalizationService.Instance["Filter.GamemodeDetail"],
            Input.Gamemode,
            (input, negate) =>
            {
                // osu/mania/ctb/taiko
                int? gamemodeId = input.ToLower() switch
                {
                    "osu" => 0,
                    "taiko" => 1,
                    "ctb" => 2,
                    "mania" => 3,
                    _ => null
                };
                if (gamemodeId == null)
                    throw new ArgumentException(LocalizationService.Instance["Filter.ErrorGamemode"]);

                return new(input, negate,
                    b => b.Ruleset.OnlineID == gamemodeId,
                    Gamemode!);
            });

        public static FilterTemplate OnlineStatus = new(
            "status",
            LocalizationService.Instance["Filter.Status"],
            LocalizationService.Instance["Filter.InputIs"],
            LocalizationService.Instance["Filter.InputIsNot"],
            LocalizationService.Instance["Filter.StatusDetail"],
            Input.Status,
            (input, negate) =>
            {
                // graveyard/leaderboard/ranked/approved/qualified/loved
                int[]? statusId = input.ToLower() switch
                {
                    var s when s.StartsWith("graveyard") || s == "unknown" => new[] { -3 },
                    var s when s.StartsWith("leaderboard") => new[] { 1, 2, 3, 4 },
                    var s when s.StartsWith("rank") => new[] { 1 },
                    var s when s.StartsWith("approve") => new[] { 2 },
                    var s when s.StartsWith("qualif") => new[] { 3 },
                    var s when s.StartsWith("love") => new[] { 4 },
                    _ => null
                };
                if (statusId == null)
                    throw new ArgumentException(LocalizationService.Instance["Filter.ErrorStatus"]);

                return new(input, negate,
                    b => statusId.Contains(b.Status),
                    OnlineStatus!);
            });

        public static FilterTemplate PlayedSince = new(
            "played",
            LocalizationService.Instance["Filter.Played"],
            LocalizationService.Instance["Filter.InputLast"],
            LocalizationService.Instance["Filter.InputNotSince"],
            LocalizationService.Instance["Filter.PlayedDetail"],
            Input.RawText,
            (input, negate) =>
            {
                // 12:00 
                if (!TimeSpan.TryParse(input, out TimeSpan since))
                    throw new ArgumentException(LocalizationService.Instance["Filter.ErrorTimeInterval"]);

                return new(input, negate,
                    b =>
                    {
                        if (b.LastPlayed == null) return false; // beatmap has never been played
                        var playedAgo = DateTime.Now - b.LastPlayed;
                        return playedAgo < since;
                    },
                    PlayedSince!);
            });

        public static FilterTemplate Played = new(
            "everplayed",
            LocalizationService.Instance["Filter.EverPlayed"],
            "",
            LocalizationService.Instance["Filter.InputNegated"],
            LocalizationService.Instance["Filter.EverPlayedDetail"],
            Input.Played,
            (input, negate) =>
            {
                var lower = input.ToLower();
                // Override flag - exports all difficulties if any were played
                if (lower.Contains("set") || lower.Contains("diff"))
                {
                    return new(input, negate,
                        b =>
                        {
                            var beatmapSet = b.BeatmapSet;
                            return beatmapSet != null && beatmapSet.Beatmaps.Any(b => b.LastPlayed != null);
                        },
                        Played!);
                }
                else
                {
                    bool? played = lower switch
                    {
                        "yes" => true,
                        "no" => false,
                        _ => null
                    };
                    if (played == null)
                        throw new ArgumentException(LocalizationService.Instance["Filter.ErrorPlayed"]);

                    return new(input, negate,
                        b => (b.LastPlayed != null) == played,
                        Played!);
                }
            });

        public static FilterTemplate Collections = new(
            "collection",
            LocalizationService.Instance["Filter.Collection"],
            "",
            LocalizationService.Instance["Filter.InputNotIn"],
            LocalizationService.Instance["Filter.CollectionDetail"],
            Input.Collection,
            (input, negate) =>
            {
                // name1, name2
                string[] collections = input.CommaSeparatedArg();
                // builds a placeholder filter that will be re-built if/where the user's collections are available
                return new(input, negate, collections);
            });
        #endregion
    }
}
