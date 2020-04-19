using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatSaber99Client.Assets;
using BeatSaber99Client.Packets;
using BeatSaber99Client.Session;
using BeatSaberMarkupLanguage;
using BS_Utils.Utilities;
using HMUI;
using Polyglot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BeatSaberUI = BeatSaberMarkupLanguage.BeatSaberUI;
using BSEvents = BS_Utils.Utilities.BSEvents;

namespace BeatSaber99Client.UI
{
    public class PluginUI : MonoBehaviour
    {
        public static PluginUI instance;
        public static TMPro.TextMeshProUGUI hudText;
        public static DynamicText playersLeftText;
        public static DynamicText winnerText;
        public static DynamicText itemText;
        public static Button _multiplayerButton;


        private static Coroutine _eventLogEmptyingCoroutine;
        private float _lastMessageTime;
        private bool _animatingEventLog;
        private Queue<string> _eventLogQueue = new Queue<string>();

        private DynamicText[] _eventLog;
        public static string[] _log_history = new string[3];

        private Vector3[] positions = new []
        {
            new Vector3(-2.5f, 2.3f, 8f),
            new Vector3(-2.5f, 2.45f, 8f),
            new Vector3(-2.5f, 2.55f, 8f),
            new Vector3(-2.5f, 2.62f, 8f),
        };

        private float[] fontSizes = new[]
        {
            8.0f,
            5.0f,
            4.0f,
            3.0f,
        };


        private MainMenuViewController _mainMenuViewController;
        private RectTransform _mainMenuRectTransform;

        private const string ButtonText = "SABER99";

        public static void Init()
        {
            if (instance == null)
            {
                instance = new GameObject("beatsaber99_ui").AddComponent<PluginUI>();
                instance.Setup();
            }
        }

        public void Setup()
        {
            DontDestroyOnLoad(this.gameObject);

            _mainMenuViewController = Resources.FindObjectsOfTypeAll<MainMenuViewController>().First();
            _mainMenuRectTransform = _mainMenuViewController.transform as RectTransform;

            CreateText();
            CreateButton();
            Client.ClientStatusChanged += ClientOnClientStatusChanged;
            
            StartCoroutine(EnableButtonFirstLaunch());
        }

        IEnumerator EnableButtonFirstLaunch()
        {
            yield return new WaitForSeconds(3.0f);

            _multiplayerButton.interactable = true;
        }

        public void SetupIngameUI()
        {
            if (winnerText != null)
                winnerText.Delete();
            if (playersLeftText != null)
                playersLeftText.Delete();
            if (itemText != null)
                itemText.Delete();

            if (_eventLog != null && _eventLog.Length > 0 && _eventLog[0] != null)
                foreach (var text in _eventLog)
                    text.Delete();


            winnerText = DynamicText.Create(
                new Vector3(-2f, 2f, 8f),
                40.0f
            );

            playersLeftText = DynamicText.Create(
                new Vector3(2.5f, 2.5f, 6.5f),
                8.0f
            );

            playersLeftText.text.text = "Players Left: " + SessionState.PlayersLeft;

            _eventLog = new DynamicText[3];

            for (int i = 0; i < _eventLog.Length; i++)
            {
                _eventLog[i] = DynamicText.Create(positions[i], fontSizes[i]);
                _eventLog[i].text.alignment = TextAlignmentOptions.Center;
                _eventLog[i].text.text = _log_history[i] ?? "";
            }

            itemText = DynamicText.Create(
                new Vector3(-3.5f, 3f, 6.5f),
                8.0f
            );

            if (SessionState.CurrentItem != null)
                SetCurrentItem(SessionState.CurrentItem);

            if (_eventLogEmptyingCoroutine != null)
                StopCoroutine(_eventLogEmptyingCoroutine);

            _eventLogEmptyingCoroutine = StartCoroutine(RemoveOldMessagesCoroutine());
        }

        IEnumerator RemoveOldMessagesCoroutine()
        {
            while (Client.Status == ClientStatus.Playing)
            {
                if (Time.time - _lastMessageTime > 5.0f)
                {
                    PushEventLog("");
                }

                yield return new WaitForSeconds(3f);
            }
        }

        public void PushEventLog(string newtext)
        {
            _lastMessageTime = Time.time;
            _eventLogQueue.Enqueue(newtext);

            if (!_animatingEventLog)
                StartCoroutine(AnimateEventLogs());
        }

        public void SetCurrentItem(string item)
        {
            if (itemText == null) return;

            if (item == null)
            {
                itemText.text.text = "";
            }
            else
            {
                itemText.text.text = "Current Item:\n" + item;
            }
        }

