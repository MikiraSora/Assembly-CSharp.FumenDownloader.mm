using DpPatches.FumenDownloader.Kernel;
using MonoMod;
using MU3.Data;
using MU3.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DpPatches.FumenDownloader.Patches
{
    [MonoModPatch("global::MU3.Data.DataStudioManager/Loader")]
    internal class DataStudioManagerLoaderEx : DataStudioManager.Loader
    {
        public static string FumenOptPath => Path.Combine(Setting.DataFolder, "opt");

        public DataStudioManagerLoaderEx(DataStudioManager dataStudioManager) : base(dataStudioManager)
        {

        }

        public extern static List<string> orig_GetLoadDirectories(string optionImagePath, MU3.DataStudio.Version version);

        public static List<string> GetLoadDirectories(string optionImagePath, MU3.DataStudio.Version version)
        {
            var list = orig_GetLoadDirectories(optionImagePath, version);

            if (Setting.Enable)
            {
                //insert user custom fumen option path.
                MakeSureFumenOptionDataReady(FumenOptPath);
                list.Add(FumenOptPath);
                PatchLog.WriteLine($"append fumen opt folder: {FumenOptPath}");
            }

            return list;
        }

        private static void MakeSureFumenOptionDataReady(string optFolder)
        {
            if (Directory.Exists(optFolder))
                return;

            Directory.CreateDirectory(optFolder);

            //copy dataconfig.xml
            var gameDataFolder = Path.Combine(Application.streamingAssetsPath, "GameData");
            var dataConfigFile = Directory.GetFiles(gameDataFolder, "DataConfig.xml", SearchOption.AllDirectories).FirstOrDefault();
            if (!File.Exists(dataConfigFile))
                throw new Exception($"DataConfig.xml is not found in GameData folder:{gameDataFolder}");

            File.Copy(dataConfigFile, Path.Combine(optFolder, "DataConfig.xml"), true);
            PatchLog.WriteLine($"create fumen opt folder: {optFolder}");

            Singleton<FumenDownloaderManager>.instance.WaitForInitalized();
        }
    }
}
