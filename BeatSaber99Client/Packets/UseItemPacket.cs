namespace BeatSaber99Client.Packets
{
    public class UseItemPacket : IPacket
    {
        public string type { get; set; } = nameof(PlayerStateUpdatePacket);

        public void Dispatch()
        {
        }
    }
}