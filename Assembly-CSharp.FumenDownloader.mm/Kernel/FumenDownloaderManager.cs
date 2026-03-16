using DpPatches.FumenDownloader.Kernel.Base;
using DpPatches.FumenDownloader.Kernel.Base.FileChange;
using MU3.DataStudio;
using MU3.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace DpPatches.FumenDownloader.Kernel
{
    internal class FumenDownloaderManager
    {
        private readonly string cacheFilePath;
        private volatile bool isInitalizing = false;
        private Dictionary<int, CacheFumenInfo> localCacheMap = new();
        private Dictionary<int, FumenSet> currentFumenSet = new();

        public FumenDownloaderManager()
        {
            cacheFilePath = Path.Combine(Setting.DataFolder, "fileCacheList.json");
            PatchLog.WriteLine($"cacheFilePath: {cacheFilePath}");
        }

        public void Initalize()
        {
            if (isInitalizing)
                return;

            try
            {
                PrepareDownloadCacheData();
                UpdateOrDownloadFumen();
            }
            catch (Exception e)
            {
                PatchLog.WriteLine($"FumenDownloaderManager.Initalize() throw exception:{e.Message}");
            }
            finally
            {
                isInitalizing = true;
            }
        }

        public void WaitForInitalized()
        {
            while (!isInitalizing)
                Thread.Sleep(100);
        }

        public void PrepareDownloadCacheData()
        {
            //准备好谱面下载缓存数据
            try
            {
                if (File.Exists(cacheFilePath))
                {
                    var json = File.ReadAllText(cacheFilePath);
                    var r = JsonUtility.FromJson<CacheFileInfoListResponse>(json);
                    if (r is null)
                    {
                        PatchLog.WriteLine($"Can't deserialize json from local: {json}");
                        return;
                    }

                    localCacheMap = r.cacheFumenInfos.ToDictionary(x => x.musicId, x => x);
                    PatchLog.WriteLine($"loaded local fumen file cache list.");
                }
            }
            catch (Exception e)
            {
                PatchLog.WriteLine($"Can't load prev fumen file cache list from local: {e.Message}");
            }
        }

        public void UpdateOrDownloadFumen()
        {
            var query = $"?onlyPublish=false";

            //对比缓存数据, 更新或者下载谱面数据
            if (!SimpleHttp.GetString(Setting.APIUriBase + "fileCache/list" + query, out var json))
            {
                PatchLog.WriteLine($"Can't fetch fumen file cache list.");
                return;
            }

            var resp = JsonUtility.FromJson<CacheFileInfoListResponse>(json);
            if (resp is null)
            {
                PatchLog.WriteLine($"Can't deserialize json from url: {json}");
                return;
            }

            var remoteCacheList = resp.cacheFumenInfos.ToDictionary(x => x.musicId, x => x);
            var changesMap = new Dictionary<int, List<IFileChangeItem>>();

            foreach (var pair in remoteCacheList)
            {
                var musicId = pair.Key;
                var remote = pair.Value;

                var changes = new List<IFileChangeItem>();

                //check if musicId is exist in local cache
                if (!localCacheMap.TryGetValue(musicId, out var local))
                {
                    //download entire new fumen
                    AddDownload(remote, changes);
                }
                else
                {
                    //exist, check every files.
                    AddFileDiff(remote, local, changes);
                }

                changesMap[remote.musicId] = changes;
                currentFumenSet[musicId] = remote.fumenSet;
            }

            var totalChanges = changesMap.Values.SelectMany(x => x).ToList();

            //dump changes
            PatchLog.WriteLine("Change List:");
            PatchLog.WriteLine("------------");
            foreach (var change in totalChanges)
            {
                PatchLog.WriteLine("\t" + change.ToString());
                PatchLog.WriteLine("");
            }
            PatchLog.WriteLine("------------");

            //execute changes
            ExecuteChanges(totalChanges);

            //update assets.bytes if has changes
            if (totalChanges.Count > 0)
                UpdateAssetBytesFile();

            //save fumen cache file list
            SaveDownloadCacheData(resp);
        }

        private void UpdateAssetBytesFile()
        {
            var assetsByteFileDirPath = Path.Combine(Setting.DataFolder, "opt/assets");

            var filterRegexExpr = "ui_jacket_*";
            var filterRegex = new Regex(filterRegexExpr);

            var assetsByteFilePath = Path.Combine(assetsByteFileDirPath, "assets.bytes");
            var isUpdateAction = File.Exists(assetsByteFilePath);
            if (isUpdateAction)
            {
                PatchLog.WriteLine("Mod will update current exist `assets.bytes` file");
                var backupIdx = 0;
                PatchLog.WriteLine("Backup file....");
                while (true)
                {
                    var backupFilePath = assetsByteFilePath + ".backup" + (backupIdx == 0 ? "" : backupIdx);
                    if (!File.Exists(backupFilePath))
                    {
                        File.Copy(assetsByteFilePath, backupFilePath, true);
                        PatchLog.WriteLine("Backup file saved : " + backupFilePath);
                        break;
                    }
                    backupIdx++;
                }
            }
            else
            {
                File.WriteAllBytes(assetsByteFilePath, new byte[0]);
                PatchLog.WriteLine("Program generate new `assets.bytes` file");
            }

            PatchLog.WriteLine($"");
            PatchLog.WriteLine($"filterRegexStr = {filterRegexExpr}");
            PatchLog.WriteLine($"assetsByteFileDirPath = {assetsByteFileDirPath}");
            PatchLog.WriteLine($"assetsByteFilePath = {assetsByteFilePath}");

            var tempDstFilePath = Path.GetTempFileName();
            var tempSrcFilePath = Path.GetTempFileName();
            PatchLog.WriteLine($"tempSrcFilePath : {tempSrcFilePath} {Environment.NewLine}tempDstFilePath : {tempDstFilePath}");

            File.Copy(assetsByteFilePath, tempSrcFilePath, true);

            using var srcFileStream = File.OpenRead(tempSrcFilePath);
            using var reader = new BinaryReader(srcFileStream);

            using var dstFileStream = File.OpenWrite(tempDstFilePath);
            using var writer = new BinaryWriter(dstFileStream);

            var bundlesCount = isUpdateAction && (reader.BaseStream.Length - reader.BaseStream.Position >= sizeof(int)) ? reader.ReadInt32() : 0;
            var bundleInfoList = Enumerable.Range(0, bundlesCount).Select(_ =>
            {
                var id = reader.ReadInt32();
                var name = reader.ReadString();
                var numDependencies = reader.ReadInt32();
                var dependencies = new int[numDependencies];
                for (int i = 0; i < numDependencies; i++)
                {
                    dependencies[i] = reader.ReadInt32();
                }
                return new { id, name, numDependencies, dependencies };
            }).ToList();

            var needInsertList = Directory.GetFiles(assetsByteFileDirPath)
                .Select(x => Path.GetFileName(x))
                .Where(x => filterRegex.IsMatch(x))
                .Except(bundleInfoList.Select(x => x.name))
                .ToList();

            PatchLog.WriteLine($"");
            if (needInsertList.Count > 0)
            {
                PatchLog.WriteLine($"there are {needInsertList.Count} entries to append/update.");
                PatchLog.WriteLine($"----Append Name List----");
                PatchLog.WriteLine(string.Join(Environment.NewLine, needInsertList.ToArray()));
                PatchLog.WriteLine($"------------------------");
            }
            else
            {
                PatchLog.WriteLine($"no new entries to append, skipped.");
                return;
            }

            bundlesCount += needInsertList.Count;
            PatchLog.WriteLine("current bundle total count : " + bundlesCount);
            writer.Write(bundlesCount);

            PatchLog.WriteLine("Write same content...");
            var idx = 0;
            bundleInfoList.ForEach(x =>
            {
                writer.Write(x.id);
                writer.Write(x.name);
                writer.Write(x.numDependencies);
                for (int i = 0; i < x.numDependencies; i++)
                {
                    writer.Write(x.dependencies[i]);
                }
                idx++;
            });
            PatchLog.WriteLine("Write append content...");
            needInsertList.ForEach(name =>
            {
                writer.Write(idx++);
                writer.Write(name);
                writer.Write(0);
            });
            PatchLog.WriteLine("Writing all done!");

            writer.Flush();
            writer.Close();
            PatchLog.WriteLine($"copy {tempDstFilePath} -> {assetsByteFilePath}");
            File.Copy(tempDstFilePath, assetsByteFilePath, true);

            PatchLog.WriteLine($"All Done!");
        }

        private void AddFileDiff(CacheFumenInfo remote, CacheFumenInfo local, List<IFileChangeItem> changes)
        {
            var localFileMap = local.cacheFileInfo.ToDictionary(x => x.relativeFilePath, x => x);
            var remoteFileMap = remote.cacheFileInfo.ToDictionary(x => x.relativeFilePath, x => x);

            foreach (var pair in localFileMap)
            {
                var key = pair.Key;
                var localFile = pair.Value;

                if (remoteFileMap.TryGetValue(key, out var remoteFile))
                {
                    var localSaveFilePath = GetLocalSaveFilePath(remote.musicId, remoteFile.relativeFilePath);
                    if (!File.Exists(localSaveFilePath))
                    {
                        //local is deleted
                        changes.Add(new DownloadFileChange()
                        {
                            DownloadUrl = GetRemoteUrl(remoteFile.relativeFilePath),
                            FilePath = GetLocalSaveFilePath(remote.musicId, remoteFile.relativeFilePath)
                        });
                    }
                    else if (remoteFile.LastWriteTime > localFile.LastWriteTime)
                    {
                        //local is old
                        changes.Add(new UpdateFileChange()
                        {
                            DownloadUrl = GetRemoteUrl(remoteFile.relativeFilePath),
                            FilePath = GetLocalSaveFilePath(remote.musicId, remoteFile.relativeFilePath)
                        });
                    }
                }
                else
                {
                    //file was deleted from remote
                    changes.Add(new DeleteFileChange()
                    {
                        FilePath = GetLocalSaveFilePath(remote.musicId, localFile.relativeFilePath)
                    });
                }
            }

            foreach (var pair in remoteFileMap)
            {
                var key = pair.Key;
                var remoteFile = pair.Value;

                if (!localFileMap.TryGetValue(key, out _))
                {
                    //there is a new file from remote
                    changes.Add(new DownloadFileChange()
                    {
                        DownloadUrl = GetRemoteUrl(remoteFile.relativeFilePath),
                        FilePath = GetLocalSaveFilePath(remote.musicId, remoteFile.relativeFilePath)
                    });
                }
            }
        }

        private void AddDownload(CacheFumenInfo remote, List<IFileChangeItem> changes)
        {
            foreach (var item in remote.cacheFileInfo)
            {
                changes.Add(new DownloadFileChange()
                {
                    DownloadUrl = GetRemoteUrl(item.relativeFilePath),
                    FilePath = GetLocalSaveFilePath(remote.musicId, item.relativeFilePath)
                });
            }
        }

        private string GetLocalSaveFilePath(int musicId, string relativeUrl)
        {
            //fumen20997/opt/assets/ui_jacket_20997_s
            var relativePath = relativeUrl.Replace($"fumen{musicId}/", string.Empty);

            //<DataFolder>/opt/assets/ui_jacket_20997_s
            var actualPath = Path.Combine(Setting.DataFolder, relativePath);

            return actualPath;
        }

        private string GetRemoteUrl(string relativeUrl)
        {
            return Setting.APIUriBase + "files/" + relativeUrl;
        }

        public void SaveDownloadCacheData(CacheFileInfoListResponse remoteResp)
        {
            try
            {
                var r = JsonUtility.ToJson(remoteResp);
                File.WriteAllText(cacheFilePath, r);

                PatchLog.WriteLine($"saved local fumen file cache list.");
            }
            catch (Exception e)
            {
                PatchLog.WriteLine($"Can't save current fumen file cache list to local: {e.Message}");
            }
        }

        private void ExecuteChanges(List<IFileChangeItem> changes)
        {
            foreach (var change in changes)
            {
                try
                {
                    var result = change.Exeuctue();
                    PatchLog.WriteLine($"execute IFileChangeItem #{change.Id} {(result ? "successfully" : "failed")}.");
                }
                catch (Exception e)
                {
                    PatchLog.WriteLine($"execute IFileChangeItem #{change.Id} throw exception: {e.Message}");
                }
            }
        }

        public bool FilterMusicData(int key, MusicData musicData)
        {
            if (musicData == null)
                return false;
            var musicId = musicData.Name.id;

            //如果是自制谱，那就检查自制谱是否是否为published状态，如果不是published状态，并且用户设置了只显示published谱面，那就过滤掉这个谱面
            if (IsCustomFumen(musicId))
            {
                if (Setting.OnlyPublished && !IsCustomFumenPublishedCurrently(musicId))
                {
                    PatchLog.WriteLine($"FilterMusicData: filter musicId {key} because it's custom fumen and not published.");
                    return false;
                }
            }

            return true;
        }

        private bool IsCustomFumenPublishedCurrently(int musicId)
        {
            if (currentFumenSet.TryGetValue(musicId, out var fumenSet))
                return fumenSet.publishState == 2;

            if (localCacheMap.TryGetValue(musicId, out var cacheInfo))
                return cacheInfo.fumenSet?.publishState == 2;

            return false;
        }

        private bool IsCustomFumen(int musicId)
        {
            return localCacheMap.ContainsKey(musicId) || currentFumenSet.ContainsKey(musicId);
        }
    }
}
