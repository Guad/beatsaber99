using BeatSaber99Client.UI;
using BS_Utils.Gameplay;
using BS_Utils.Utilities;
using IPA;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = IPA.Logging.Logger;
using Config = IPA.Config.Config;


namespace BeatSaber99Client
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static Logger log { get; private set; }
        private ScoreController _scoreController;
        private StandardLevelGameplayManager _gameManager;
        private GameEnergyCounter _energyCounter;
        private PauseMenuManager _pauseMenuManager;
        private MenuTransitionsHelper _menuTransitionsHelper;
        

        [Init]
        public Plugin(Logger logger)
        {
            log = logger;
        }

        [OnStart]
        public void OnStart()
        {
            Gamemode.Init();
            GetUserInfo.UpdateUserInfo();
            BSEvents.OnLoad();
            HarmonyPatcher.Patch();
            Executor.Init();
            Client.Init();

            _scoreController = Object.FindObjectOfType<ScoreController>();
            _gameManager = Object.FindObjectOfType<StandardLevelGameplayManager>();
            _energyCounter = Object.FindObjectOfType<GameEnergyCounter>();
            _pauseMenuManager = Object.FindObjectOfType<PauseMenuManager>();
            _menuTransitionsHelper = Object.FindObjectOfType<MenuTransitionsHelper>();

            log.Info("Test mod started");

            BSEvents.energyDidChange += BSEvents_energyDidChange;
            BSEvents.gameSceneLoaded += BSEvents_gameSceneLoaded;
            BSEvents.menuSceneLoaded += BSEvents_menuSceneLoaded;
            BSEvents.noteWasCut += BSEvents_noteWasCut;
            BSEvents.noteWasMissed += BSEvents_noteWasMissed;

            BSEvents.levelSelected += BSEvents_levelSelected;
            BSEvents.levelFailed += BSEvents_levelFailed;
            BSEvents.levelCleared += BSEvents_levelCleared;
            BSEvents.levelQuit += BSEventsOnlevelQuit;

            BSEvents.songPaused += BSEvents_songPaused;
            
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            BSEvents.menuSceneLoadedFresh += () => PluginUI.Init();
            BSEvents.gameSceneActive += BSEventsOngameSceneActive;
        }

        private void BSEventsOngameSceneActive()
        {
            PluginUI.instance.SetupIngameUI();
        }

        private void BSEventsOnlevelQuit(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            Client.Disconnect();
        }

        private void SceneManager_activeSceneChanged(Scene from, Scene to)
        {
            log.Info($"Scene change from {@from.name} to {to.name}");

            if (to.name.ToLower().Contains("menu"))
            {
                LevelLoader.Init();
            }
        }

        private void BSEvents_levelCleared(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            log.Info($"Level cleared: {arg2.rawScore}");
            Client.Disconnect();
        }

        private void BSEvents_levelFailed(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            log.Info($"Level failed: {arg2.endSongTime}");
            // TODO: send fail packet
            Client.Disconnect();
        }

        private void BSEvents_levelSelected(LevelCollectionViewController arg1, IPreviewBeatmapLevel arg2)
        {            
            log.Info($"Level selected: {arg2.songName}, {arg2.songAuthorName}");
        }

        private void BSEvents_songPaused()
        {
            log.Info("Song paused");
        }

        private void BSEvents_scoreDidChange(int obj)
        {
            log.Info($"Score: {obj}");
        }

        private void BSEvents_noteWasMissed(NoteData arg1, int arg2)
        {
            log.Info($"Note missed: {arg1.id} ({arg2})");
        }

        private void BSEvents_noteWasCut(NoteData arg1, NoteCutInfo arg2, int arg3)
        {
            log.Info($"Note cut: {arg1.id} w/ saber {arg2.saberType} ({arg3})");
        }

        private void BSEvents_menuSceneLoaded()
        {
            log.Info("Menu loaded.");
        }

        private void BSEvents_gameSceneLoaded()
        {
            log.Info("Game scene loaded.");                        
        }

        private void BSEvents_energyDidChange(float obj)
        {
            log.Info("NEW ENERGY: " + obj);
        }

        private void BSEvents_beatmapEvent(BeatmapEventData obj)        {
            
            log.Info(string.Format("[EVENT] {0}: {1}", obj.type, obj.value));
        }

        [OnExit]
        public void OnExit()
        {
            Client.Disconnect();
        }
    }
}
