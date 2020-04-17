using BeatSaber99Client.UI;

namespace BeatSaber99Client.Packets
{
    public class GiveItemPacket : IPacket
    {
        public string ItemType { get; set; }
        
        public void Dispatch()
        {
            SessionState.CurrentItem = ItemType;
            PluginUI.instance.SetCurrentItem(ItemType);
        }
    }
}