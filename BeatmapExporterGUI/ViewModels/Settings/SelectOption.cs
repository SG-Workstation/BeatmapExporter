namespace BeatmapExporterGUI.ViewModels.Settings;

public class SelectOption
{
    public string DisplayName { get; }
    public string InternalValue { get; }

    public SelectOption(string displayName, string internalValue)
    {
        DisplayName = displayName;
        InternalValue = internalValue;
    }

    public override string ToString() => DisplayName;
}
