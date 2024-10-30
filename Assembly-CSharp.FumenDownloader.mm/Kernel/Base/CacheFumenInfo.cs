using System;
using System.Collections.Generic;

namespace DpPatches.FumenDownloader.Kernel.Base
{
    [Serializable]
    public class CacheFumenInfo
    {
        public int musicId;
        public string updateTime;
        public List<CacheFileInfo> cacheFileInfo;

        public DateTime UpdateTime => DateTime.Parse(updateTime);
    }
}