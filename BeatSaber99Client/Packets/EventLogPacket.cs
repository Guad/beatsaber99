using BeatSaber99Client.UI;

namespace BeatSaber99Client.Packets
{
    public class EventLogPacket : IPacket
    {
        public string Text { get; set; }

        public void Dispatch()
        {
            PluginUI.instance.PushEventLog(Text);
        }
    }
}