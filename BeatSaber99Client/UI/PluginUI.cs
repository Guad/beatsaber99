using System.Linq;
using BeatSaber99Client.Packets;
using BeatSaberMarkupLanguage;
using HMUI;
using Polyglot;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaber99Client.UI
{
    public class PluginUI : MonoBehaviour
    {
        public static PluginUI instance;
        public static TMPro.TextMeshProUGUI hudText;
        public static TMPro.TextMeshProUGUI ingameText;
        public static Button _multiplayerButton;


        private MainMenuViewController _mainMenuViewController;
        private RectTransform _mainMenuRectTransform;

        public static void Init()
        {
            if (instance == null)
            {
                instance = new GameObject("testmod").AddComponent<PluginUI>();
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
        }

        public void SetupIngameUI()
        {
            if (ingameText != null) return;

            // BeatmapObjectSpawnController

            GameObject gameObject =
                (Resources.FindObjectsOfTypeAll<ScoreMultiplierUIController>().FirstOrDefault() as MonoBehaviour)
                ?.gameObject;

            if (gameObject == null)
            {
                Plugin.log.Info("Failed to hook ScoreMultiplier");
                return;
            }

            ingameText = BeatSaberUI.CreateText(gameObject.transform as RectTransform,
                $"Players Left: {SessionState.PlayersLeft}",
                new Vector2(-50f, 100f));
            ingameText.fontSize = 30f;
            ingameText.lineSpacing = -52;
            ingameText.gameObject.SetActive(true);
        }

        public void UpdatePlayersLeftText(int left)
        {
            ingameText.text = $"Players Left: {left}";
        }

        private void ClientOnClientStatusChanged(object sender, ClientStatus e)
        {
            switch (e)
            {
                case ClientStatus.Waiting:
                    hudText.gameObject.SetActive(false);
                    _multiplayerButton.interactable = true;
                    break;
                case ClientStatus.Connecting:
                    _multiplayerButton.interactable = false;
                    hudText.gameObject.SetActive(true);
                    hudText.text = "Connecting...";
                    break;
                case ClientStatus.Matchmaking:
                    hudText.text = "Matchmaking...";
                    break;
                case ClientStatus.Playing:
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
            _multiplayerButton.name = "BSMultiplayerButton";
            Destroy(_multiplayerButton.GetComponentInChildren<LocalizedTextMeshProUGUI>());
            Destroy(_multiplayerButton.GetComponentInChildren<HoverHint>());
            _multiplayerButton.transform.SetParent(mainButtons.First(x => x.name == "SoloFreePlayButton").transform.parent);
            _multiplayerButton.transform.SetAsLastSibling();

            _multiplayerButton.SetButtonText("SABER99");
            //_multiplayerButton.SetButtonIcon(Sprites.onlineIcon);

            _multiplayerButton.interactable = true;

            _multiplayerButton.onClick = new Button.ButtonClickedEvent();
            _multiplayerButton.onClick.AddListener(delegate ()
            {
                BattleRoyaleClicked();
            });
        }

        private void BattleRoyaleClicked()
        {
            Client.ConnectAndMatchmake();
        }
    }
}