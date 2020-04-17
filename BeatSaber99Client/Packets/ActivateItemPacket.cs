using BeatSaber99Client.Items;

namespace BeatSaber99Client.Packets
{
    public class ActivateItemPacket : IPacket
    {
        public string type { get; set; } = nameof(ActivateItemPacket);

        public string ItemType { get; set; }
        public void Dispatch()
        {
            ItemManager.ActivateItem(ItemType);
        }
    }
}