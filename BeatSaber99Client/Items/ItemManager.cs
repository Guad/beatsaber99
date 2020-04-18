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
        public static float? Shield = null;
        public static float? Poison = null;
        private static float? _oldEnergy;

        public static float AddHealthNextFrame;
        public static bool StartGhostNotesNextFrame;
        public static bool StartGhostArrowsNextFrame;


        private const float ItemDuration = 5f;
        private const float BombChance = 0.2f;

        public float _triggerLastPush = 0;
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
            Shield = null;
            Poison = null;
            _oldEnergy = null;
            StartGhostArrowsNextFrame = false;
            StartGhostNotesNextFrame = false;
            PluginUI.instance.SetEnergyBarColor(Color.white);
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
                if (Time.time - Invulnerable.Value > ItemDuration)
                {
                    Invulnerable = null;
                    PluginUI.instance.SetEnergyBarColor(Color.white);
                }
                else
                    _gameEnergyCounter.AddEnergy(1.0f - _gameEnergyCounter.energy);
            }
            else if(Brink.HasValue)
            {
                if (Time.time - Brink.Value > ItemDuration)
                {
                    Brink = null;
                    PluginUI.instance.SetEnergyBarColor(Color.white);
                }
                else if (_gameEnergyCounter.energy > 0f)
                    _gameEnergyCounter.AddEnergy(0.05f - _gameEnergyCounter.energy);
            }
            else if (Poison.HasValue)
            {
                if (Time.time - Poison.Value > ItemDuration)
                {
                    Poison = null;
                    _oldEnergy = null;
                    PluginUI.instance.SetEnergyBarColor(Color.white);
                }
                else if (_oldEnergy == null)
                {
                    _oldEnergy = _gameEnergyCounter.energy;
                }
                else if (_gameEnergyCounter.energy > _oldEnergy.Value)
                {
                    _gameEnergyCounter.AddEnergy(_oldEnergy.Value - _gameEnergyCounter.energy);
                }
                else
                {
                    _oldEnergy = _gameEnergyCounter.energy;
                }
            }
            else if (AddHealthNextFrame > 0 && _gameEnergyCounter.energy > 0f)
            {
                _gameEnergyCounter.AddEnergy(AddHealthNextFrame);
                AddHealthNextFrame = 0;
            }

            if (Shield.HasValue && Time.time - Shield.Value > ItemDuration)
            {
                Shield = null;
                PluginUI.instance.SetEnergyBarColor(Color.white);
            }

            if (StartGhostNotesNextFrame)
            {
                StartCoroutine(BeatmapSpawnManager.instance.ReplaceNextXNotesWithGhostNotesCoroutine(ItemDuration));
                StartGhostNotesNextFrame = false;
            }

            if (StartGhostArrowsNextFrame)
            {
                StartCoroutine(BeatmapSpawnManager.instance.ReplaceNextXNotesWithDisappearingArrowsCoroutine(ItemDuration));
                StartGhostArrowsNextFrame = false;
            }


            float trigger = Mathf.Max(Input.GetAxis("TriggerRightHand"), Input.GetAxis("TriggerLeftHand"));

            if (trigger > 0.6 && Time.time - _triggerLastPush > 1.5f)
            {
                UseCurrentItem();
                _triggerLastPush = Time.time;
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
                case ItemTypes.Brink:
                case ItemTypes.Poison:
                case ItemTypes.SwapNotes:
                case ItemTypes.SendBombs:
                case ItemTypes.GhostArrows:
                case ItemTypes.GhostNotes:
                    break;
                default:
                    ActivateItem(SessionState.CurrentItem);
                    PluginUI.instance.PushEventLog("Used current item!");
                    break;
            }

            SessionState.CurrentItem = null;
            PluginUI.instance.SetCurrentItem(null);
        }

        private static bool IsProtectedFrom(string item)
        {
            switch (item)
            {
                case ItemTypes.Brink:
                case ItemTypes.Poison:
                case ItemTypes.SwapNotes:
                case ItemTypes.SendBombs:
                case ItemTypes.GhostArrows:
                case ItemTypes.GhostNotes:
                    return Shield.HasValue;
            }

            return false;
        }

        public static void ActivateItem(string item)
        {
            if (IsProtectedFrom(item)) return;

            switch (item)
            {
                case ItemTypes.Health1:
                    AddHealthNextFrame = 0.1f;
                    break;
                case ItemTypes.Health2:
                    AddHealthNextFrame = 0.2f;
                    break;
                case ItemTypes.Health3:
                    AddHealthNextFrame = 0.5f;
                    break;
                case ItemTypes.Invulnerability:
                    Invulnerable = Time.time;
                    PluginUI.instance.SetEnergyBarColor(Color.magenta);
                    break;
                case ItemTypes.Brink:
                    Brink = Time.time;
                    PluginUI.instance.SetEnergyBarColor(Color.red);
                    break;
                case ItemTypes.Poison:
                    Poison = Time.time;
                    PluginUI.instance.SetEnergyBarColor(Color.green);
                    break;
                case ItemTypes.Shield:
                    Shield = Time.time;
                    PluginUI.instance.SetEnergyBarColor(Color.blue);
                    break;
                case ItemTypes.NoArrows:
                    BeatmapSpawnManager.instance.ReplaceNextXNotesWithAnyDirection(ItemDuration);
                    break;
                case ItemTypes.SwapNotes:
                    BeatmapSpawnManager.instance.ReplaceNextXNotesWithEachother(ItemDuration);
                    break;
                case ItemTypes.SendBombs:
                    BeatmapSpawnManager.instance.ReplaceNextXNotesWithRandomBombs(ItemDuration, BombChance);
                    break;
                case ItemTypes.GhostNotes:
                    StartGhostNotesNextFrame = true;
                    break;
                case ItemTypes.GhostArrows:
                    StartGhostArrowsNextFrame = true;
                    break;
            }

        }
    }
}