        private IEnumerator AnimateEventLogs()
        {
            _animatingEventLog = true;

            do
            {
                string newText = _eventLogQueue.Dequeue();

                const float animationTime = 0.7f;
                float start = Time.time;
                float now = Time.time;

                while (now < start + animationTime)
                {
                    now = Time.time;
                    float delta = (now - start) / animationTime;
                    if (delta > 1.0f) delta = 1.0f;

                    delta = Mathf.Sin(delta * (Mathf.PI / 2.0f));

                    for (int i = 0; i < _eventLog.Length; i++)
                    {
                        float newfont = delta * (fontSizes[i + 1] - fontSizes[i]) + fontSizes[i];
                        Vector3 newpos = delta * (positions[i + 1] - positions[i]) + positions[i];

                        _eventLog[i].text.fontSize = newfont;
                        _eventLog[i].SetPosition(newpos);
                    }

                    yield return new WaitForEndOfFrame();
                }

                for (int i = _eventLog.Length - 1; i >= 0; i--)
                {
                    _eventLog[i].text.text = i == 0 ? newText : _eventLog[i - 1].text.text;
                    _log_history[i] = _eventLog[i].text.text;
                    _eventLog[i].text.fontSize = fontSizes[i];
                    _eventLog[i].SetPosition(positions[i]);
                }
            } while (_eventLogQueue.Count > 0);

            _animatingEventLog = false;
        }


        public void UpdatePlayersLeftText(int left)
        {
            Plugin.log.Info($"Updated player count: {left}");

            if (playersLeftText != null)
            {
                playersLeftText.text.text = "Players Left: " + left;
            }
        }

        public void SetWinnerText(bool active)
        {
            if (winnerText != null)
                winnerText.text.text = active ? "Victory Royale!" : "";
        }
        
        private void ClientOnClientStatusChanged(object sender, ClientStatus e)
        {
            switch (e)
            {
                case ClientStatus.Waiting:
                    hudText.text = Client.Status == ClientStatus.Playing ? $"Placed: {SessionState.PlayersLeft}" : $"";

                    if (Client.Status == ClientStatus.Playing)
                    {
                        if (winnerText != null) 
                            winnerText.Delete();
                        if (playersLeftText != null)
                            playersLeftText.Delete();
                        if (itemText != null)
                            itemText.Delete();

                        winnerText = playersLeftText = itemText = null;
                    }

                    playersLeftText?.gameObject?.SetActive(false);
                    SetCurrentItem(null);

                    BeatSaberUI.SetButtonText(_multiplayerButton, ButtonText);
                    _multiplayerButton.interactable = true;
                    break;
                case ClientStatus.Connecting:
                    _multiplayerButton.interactable = false;
                    hudText.gameObject.SetActive(true);
                    hudText.text = "Connecting...";
                    break;
                case ClientStatus.Matchmaking:
                    BeatSaberUI.SetButtonText(_multiplayerButton, "CANCEL");
                    _multiplayerButton.interactable = true;
                    hudText.text = "Matchmaking...";
                    hudText.gameObject.SetActive(true);
                    SetWinnerText(false);
                    break;
                case ClientStatus.Playing:
                    _log_history = new string[3];
                    hudText.text = "Starting...";
                    break;
            }
        }

        private void CreateText()
        {
            hudText = BeatSaberUI.CreateText(_mainMenuRectTransform, "Matchmaking...", new Vector2(55.5f, 33f));
            hudText.fontSize = 5f;
            hudText.lineSpacing = -52;
            hudText.gameObject.SetActive(false);
        }

        private void CreateButton()
        {
            Button[] mainButtons = Resources.FindObjectsOfTypeAll<RectTransform>()
                .First(x => x.name == "MainButtons" && x.parent.name == "MainMenuViewController")
                .GetComponentsInChildren<Button>();

            foreach (var item in mainButtons)
            {
                (item.transform as RectTransform).sizeDelta = new Vector2(35f, 30f);
            }

            _multiplayerButton = Instantiate(
                Resources.FindObjectsOfTypeAll<Button>()
                    .Last(x => (x.name == "SoloFreePlayButton")),
                _mainMenuRectTransform, false);
            _multiplayerButton.name = "BS99Button";
            Destroy(_multiplayerButton.GetComponentInChildren<LocalizedTextMeshProUGUI>());
            Destroy(_multiplayerButton.GetComponentInChildren<HoverHint>());
            _multiplayerButton.transform.SetParent(mainButtons.First(x => x.name == "SoloFreePlayButton").transform.parent);
            _multiplayerButton.transform.SetAsLastSibling();

            BeatSaberUI.SetButtonText(_multiplayerButton, ButtonText);
            _multiplayerButton.SetButtonIcon(Sprites.logoIcon);

            _multiplayerButton.interactable = false;

            _multiplayerButton.onClick = new Button.ButtonClickedEvent();
            _multiplayerButton.onClick.AddListener(delegate ()
            {
                BattleRoyaleClicked();
            });
        }

        private void BattleRoyaleClicked()
        {
            if (Client.Status == ClientStatus.Matchmaking)
            {
                Client.Disconnect();
            }
            else
            {
                Client.ConnectAndMatchmake();
            }
        }

        public void SetEnergyBarColor(Color color)
        {
            var _energyPanel = Resources.FindObjectsOfTypeAll<GameEnergyUIPanel>().FirstOrDefault();
            if (_energyPanel == null) return;
            Image energyBar = _energyPanel.GetField<Image>("_energyBar");
            if (energyBar == null) return;
            energyBar.color = color;

        }
    }
}