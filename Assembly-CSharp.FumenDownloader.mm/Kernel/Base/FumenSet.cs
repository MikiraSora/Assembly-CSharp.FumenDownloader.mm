using MU3.DataStudio;
using System;
using System.Collections.Generic;

namespace DpPatches.FumenDownloader.Kernel.Base
{
    [Serializable]
    public class FumenSet
    {
        public int musicId;
        public string title;
        public string artist;
        public string updateTime;

        public int publishState;

        public DateTime UpdateTime => DateTime.Parse(updateTime);
    }
}