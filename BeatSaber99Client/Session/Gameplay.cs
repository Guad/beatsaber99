﻿using System.Linq;
using System.Threading;
using BeatSaber99Client.Packets;
using BeatSaber99Client.UI;
using BS_Utils.Utilities;
using UnityEngine;

namespace BeatSaber99Client.Session
{
    /// <summary>
    /// Keep track of gameplay stuff like energy levels, level quits, combos, etc.
    /// Also send our data to server periodically.
    /// </summary>
    public class Gameplay : MonoBehaviour
    {
        private ScoreController _scoreController;

        public static void Init()
        {
            new GameObject("beatsaber99_gameplay").AddComponent<Gameplay>();
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);


            BSEvents.energyDidChange += BSEvents_energyDidChange;
            BSEvents.energyReachedZero += BSEventsOnenergyReachedZero;

            BSEvents.levelFailed += BSEvents_levelFailed;
            BSEvents.levelCleared += BSEvents_levelCleared;
            BSEvents.levelQuit += BSEventsOnlevelQuit;
            BSEvents.levelRestarted += BSEventsOnlevelRestarted;

            BSEvents.comboDidChange += BSEventsOncomboDidChange;


            var t = new Thread(DataSender);
            t.Start();
        }

        void Update()
        {
            if (Client.Status == ClientStatus.Playing &&_scoreController == null)
            {
                _scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();

                if (_scoreController != null)
                {
                    Plugin.log.Info("Scores hooked");

                    _scoreController.scoreDidChangeEvent += (score, afterModifiers) =>
                    {
                        BSEvents_scoreDidChange(score);
                    };
                }
            }
        }

        void DataSender()
        {
            while (true)
            {
                if (Client.Status == ClientStatus.Playing)
                {
                    Client.Send(new PlayerStateUpdatePacket()
                    {
                        CurrentCombo = SessionState.CurrentCombo,
                        Energy = SessionState.Energy,
                        Score = SessionState.Score,
                    });
                }

                Thread.Sleep(1000);
            }
        }

        private void BSEventsOnlevelQuit(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            PluginUI.instance.SetWinnerText(false);
            if (Client.Status != ClientStatus.Playing) return;
            Client.Disconnect();
        }


        private void BSEventsOnlevelRestarted(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            PluginUI.instance.SetWinnerText(false);
            if (Client.Status != ClientStatus.Playing) return;
            Client.Disconnect();
        }


        private void BSEvents_levelCleared(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            PluginUI.instance.SetWinnerText(false);
            if (Client.Status != ClientStatus.Playing) return;
            Plugin.log.Info("Level cleared");
            Client.Disconnect();
        }

        private void BSEventsOnenergyReachedZero()
        {
            if (Client.Status != ClientStatus.Playing) return;
            Client.Disconnect();
        }

        private void BSEvents_levelFailed(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            PluginUI.instance.SetWinnerText(false);
            if (Client.Status != ClientStatus.Playing) return;
            Client.Disconnect();
        }

        private void BSEvents_scoreDidChange(int obj)
        {
            if (Client.Status != ClientStatus.Playing) return;
            SessionState.Score = obj;
        }

        private void BSEventsOncomboDidChange(int obj)
        {
            if (Client.Status != ClientStatus.Playing) return;
            SessionState.CurrentCombo = obj;
        }

        private void BSEvents_energyDidChange(float obj)
        {
            if (Client.Status != ClientStatus.Playing) return;
            SessionState.Energy = obj;
        }

    }
}