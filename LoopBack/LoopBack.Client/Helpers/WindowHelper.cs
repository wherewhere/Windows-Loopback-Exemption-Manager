using System.Collections.Generic;
using Windows.UI.Xaml;

namespace LoopBack.Client.Helpers
{
    /// <summary>
    /// Helpers class to allow the app to find the Window that contains an
    /// arbitrary <see cref="UIElement"/> (GetWindowForElement).
    /// To do this, we keep track of all active Windows. The app code must call
    /// WindowHelper.CreateWindow rather than "new <see cref="Window"/>()"
    /// so we can keep track of all the relevant windows.
    /// </summary>
    public static class WindowHelper
    {
        public static void TrackWindow(this Window window)
        {
            if (!ActiveWindows.Contains(window))
            {
                SettingsPaneRegister.Register(window);
                window.Closed += (sender, args) =>
                {
                    ActiveWindows.Remove(window);
                    SettingsPaneRegister.Unregister(window);
                    window = null;
                };
                ActiveWindows.Add(window);
            }
        }

        public static HashSet<Window> ActiveWindows { get; } = new HashSet<Window>(1);
    }
}
