using System.Collections.Generic;
using System.Linq;

namespace BeatmapExporterGUI.ViewModels.Settings
{
    /// <summary>
    /// The DropdownSelector is a ValueSelector that offers a limited number of strictly defined options to the user as a dropdown.
    /// </summary>
    public class DropdownSelectorViewModel : ValueSelectorViewModel
    {
        private int selected;

        public DropdownSelectorViewModel(NewFilterViewModel newFilter, List<SelectOption> options) : base(newFilter)
        {
            Options = options;

            // Dropdown filters should have valid initial value and should create immediately
            Parent.ConstructFilter(Options[OptionSelected].InternalValue);
        }

        /// <summary>
        /// All available options for the user to choose from, as user-displayable objects.
        /// </summary>
        public List<SelectOption> Options { get; }

        /// <summary>
        /// The currently selected index from the dropdown options.
        /// </summary>
        public int OptionSelected
        {
            get => selected;
            set
            {
                if (value != -1)
                {
                    var option = Options[value];
                    Parent.ConstructFilter(option.InternalValue);
                }
                selected = value;
            }
        }
    }
}
