using DpPatches.FumenDownloader;
using MonoMod;
using MU3.User;
using System;
using System.Collections.Generic;

namespace DpPatches.FumenDownloader.Patches
{
    /// <summary>
    /// 在 ranking 之前过滤输入，保证 2xxxx/下载曲不占用 Best/New/Platinum 名额，
    /// 被排除曲目空出的位置由后续合法曲目正常补上。
    /// </summary>
    [MonoModPatch("global::MU3.User.UserNewRating")]
    internal class UserNewRatingEx : UserNewRating
    {
        private extern void orig_calcBest(Dictionary<int, UserMusic> musicDict);

        private void calcBest(Dictionary<int, UserMusic> musicDict)
        {
            orig_calcBest(RatingFilter.FilterMusicDict(musicDict, exclude2xxxx: true));
        }

        private extern void orig_calcPlatinumScore(Dictionary<int, UserMusic> musicDict);

        private void calcPlatinumScore(Dictionary<int, UserMusic> musicDict)
        {
            orig_calcPlatinumScore(RatingFilter.FilterMusicDict(musicDict, exclude2xxxx: true));
        }
    }
}
