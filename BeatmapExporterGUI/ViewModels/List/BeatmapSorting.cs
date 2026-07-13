using BeatmapExporterCore.Localization;
using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using static BeatmapExporterGUI.ViewModels.List.BeatmapSorting;

namespace BeatmapExporterGUI.ViewModels.List
{
    public class BeatmapSorting
    {
        /// <summary>
        /// All supported beatmap sorting options.
        /// </summary>
        public enum SortBy
        {
            ID, // beatmap ID
            Artist, // song artist name
            DateAdded, // beatmap set added to lazer date
            Count, // number of beatmaps in set
            Title, // song title
            Author, // mapper name
            Length, // song length 
        }

        /// <summary>
        /// Array of all SortBy values.
        /// </summary>
        public static IEnumerable<SortBy> AllSortOptions => ((SortBy[])Enum.GetValues(typeof(SortBy)));

        /// <summary>
        /// All supported beatmap display/viewing options.
        /// </summary>
        public enum View { Selected, All }

        /// <summary>
        /// Array of all View values.
        /// </summary>
        public static IEnumerable<View> AllDisplayOptions => (View[])Enum.GetValues(typeof(View));

        /// <summary>
        /// Delegate function returning which property of a BeatmapSet that the sorting method should compare.
        /// </summary>
        internal delegate IComparable? ComparedProperty(BeatmapSet set);

        /// <summary>
        /// Helper method to create a Comparison from a specific comparable BeatmapSet property.
        /// </summary>
        internal static Comparison<BeatmapSet> SetComparison(ComparedProperty prop) => (x, y) => prop(x)?.CompareTo(prop(y)) ?? 0;
    }

    public static class SortExtension
    {
        /// <summary>
        /// A string representing a full, user-displayable name for this SortBy option.
        /// </summary>
        public static string FullName(this SortBy sort) => sort switch
        {
            SortBy.ID => LocalizationService.Instance["Sort.ID"],
            SortBy.Artist => LocalizationService.Instance["Sort.Artist"],
            SortBy.DateAdded => LocalizationService.Instance["Sort.DateAdded"],
            SortBy.Count => LocalizationService.Instance["Sort.Count"],
            SortBy.Title => LocalizationService.Instance["Sort.Title"],
            SortBy.Author => LocalizationService.Instance["Sort.Author"],
            SortBy.Length => LocalizationService.Instance["Sort.Length"]
        };

        /// <summary>
        /// A Comparison for this SortBy option which should be used to perform the sorting itself.
        /// </summary>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static Comparison<BeatmapSet> Comparer(this SortBy sort) => sort switch
        {
            SortBy.ID => SetComparison(b => b.OnlineID),
            SortBy.Artist => SetComparison(b => b.DiffMetadata?.Artist),
            SortBy.DateAdded => SetComparison(b => b.DateAdded),
            SortBy.Count => SetComparison(b => b.Beatmaps.Count),
            SortBy.Title => SetComparison(b => b.DiffMetadata?.Title),
            SortBy.Author => SetComparison(b => b.DiffMetadata?.Author?.Username),
            SortBy.Length => SetComparison(b => b.Beatmaps.Select(diff => diff.Length).Max()),
        };
    }

    public static class DisplayExtension
    {
        /// <summary>
        /// A string representing this View option when it is applied to displaying entire beatmap sets.
        /// </summary>
        public static string SetName(this View display) => display switch
        {
            View.Selected => LocalizationService.Instance["View.Filtered"],
            View.All => LocalizationService.Instance["View.All"]
        };

        /// <summary>
        /// A string representing this View option when it is applied to individual beatmap difficulties.
        /// </summary>
        public static string DiffName(this View display) => display switch
        {
            View.Selected => LocalizationService.Instance["View.FilteredDiffs"],
            View.All => LocalizationService.Instance["View.AllDiffs"]
        };
    }
}
