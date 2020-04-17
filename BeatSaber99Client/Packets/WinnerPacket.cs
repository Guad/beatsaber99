using BeatSaber99Client.UI;

namespace BeatSaber99Client.Packets
{
    public class WinnerPacket : IPacket
    {
        public void Dispatch()
        {
            Plugin.log.Info("Winner packet received.");
            PluginUI.instance.SetWinnerText(true);
            Client.Disconnect();
        }
    }
}