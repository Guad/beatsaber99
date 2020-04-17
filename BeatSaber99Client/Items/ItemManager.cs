using System.Linq;
using BeatSaber99Client.Packets;
using BeatSaber99Client.UI;
using UnityEngine;
using UnityEngine;
using UnityEngine.XR;

namespace BeatSaber99Client.Items
{
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager instance;

        public static float? Brink = null;
        public static float? Invulnerable = null;
        public static float AddHealthNextFrame;

        public float? _triggerHelddownStart;
        private GameEnergyCounter _gameEnergyCounter;

        public static void Init()
        {
            new GameObject("beatsaber99_itemmanager").AddComponent<ItemManager>();
        }
        void Start()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            Client.ClientStatusChanged += ClientOnClientStatusChanged;
        }

        void Clean()
        {
            Brink = null;
            Invulnerable = null;
            AddHealthNextFrame = 0;
            _gameEnergyCounter = null;
        }

        private void ClientOnClientStatusChanged(object sender, ClientStatus e)
        {
            switch (e)
            {
                case ClientStatus.Playing:
                    Clean();
                    break;
            }
        }

        void Update()
        {
            if (Client.Status != ClientStatus.Playing) return;
            
            if (_gameEnergyCounter == null)
                _gameEnergyCounter = Resources.FindObjectsOfTypeAll<GameEnergyCounter>().FirstOrDefault();

            if (_gameEnergyCounter == null) return;

            if (Invulnerable.HasValue)
            {
                if (Time.time - Invulnerable.Value > 10f)
                {
                    Invulnerable = null;
                }
                else
                    _gameEnergyCounter.AddEnergy(1.0f - _gameEnergyCounter.energy);
            }
            else if(Brink.HasValue)
            {
                if (Time.time - Brink.Value > 10f)
                {
                    Brink = null;
                }
                else if (_gameEnergyCounter.energy > 0f)
                    _gameEnergyCounter.AddEnergy(0.05f - _gameEnergyCounter.energy);
            }
            else if (AddHealthNextFrame > 0 && _gameEnergyCounter.energy > 0f)
            {
                _gameEnergyCounter.AddEnergy(AddHealthNextFrame);
                AddHealthNextFrame = 0;
            }

            float trigger = Mathf.Max(Input.GetAxis("TriggerRightHand"), Input.GetAxis("TriggerLeftHand"));

            if (trigger > 0.6)
            {
                if (_triggerHelddownStart == null)
                    _triggerHelddownStart = Time.time;
                else if (Time.time - _triggerHelddownStart.Value > 1.0f)
                {
                    _triggerHelddownStart = null;
                    UseCurrentItem();
                }
            }
        }

        public static void UseCurrentItem()
        {
            if (string.IsNullOrEmpty(SessionState.CurrentItem)) return;
            Plugin.log.Info("Using current item");

            Client.Send(new ActivateItemPacket()
            {
                ItemType = SessionState.CurrentItem,
            });

            switch (SessionState.CurrentItem)
            {
                case "One Hit Fail":
                    break;
                default:
                    ActivateItem(SessionState.CurrentItem);
                    PluginUI.instance.PushEventLog("Used current item!");
                    break;
            }

            SessionState.CurrentItem = null;
            PluginUI.instance.SetCurrentItem(null);
        }

        public static void ActivateItem(string item)
        {
            switch (item)
            {
                case "+Health":
                    AddHealthNextFrame = 0.1f;
                    break;
                case "++Health":
                    AddHealthNextFrame = 0.2f;
                    break;
                case "+++Health":
                    AddHealthNextFrame = 0.5f;
                    break;
                case "Invulnerability":
                    Invulnerable = Time.time;
                    break;
                case "One Hit Fail":
                    Brink = Time.time;
                    break;
            }

        }
    }
}