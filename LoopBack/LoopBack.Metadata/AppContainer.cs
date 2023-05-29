using System;
using System.Collections.Generic;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;

namespace LoopBack.Metadata
{
    public sealed class AppContainer
    {
        public string AppContainerName { get; set; }
        public string DisplayName { get; set; }
        public string WorkingDirectory { get; set; }
        public string StringSid { get; set; }
        public IList<string> Capabilities { get; set; } = new List<string>();
        public bool LoopUtil { get; set; }

        internal unsafe AppContainer(string _appContainerName, string _displayName, string _workingDirectory, SID* _sid)
        {
            AppContainerName = _appContainerName;
            DisplayName = _displayName;
            WorkingDirectory = _workingDirectory;
            PInvoke.ConvertSidToStringSid(new PSID(_sid), out PWSTR tempSid);
            StringSid = tempSid.ToString();
        }
    }
}
