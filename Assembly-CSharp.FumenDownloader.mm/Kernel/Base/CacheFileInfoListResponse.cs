using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DpPatches.FumenDownloader.Kernel.Base
{
    [Serializable]
    internal class CacheFileInfoListResponse
    {
        public List<CacheFumenInfo> cacheFumenInfos;
    }
}
