namespace BeatSaber99Client.Game
{
    /// <summary>
    /// Represents a fully preloaded level which can be switched to at moment's notice.
    /// </summary>
    public class PreloadedLevel
    {
        public BeatmapLevelsModel.GetBeatmapLevelResult levelResult { get; set; }
        public BeatmapCharacteristicSO characteristic { get; set; }
        public GameplayModifiers modifiers { get; set; }
        public IDifficultyBeatmap difficulty { get; set; }
        public PlayerData playerData { get; set; }
        public PlayerSpecificSettings playerSpecificSettings { get; set; }
        public OverrideEnvironmentSettings environmentSettings { get; set;  }
        public ColorScheme colorScheme { get; set; }

        public float speed { get; set; }

    }
}