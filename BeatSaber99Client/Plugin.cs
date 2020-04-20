using System.Collections.Generic;
using System.IO;
using System.Threading;
using BeatSaber99Client.Assets;
using BeatSaber99Client.Game;
using BeatSaber99Client.Items;
using BeatSaber99Client.Session;
using BeatSaber99Client.UI;
using BS_Utils.Gameplay;
using BS_Utils.Utilities;
using IPA;
using IPA.Config.Stores;
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
        public Plugin(Logger logger, Config config)
        {
            log = logger;
            PluginConfig.Instance = config.Generated<PluginConfig>();
        }

        [OnStart]
        public void OnStart()
        {
            BSEvents.OnLoad();
            Sprites.Init();
            Gamemode.Init();
            GetUserInfo.UpdateUserInfo();
            HarmonyPatcher.Patch();
            Executor.Init();
            Client.Init();
            Gameplay.Init();
            Jukebox.Init();
            ItemManager.Init();
            ScoreManager.Init();

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

        /// <summary>
        /// Clean up the songs downloaded during gameplay.
        /// </summary>
        private static void Cleanup()
        {
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

            CleanPaths.Clear();
        }

        [OnExit]
        public void OnExit()
        {
            Client.Disconnect();

            Cleanup();
        }
    }
}
