using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BS_Utils.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BeatSaber99Client.Game
{
    /// <summary>
    /// Loads and changes levels in the game.
    /// </summary>
    public static class LevelLoader
    {
        private static bool _loaded = false;
        private static BeatmapLevelsModel _beatmapLevelsModel;
        private static AdditionalContentModel _contentModelSO;
        private static BeatmapCharacteristicSO[] _beatmapCharacteristics;

        public static void Init()
        {
            if (_loaded) return;
            _loaded = true;

            Plugin.log.Debug("Loading level loader");

            _beatmapLevelsModel = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().FirstOrDefault();
            _contentModelSO = Resources.FindObjectsOfTypeAll<AdditionalContentModel>().FirstOrDefault();
            _beatmapCharacteristics = Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSO>();
        }

        // This is stuff like 360 level, one saber, standard, 90 degrees, etc.
        public static BeatmapCharacteristicSO[] Characteristics
        {
            get
            {
                if (_beatmapCharacteristics == null)
                    _beatmapCharacteristics = Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSO>();
                return _beatmapCharacteristics;
            }
        }

        public static BeatmapCharacteristicSO StandardCharacteristic =>
            _beatmapCharacteristics.First(c => c.serializedName == "Standard");

        // Only assume the official levels are contained here.
        public static IEnumerable<IPreviewBeatmapLevel> AllLevels
        {
            get
            {
                _beatmapLevelsModel = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().First();

                if (_beatmapLevelsModel.allLoadedBeatmapLevelPackCollection == null)
                    Plugin.log.Critical("All Loaded beatmap levels is null!");
                if (_beatmapLevelsModel.allLoadedBeatmapLevelPackCollection.beatmapLevelPacks == null)
                    Plugin.log.Critical("All Loaded beatmap levels is null!");

                return _beatmapLevelsModel.allLoadedBeatmapLevelPackCollection.beatmapLevelPacks.SelectMany(x =>
                    x.beatmapLevelCollection?.beatmapLevels ?? new IPreviewBeatmapLevel[0]);
            }
        }

        public static async void LoadBeatmapLevelAsync(
            BeatmapCharacteristicSO characteristic, IPreviewBeatmapLevel selectedLevel, BeatmapDifficulty difficulty,
            Action<AdditionalContentModel.EntitlementStatus, bool, IBeatmapLevel> callback)
        {
            _beatmapLevelsModel = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().FirstOrDefault();
            _contentModelSO = Resources.FindObjectsOfTypeAll<AdditionalContentModel>().FirstOrDefault();
            _beatmapCharacteristics = Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSO>();

            var token = new CancellationTokenSource();

            Plugin.log.Info("Checking entitlement");

            var entitlementStatus =
                await _contentModelSO.GetLevelEntitlementStatusAsync(selectedLevel.levelID, token.Token);

            if (entitlementStatus == AdditionalContentModel.EntitlementStatus.Owned)
            {
                Plugin.log.Info("Level owned. Loading...");

                var getBeatmapLevelResult =
                    await _beatmapLevelsModel.GetBeatmapLevelAsync(selectedLevel.levelID, token.Token);

                callback?.Invoke(entitlementStatus, !getBeatmapLevelResult.isError, getBeatmapLevelResult.beatmapLevel);

                Plugin.log.Info("Starting...");
                StartLevel(getBeatmapLevelResult.beatmapLevel, characteristic, difficulty,
                    GameplayModifiers.defaultModifiers);
            }
            else
            {
                callback?.Invoke(entitlementStatus, false, null);
            }
        }

        private static void StartLevel(IBeatmapLevel level, BeatmapCharacteristicSO characteristic,
            BeatmapDifficulty difficulty, GameplayModifiers modifiers, float startTime = 0f)
        {
            var menuSceneSetupData = Resources.FindObjectsOfTypeAll<MenuTransitionsHelper>().FirstOrDefault();


            if (menuSceneSetupData != null)
            {
                var playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;

                var playerSettings = playerData.playerSpecificSettings;
                var environmentOverrideSettings = playerData.overrideEnvironmentSettings;

                var colorSchemesSettings = playerData.colorSchemesSettings.overrideDefaultColors
                    ? playerData.colorSchemesSettings.GetColorSchemeForId(playerData.colorSchemesSettings
                        .selectedColorSchemeId)
                    : null;


                var difficultyBeatmap = level.GetDifficultyBeatmap(characteristic, difficulty, false);
                
                try
                {
                    BS_Utils.Gameplay.Gamemode.NextLevelIsIsolated("TestMod");
                }
                catch
                {
                }

                PracticeSettings practiceSettings = null;
                if (startTime > 1f)
                {
                    practiceSettings = new PracticeSettings(PracticeSettings.defaultPracticeSettings);
                    if (startTime > 1f)
                    {
                        practiceSettings.startSongTime = startTime + 1.5f;
                        practiceSettings.startInAdvanceAndClearNotes = true;
                    }

                    practiceSettings.songSpeedMul = modifiers.songSpeedMul;
                }
                
                menuSceneSetupData.StartStandardLevel(
                    difficultyBeatmap,
                    environmentOverrideSettings,
                    colorSchemesSettings,
                    modifiers,
                    playerSettings,
                    practiceSettings,
                    "Menu",
                    false,
                    () => { },
                    (manager, result) => { });
            }
            else
            {
                Plugin.log.Error("SceneSetupData is null!");
            }
        }

        public static async void PreloadBeatmapLevelAsync(
            BeatmapCharacteristicSO characteristic,
            IPreviewBeatmapLevel selectedLevel,
            BeatmapDifficulty difficulty,
            GameplayModifiers modifiers,
            Action<PreloadedLevel> callback)
        {
            _beatmapLevelsModel = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().FirstOrDefault();
            _contentModelSO = Resources.FindObjectsOfTypeAll<AdditionalContentModel>().FirstOrDefault();
            _beatmapCharacteristics = Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSO>();

            var token = new CancellationTokenSource();

            var previewCache = _beatmapLevelsModel
                .GetPrivateField<Dictionary<string, IPreviewBeatmapLevel>>("_loadedPreviewBeatmapLevels");

            if (!previewCache.ContainsKey(selectedLevel.levelID))
            {
                previewCache.Add(selectedLevel.levelID, selectedLevel);
                _beatmapLevelsModel.SetPrivateField("_loadedPreviewBeatmapLevels", previewCache);
            }

            var entitlementStatus =
                await _contentModelSO.GetLevelEntitlementStatusAsync(selectedLevel.levelID, token.Token);

            if (entitlementStatus == AdditionalContentModel.EntitlementStatus.Owned)
            {
                var getBeatmapLevelResult =
                    await _beatmapLevelsModel.GetBeatmapLevelAsync(selectedLevel.levelID, token.Token);

                if (getBeatmapLevelResult.isError)
                {
                    callback?.Invoke(null);
                    return;
                }
                
                var playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;

                var playerSettings = playerData.playerSpecificSettings;
                var environmentOverrideSettings = playerData.overrideEnvironmentSettings;

                var colorSchemesSettings = playerData.colorSchemesSettings.overrideDefaultColors
                    ? playerData.colorSchemesSettings.GetColorSchemeForId(playerData.colorSchemesSettings
                        .selectedColorSchemeId)
                    : null;

                
                var difficultyBeatmap =
                    getBeatmapLevelResult.beatmapLevel.GetDifficultyBeatmap(characteristic, difficulty, false);

                callback?.Invoke(new PreloadedLevel()
                {
                    characteristic = characteristic,
                    levelResult = getBeatmapLevelResult,
                    modifiers = modifiers,
                    difficulty = difficultyBeatmap,
                    playerData = playerData,
                    playerSpecificSettings = playerSettings,
                    environmentSettings = environmentOverrideSettings,
                    colorScheme = colorSchemesSettings
                });
            }
            else
            {
                callback?.Invoke(null);
            }
        }

        public static void SwitchLevel(PreloadedLevel level, float startTime = 0f)
        {
            var _scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();

            if (_scoreController == null)
                Plugin.log.Info("ScoreController was null!");

            var oldCombo = _scoreController.GetPrivateField<int>("_combo");
            var oldMaxCombo = _scoreController.maxCombo;
            var oldScore = _scoreController.GetPrivateField<int>("_baseRawScore");
            var oldTotalNotes = _scoreController.GetPrivateField<int>("_cutOrMissedNotes");

            var menuSceneSetupData =
                Resources.FindObjectsOfTypeAll<MenuTransitionsHelper>().FirstOrDefault();

            if (menuSceneSetupData != null)
            {
                Plugin.log.Debug(
                    $"Starting song: name={level.levelResult.beatmapLevel.songName}, levelId={level.levelResult.beatmapLevel.levelID}");

                try
                {
                    BS_Utils.Gameplay.Gamemode.NextLevelIsIsolated("TestMod");
                }
                catch
                {
                }

                PracticeSettings practiceSettings = null;
                if (startTime > 1f || level.speed != 1f)
                {
                    practiceSettings = new PracticeSettings(PracticeSettings.defaultPracticeSettings);
                    if (startTime > 1f)
                    {
                        practiceSettings.startSongTime = startTime + 1.5f;
                        practiceSettings.startInAdvanceAndClearNotes = true;
                    }

                    practiceSettings.songSpeedMul = level.speed != 1f ? level.speed : level.modifiers.songSpeedMul;
                }

                var transition = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().First();
                transition.Init(
                    level.difficulty,
                    level.environmentSettings,
                    level.colorScheme,
                    level.modifiers,
                    level.playerSpecificSettings,
                    practiceSettings,
                    "Menu",
                    false);
                var _gameScenesManager = Object.FindObjectOfType<GameScenesManager>();

                _gameScenesManager.PopScenes(0f, null, 
                    () => { 
                        _gameScenesManager.PushScenes(transition, 0.0f, null, () =>
                        {
                            var newScoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();

                            if (newScoreController == null)
                                Plugin.log.Info("new ScoreController was null!");

                            newScoreController.SetPrivateField("_combo", oldCombo);
                            newScoreController.SetPrivateField("_maxCombo", oldMaxCombo);
                            newScoreController.SetPrivateField("_baseRawScore", oldScore);
                            newScoreController.SetPrivateField("_prevFrameRawScore", oldScore);
                            newScoreController.SetPrivateField("_cutOrMissedNotes", oldTotalNotes);

                            Executor.instance.StartCoroutine(FinishMovingScores(oldCombo, oldScore));
                        });

                    });
            }
            else
            {
                Plugin.log.Error("SceneSetupData is null!");
            }
        }

        private static IEnumerator FinishMovingScores(int oldCombo, int oldScore)
        {
            var comboui = Resources.FindObjectsOfTypeAll<ComboUIController>().FirstOrDefault();
            var scoreui = Resources.FindObjectsOfTypeAll<ScoreUIController>().FirstOrDefault();

            while (comboui == null || scoreui == null)
            {
                yield return null;

                comboui = Resources.FindObjectsOfTypeAll<ComboUIController>().FirstOrDefault();
                scoreui = Resources.FindObjectsOfTypeAll<ScoreUIController>().FirstOrDefault();
            }

            yield return new WaitForSeconds(0.2f);

            comboui.HandleComboDidChange(oldCombo);
            scoreui.HandleScoreDidChangeRealtime(oldScore, oldScore);

            Plugin.log.Info("Score update successful!");
        }

        // Taken from BeatSaberMultiplayer
        public static IDifficultyBeatmap GetDifficultyBeatmap(this IBeatmapLevel level,
            BeatmapCharacteristicSO characteristic, BeatmapDifficulty difficulty, bool strictDifficulty = false)
        {
            IDifficultyBeatmapSet difficultySet = null;
            if (characteristic == null)
            {
                difficultySet = level.beatmapLevelData.difficultyBeatmapSets.FirstOrDefault();
            }
            else
            {
                difficultySet =
                    level.beatmapLevelData.difficultyBeatmapSets.FirstOrDefault(x =>
                        x.beatmapCharacteristic == characteristic);
                if (difficultySet == null)
                    difficultySet = level.beatmapLevelData.difficultyBeatmapSets.FirstOrDefault();
            }

            if (difficultySet == null)
            {
                Plugin.log.Error("Unable to find any difficulty set!");
                return null;
            }

            var beatmap = difficultySet.difficultyBeatmaps.FirstOrDefault(x => x.difficulty == difficulty);

            if (beatmap == null && !strictDifficulty)
            {
                var index = GetClosestDifficultyIndex(difficultySet.difficultyBeatmaps, difficulty);
                if (index >= 0)
                {
                    return difficultySet.difficultyBeatmaps[index];
                }
                else
                {
                    Plugin.log.Error("Unable to find difficulty!");
                    return null;
                }
            }
            else
            {
                return beatmap;
            }
        }

        public static int GetClosestDifficultyIndex(IDifficultyBeatmap[] beatmaps, BeatmapDifficulty difficulty)
        {
            var num = -1;
            foreach (var difficultyBeatmap in beatmaps)
            {
                if (difficulty < difficultyBeatmap.difficulty) break;
                num++;
            }

            if (num == -1) num = 0;
            return num;
        }
    }
}