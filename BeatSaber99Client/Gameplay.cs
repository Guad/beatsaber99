using System.Linq;
using BS_Utils.Utilities;
using HarmonyLib;
using UnityEngine;

namespace BeatSaber99Client
{
    public static class Gameplay
    {
        public static void Init()
        {
            BSEvents.energyDidChange += BSEvents_energyDidChange;
            BSEvents.noteWasCut += BSEvents_noteWasCut;
            BSEvents.noteWasMissed += BSEvents_noteWasMissed;

            BSEvents.levelFailed += BSEvents_levelFailed;
            BSEvents.levelCleared += BSEvents_levelCleared;
            BSEvents.levelQuit += BSEventsOnlevelQuit;
            // BSEvents.scoreDidChange += BSEvents_scoreDidChange;

            BSEvents.levelSelected += BSEventsOnlevelSelected;

            /*
            _scoreController = Object.FindObjectOfType<ScoreController>();
            _gameManager = Object.FindObjectOfType<StandardLevelGameplayManager>();
            _energyCounter = Object.FindObjectOfType<GameEnergyCounter>();
            _pauseMenuManager = Object.FindObjectOfType<PauseMenuManager>();
            _menuTransitionsHelper = Object.FindObjectOfType<MenuTransitionsHelper>();*/
        }

        private static void BSEventsOnlevelSelected(LevelCollectionViewController arg1, IPreviewBeatmapLevel arg2)
        {
            Plugin.log.Info($"Selected: {arg2.songName} - {arg2.songAuthorName} ({arg2.levelID})");
        }

        private static void BSEventsOnlevelQuit(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            Client.Disconnect();
        }

        private static void BSEvents_levelCleared(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            Plugin.log.Info("Level cleared");

            /*
            if (Client.Status == ClientStatus.Playing)
            {
                // Loop around
                Plugin.log.Info("Restarting level");

                LevelLoader.LoadBeatmapLevelAsync(
                    LevelLoader.StandardCharacteristic,
                    LevelLoader.AllLevels.First(),
                    BeatmapDifficulty.Expert,
                    null
                    );
            }
            */

            // Client.Disconnect();
        }

        private static void BSEvents_levelFailed(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            Client.Disconnect();
        }

        private static void BSEvents_scoreDidChange(int obj)
        {
            // Plugin.log.Info($"Score: {obj}");
        }

        private static void BSEvents_noteWasMissed(NoteData arg1, int arg2)
        {
            // Plugin.log.Info($"Note missed: {arg1.id} ({arg2})");
        }

        private static void BSEvents_noteWasCut(NoteData arg1, NoteCutInfo arg2, int arg3)
        {
            // Plugin.log.Info($"Note cut: {arg1.id} w/ saber {arg2.saberType} ({arg3})");
        }


        private static void BSEvents_energyDidChange(float obj)
        {
            // Plugin.log.Info("NEW ENERGY: " + obj);
        }

    }
}