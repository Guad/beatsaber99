using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using SongCore;
using SongCore.Data;
using SongCore.Utilities;
using UnityEngine;

namespace BeatSaber99Client
{
    public static class CustomSongInjector
    {
        private static CustomLevelLoader _customLevelLoader;
        private static BeatmapCharacteristicCollectionSO beatmapCharacteristicCollection;
        private static CachedMediaAsyncLoader cachedMediaAsyncLoaderSO;
        private static Sprite defaultCoverImage;

        public static void Init()
        {
            _customLevelLoader = Resources.FindObjectsOfTypeAll<CustomLevelLoader>().FirstOrDefault();
            beatmapCharacteristicCollection = _customLevelLoader.GetField<BeatmapCharacteristicCollectionSO>("_beatmapCharacteristicCollection");

            Texture2D defaultCoverTex = Texture2D.blackTexture;
            defaultCoverImage = Sprite.Create(defaultCoverTex, new Rect(0f, 0f,
                defaultCoverTex.width, defaultCoverTex.height), new Vector2(0.5f, 0.5f));
            cachedMediaAsyncLoaderSO = _customLevelLoader.GetField<CachedMediaAsyncLoader>("_cachedMediaAsyncLoaderSO");
        }

        public static void StartSongDownload(string url, Action<CustomPreviewBeatmapLevel> onComplete)
        {
            Plugin.log.Info("Queuing song download...");

            var t = new Thread(() =>
            {
                string tmpPath = Path.Combine(Path.GetTempPath(), "tmpdownload");
                Plugin.log.Info("Starting song download into " + tmpPath);

                if (File.Exists(tmpPath))
                    File.Delete(tmpPath);

                using (var wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "BeatSaber99/Client");
                    try
                    {
                        wc.DownloadFile(url, tmpPath);
                    }
                    catch (Exception e)
                    {
                        Plugin.log.Error(e);
                        return;
                    }
                }

                Plugin.log.Info("Song downloaded successfuly");

                string songDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                System.IO.Compression.ZipFile.ExtractToDirectory(tmpPath, songDirectory);

                Plugin.log.Info("Downloaded & extracted to " + songDirectory);

                Executor.Enqueue(() =>
                {
                    var level = LoadSong(songDirectory);

                    if (level == null)
                        Plugin.log.Error("Failed to load song!");

                    onComplete?.Invoke(level);
                });
            });

            t.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">path to the folder of the song</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static CustomPreviewBeatmapLevel LoadSong(string path)
        {
            StandardLevelInfoSaveData saveData = GetStandardLevelInfoSaveData(path);
            string hash;
            return LoadSong(saveData, path, out hash);
        }

        public static StandardLevelInfoSaveData GetStandardLevelInfoSaveData(string path)
        {
            var text = File.ReadAllText(path + "/info.dat");
            return StandardLevelInfoSaveData.DeserializeFromJSONString(text);

        }

        public static CustomPreviewBeatmapLevel LoadSong(StandardLevelInfoSaveData saveData, string songPath, out string hash, SongFolderEntry folderEntry = null)
        {
            CustomPreviewBeatmapLevel result;
            bool wip = false;
            if (songPath.Contains("CustomWIPLevels")) wip = true;
            if (folderEntry != null)
            {
                if (folderEntry.Pack == FolderLevelPack.CustomWIPLevels)
                    wip = true;
                else if (folderEntry.WIP)
                    wip = true;
            }
            hash = Hashing.GetCustomLevelHash(saveData, songPath);
            try
            {
                string folderName = new DirectoryInfo(songPath).Name;
                string levelID = CustomLevelLoader.kCustomLevelPrefixId + hash;
                if (wip) levelID += " WIP";
                if (SongCore.Collections.hashForLevelID(levelID) != "")
                    levelID += "_" + folderName;
                string songName = saveData.songName;
                string songSubName = saveData.songSubName;
                string songAuthorName = saveData.songAuthorName;
                string levelAuthorName = saveData.levelAuthorName;
                float beatsPerMinute = saveData.beatsPerMinute;
                float songTimeOffset = saveData.songTimeOffset;
                float shuffle = saveData.shuffle;
                float shufflePeriod = saveData.shufflePeriod;
                float previewStartTime = saveData.previewStartTime;
                float previewDuration = saveData.previewDuration;
                EnvironmentInfoSO environmentSceneInfo = _customLevelLoader.LoadEnvironmentInfo(saveData.environmentName, false);
                EnvironmentInfoSO allDirectionEnvironmentInfo = _customLevelLoader.LoadEnvironmentInfo(saveData.allDirectionsEnvironmentName, true);
                List<PreviewDifficultyBeatmapSet> list = new List<PreviewDifficultyBeatmapSet>();
                foreach (StandardLevelInfoSaveData.DifficultyBeatmapSet difficultyBeatmapSet in saveData.difficultyBeatmapSets)
                {
                    BeatmapCharacteristicSO beatmapCharacteristicBySerializedName = beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(difficultyBeatmapSet.beatmapCharacteristicName);
                    BeatmapDifficulty[] array = new BeatmapDifficulty[difficultyBeatmapSet.difficultyBeatmaps.Length];
                    for (int j = 0; j < difficultyBeatmapSet.difficultyBeatmaps.Length; j++)
                    {
                        BeatmapDifficulty beatmapDifficulty;
                        difficultyBeatmapSet.difficultyBeatmaps[j].difficulty.BeatmapDifficultyFromSerializedName(out beatmapDifficulty);
                        array[j] = beatmapDifficulty;
                    }
                    list.Add(new PreviewDifficultyBeatmapSet(beatmapCharacteristicBySerializedName, array));
                }

                result = new CustomPreviewBeatmapLevel(defaultCoverImage.texture, saveData, songPath,
                    cachedMediaAsyncLoaderSO, cachedMediaAsyncLoaderSO, levelID, songName, songSubName,
                    songAuthorName, levelAuthorName, beatsPerMinute, songTimeOffset, shuffle, shufflePeriod,
                    previewStartTime, previewDuration, environmentSceneInfo, allDirectionEnvironmentInfo, list.ToArray());
            }
            catch
            {
                Plugin.log.Error("Failed to Load Song: " + songPath);
                result = null;
            }
            return result;
        }


    }
}