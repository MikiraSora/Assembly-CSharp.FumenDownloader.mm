using System;

namespace DpPatches.FumenDownloader.Kernel.Base
{
    [Serializable]
    public class CacheFileInfo
    {
        public string relativeFilePath;
        public string lastWriteTime;

        public DateTime LastWriteTime => DateTime.Parse(lastWriteTime);
    }
}