using DpPatches.FumenDownloader.Kernel;
using MU3.User;
using MU3.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DpPatches.FumenDownloader
{
    internal static class RatingFilter
    {
        /// <summary>过滤 rating 输入字典；不修改调用方的字典。</summary>
        public static Dictionary<int, UserMusic> FilterMusicDict(Dictionary<int, UserMusic> src, bool exclude2xxxx)
        {
            if (src == null)
                return null;

            var dst = new Dictionary<int, UserMusic>(src.Count);
            foreach (var kv in src)
            {
                if (!IsExcluded(kv.Key, exclude2xxxx))
                    dst[kv.Key] = kv.Value;
            }
            return dst;
        }

        /// <summary>过滤 recent 输入列表；不修改调用方的列表。</summary>
        public static List<UserRecentRating> FilterRecentList(List<UserRecentRating> src, bool exclude2xxxx)
        {
            if (src == null)
                return null;

            var dst = new List<UserRecentRating>(src.Count);
            foreach (var item in src)
            {
                if (item != null && !IsExcluded(item.musicId, exclude2xxxx))
                    dst.Add(item);
            }
            return dst;
        }

        /// <summary>
        /// 2xxxx 段（新 rating 的 New 框/Platinum 均排除）与下载谱面（默认排除，
        /// ini: EnableAddDownloadedMusicIntoRating 可放行）。
        /// </summary>
        public static bool IsExcluded(int musicId, bool exclude2xxxx)
        {
            if (exclude2xxxx && musicId >= 20000 && musicId < 30000)
                return true;

            var manager = Singleton<FumenDownloaderManager>.instance;
            return manager.IsDownloadedMusic(musicId) && !manager.IsEnableAddDownloadedMusicIntoRating();
        }
    }
}
