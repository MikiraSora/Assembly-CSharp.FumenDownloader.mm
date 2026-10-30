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
        public static bool OnlyPublished { set; get; }
        public static bool EnableAddDownloadedMusicIntoRating { set; get; }

        public static string OptDirectory => Path.Combine(DataFolder, "opt");
        public static string OptDirectoryPrefix { get; private set; }

        public static void Init()
        {
            string iniFilePath = File.Exists(@"D:\mu3.ini") ? @"D:\mu3.ini" : "mu3.ini";
            string logFilePath;

            using (var iniFile = new IniFile(iniFilePath))
            {
                // 新键 FumenDownloaderFolder；兼容旧键 DataFolder。
                var folder = iniFile.getValue(Section, "FumenDownloaderFolder", "");
                if (string.IsNullOrEmpty(folder))
                    folder = iniFile.getValue(Section, "DataFolder", @"D:\dpFumenData");

                DataFolder = Path.GetFullPath(folder);
                Enable = iniFile.getValue(Section, "Enable", true);
                APIUriBase = iniFile.getValue(Section, "APIUriBase", "http://fumen.nageki-net.com/");
                OnlyPublished = iniFile.getValue(Section, "OnlyPublished", true);
                EnableAddDownloadedMusicIntoRating = iniFile.getValue(Section, "EnableAddDownloadedMusicIntoRating", false);
                logFilePath = iniFile.getValue(Section, "FumenDownloaderLogFilePath", @"D:\dpFumenDownload.log");
            }

            if (!APIUriBase.EndsWith("/"))
                APIUriBase += "/";

            OptDirectoryPrefix = Path.GetFullPath(OptDirectory).Replace('/', '\\');
            if (!OptDirectoryPrefix.EndsWith("\\"))
                OptDirectoryPrefix += "\\";

            PatchLog.SetFilePath(logFilePath);

            PatchLog.WriteLine($"---------DpPatches.FumenDownloader.Setting------------");
            PatchLog.WriteLine($"Enable = {Enable}");
            PatchLog.WriteLine($"DataFolder = {DataFolder}");
            PatchLog.WriteLine($"APIUriBase = {APIUriBase}");
            PatchLog.WriteLine($"OnlyPublished = {OnlyPublished}");
            PatchLog.WriteLine($"EnableAddDownloadedMusicIntoRating = {EnableAddDownloadedMusicIntoRating}");
            PatchLog.WriteLine($"FumenDownloaderLogFilePath = {logFilePath}");
            PatchLog.WriteLine($"--------------------------------------");
        }

        /// <summary>纯路径判定：不受 Enable 影响，调用方自行门控。</summary>
        public static bool IsUnderOptDirectory(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return false;
                return path.Replace('/', '\\').StartsWith(OptDirectoryPrefix, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
