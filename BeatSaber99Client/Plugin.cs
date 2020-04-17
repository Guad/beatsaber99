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
        

        [Init]
        public Plugin(Logger logger)
        {
            log = logger;
        }

        [OnStart]
        public void OnStart()
        {
            BSEvents.OnLoad();
            Gamemode.Init();
            GetUserInfo.UpdateUserInfo();
            HarmonyPatcher.Patch();
            Executor.Init();
            Client.Init();
            Gameplay.Init();
            Jukebox.Init();

            BSEvents.menuSceneLoadedFresh += () => PluginUI.Init();
            BSEvents.gameSceneActive += BSEventsOngameSceneActive;
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

            log.Info("Beat Saber 99 started");
        }

        private void BSEventsOngameSceneActive()
        {
            PluginUI.instance.SetupIngameUI();
        }


        private void SceneManager_activeSceneChanged(Scene from, Scene to)
        {
            log.Info($"Scene change from {@from.name} to {to.name}");

            if (to.name.ToLower().Contains("menu"))
            {
                LevelLoader.Init();
            }
        }

        [OnExit]
        public void OnExit()
        {
            Client.Disconnect();
        }
    }
}
