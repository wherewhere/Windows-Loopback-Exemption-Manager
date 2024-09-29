using LoopBack.Client.Common;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace LoopBack.Client.Helpers
{
    /// <summary>
    /// Class providing functionality around switching and restoring theme settings
    /// </summary>
    public static class ThemeHelper
    {
        public static bool IsStatusBarSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        // Keep reference so it does not get optimized/garbage collected
        public static UISettings UISettings { get; } = new UISettings();
        public static AccessibilitySettings AccessibilitySettings { get; } = new AccessibilitySettings();

        static ThemeHelper()
        {
            // Registering to color changes, thus we notice when user changes theme system wide
            UISettings.ColorValuesChanged += UISettings_ColorValuesChanged;
        }

        public static void Initialize(Window window)
        {
            UpdateSystemCaptionButtonColors(window);
        }

        private static void UISettings_ColorValuesChanged(UISettings sender, object args)
        {
            UpdateSystemCaptionButtonColors();
        }

        public static bool IsDarkTheme() => UISettings.GetColorValue(UIColorType.Foreground).IsColorLight();

        public static bool IsColorLight(this Color color) => ((5 * color.G) + (2 * color.R) + color.B) > (8 * 128);

        public static async void UpdateSystemCaptionButtonColors()
        {
            bool isDark = IsDarkTheme();
            bool isHighContrast = AccessibilitySettings.HighContrast;

            Color foregroundColor = isDark || isHighContrast ? Colors.White : Colors.Black;
            Color backgroundColor = isHighContrast ? Color.FromArgb(255, 0, 0, 0) : isDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

            foreach (Window window in WindowHelper.ActiveWindows)
            {
                await window.Dispatcher.ResumeForegroundAsync();

                if (IsStatusBarSupported)
                {
                    StatusBar statusBar = StatusBar.GetForCurrentView();
                    statusBar.ForegroundColor = foregroundColor;
                    statusBar.BackgroundColor = backgroundColor;
                    statusBar.BackgroundOpacity = 0; // 透明度
                }

                bool extendViewIntoTitleBar = CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.ForegroundColor = titleBar.ButtonForegroundColor = foregroundColor;
                titleBar.BackgroundColor = titleBar.InactiveBackgroundColor = backgroundColor;
                titleBar.ButtonBackgroundColor = titleBar.ButtonInactiveBackgroundColor = extendViewIntoTitleBar ? Colors.Transparent : backgroundColor;
            }
        }

        public static async void UpdateSystemCaptionButtonColors(Window window)
        {
            await window.Dispatcher.ResumeForegroundAsync();

            bool isDark = IsDarkTheme();
            bool isHighContrast = AccessibilitySettings.HighContrast;

            Color foregroundColor = isDark || isHighContrast ? Colors.White : Colors.Black;
            Color backgroundColor = isHighContrast ? Color.FromArgb(255, 0, 0, 0) : isDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

            if (IsStatusBarSupported)
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = foregroundColor;
                statusBar.BackgroundColor = backgroundColor;
                statusBar.BackgroundOpacity = 0; // 透明度
            }

            bool extendViewIntoTitleBar = CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ForegroundColor = titleBar.ButtonForegroundColor = foregroundColor;
            titleBar.BackgroundColor = titleBar.InactiveBackgroundColor = backgroundColor;
            titleBar.ButtonBackgroundColor = titleBar.ButtonInactiveBackgroundColor = extendViewIntoTitleBar ? Colors.Transparent : backgroundColor;
        }
    }
}
