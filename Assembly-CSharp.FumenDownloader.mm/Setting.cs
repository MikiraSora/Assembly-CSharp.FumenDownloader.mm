using MU3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DpPatches.FumenDownloader
{
    internal static class Setting
    {
        const string Section = "FumenDownloader";

        public static bool Enable { set; get; }
        public static string DataFolder { set; get; }
        public static string APIUriBase { set; get; }

        public static void Init()
        {
            using (var iniFile = new IniFile("mu3.ini"))
            {
                DataFolder = Path.GetFullPath(iniFile.getValue(Section, "DataFolder", "dpFumenData"));
                Enable = iniFile.getValue(Section, "Enable", true);
                APIUriBase = iniFile.getValue(Section, "APIUriBase", "http://nageki-net.com/fumen/");
            }

            if (!APIUriBase.EndsWith("/"))
                APIUriBase += "/";

            PatchLog.WriteLine($"---------DpPatches.FumenDownloader.Setting------------");
            PatchLog.WriteLine($"Enable = {Enable}");
            PatchLog.WriteLine($"DataFolder = {DataFolder}");
            PatchLog.WriteLine($"APIUriBase = {APIUriBase}");
            PatchLog.WriteLine($"--------------------------------------");
        }
    }
}
