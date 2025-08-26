using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.Logging;
using Windows.Storage;

namespace LoopBack.Helpers
{
    public static partial class SettingsHelper
    {
        public static ILoggerFactory LoggerFactory { get; } = CreateLoggerFactory();

        public static ILoggerFactory CreateLoggerFactory() =>
            Microsoft.Extensions.Logging.LoggerFactory.Create(x => _ = x.AddFile(x =>
            {
                x.RootPath = ApplicationData.Current.LocalFolder.Path;
                x.IncludeScopes = true;
                x.BasePath = "Logs";
                x.Files = [
                    new LogFileOptions()
                    {
                        Path = "Log - <date>.log"
                    }
                ];
            }).AddDebug());
    }
}
