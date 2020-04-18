using System.Collections.Generic;
using System.IO;
using BeatSaber99Client.Items;
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
        public static List<string> CleanPaths = new List<string>();
        

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
            ItemManager.Init();

            BSEvents.menuSceneLoadedFresh += BSEventsOnmenuSceneLoadedFresh;
            BSEvents.gameSceneActive += BSEventsOngameSceneActive;
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

            log.Info("Beat Saber 99 started");
        }

        private void BSEventsOnmenuSceneLoadedFresh()
        {
            LevelLoader.Init();
            CustomSongInjector.Init();
        }

        private void BSEventsOngameSceneActive()
        {
            PluginUI.instance.SetupIngameUI();
            BeatmapSpawnManager.Init();
        }


        private void SceneManager_activeSceneChanged(Scene from, Scene to)
        {
            log.Info($"Scene change from {@from.name} to {to.name}");

            if (to.name.ToLower().Contains("menu"))
            {
                PluginUI.Init();
            }
        }

        [OnExit]
        public void OnExit()
        {
            Client.Disconnect();

            foreach (var path in CleanPaths)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
