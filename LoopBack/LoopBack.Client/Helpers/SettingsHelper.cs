using MetroLog;

namespace LoopBack.Client.Helpers
{
    internal static partial class SettingsHelper
    {
        public static readonly ILogManager LogManager = LogManagerFactory.CreateLogManager();
    }
}
