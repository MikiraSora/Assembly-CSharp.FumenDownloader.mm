using DpPatches.FumenDownloader;
using MonoMod;
using MU3.User;
using System;
using System.Collections.Generic;

namespace DpPatches.FumenDownloader.Patches
{
    /// <summary>
    /// 旧 rating：只排除下载谱面（与新版 in-tree 的 UserRating 行为一致）；
    /// 2xxxx 段仍按旧 rating 的 current/old 分桶参与。
    /// </summary>
    [MonoModPatch("global::MU3.User.UserRating")]
    internal class UserRatingEx : UserRating
    {
        private extern void orig_calcBest(Dictionary<int, UserMusic> musicDict);

        private void calcBest(Dictionary<int, UserMusic> musicDict)
        {
            orig_calcBest(RatingFilter.FilterMusicDict(musicDict, exclude2xxxx: false));
        }

        private extern void orig_calcRecent(List<UserRecentRating> srcList);

        private void calcRecent(List<UserRecentRating> srcList)
        {
            orig_calcRecent(RatingFilter.FilterRecentList(srcList, exclude2xxxx: false));
        }

        private extern void orig_calcNewRecent(Rating src, bool force);

        private void calcNewRecent(Rating src, bool force)
        {
            if (src != null && RatingFilter.IsExcluded(src.musicID, exclude2xxxx: false))
                return;

            orig_calcNewRecent(src, force);
        }
    }
}
