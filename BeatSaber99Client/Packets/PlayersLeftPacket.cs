using System.Linq;
using BeatSaber99Client.UI;

namespace BeatSaber99Client.Packets
{
    public class PlayersLeftPacket : IPacket
    {
        public int TotalPlayers { get; set; }

        public void Dispatch()
        {
            SessionState.PlayersLeft = TotalPlayers;
            PluginUI.instance.UpdatePlayersLeftText(this.TotalPlayers);
        }
    }
}