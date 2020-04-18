namespace BeatSaber99Client
{
    public class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public string ServerAddress { get; set; } = "wss://beatsaber.kolhos.chichasov.es/ws";
    }
}