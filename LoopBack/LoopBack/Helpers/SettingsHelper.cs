using MetroLog;
using MetroLog.Targets;
using System.IO;
using Windows.Storage;

namespace LoopBack.Helpers
{
    public static partial class SettingsHelper
    {
        public static ILogManager LogManager { get; } = CreateLogManager();

        public static ILogManager CreateLogManager()
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "MetroLogs");
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            LoggingConfiguration loggingConfiguration = new();
            loggingConfiguration.AddTarget(LogLevel.Info, LogLevel.Fatal, new StreamingFileTarget(path, 7));
            return LogManagerFactory.CreateLogManager(loggingConfiguration);
        }
    }
}
