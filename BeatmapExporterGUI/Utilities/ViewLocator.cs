using Avalonia.Controls;
using Avalonia.Controls.Templates;
using BeatmapExporterCore.Localization;
using BeatmapExporterGUI.ViewModels;
using System;

namespace BeatmapExporterGUI.Utilities
{
    // Ref: https://github.com/AvaloniaUI/Avalonia.Samples/tree/main/src%2FAvalonia.Samples%2FRouting%2FBasicViewLocatorSample
    public class ViewLocator : IDataTemplate
    {
        public Control Build(object? data)
        {
            if (data is null)
            {
                return new TextBlock { Text = LocalizationService.Instance["ViewLocator.NullData"] };
            }

            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            else
            {
                return new TextBlock { Text = LocalizationService.Instance.Format("ViewLocator.NotFound", name) };
            }
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}