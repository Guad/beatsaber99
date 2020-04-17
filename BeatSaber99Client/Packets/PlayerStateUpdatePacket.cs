namespace BeatSaber99Client.Packets
{
    public class PlayerStateUpdatePacket : IPacket
    {
        public string type { get; set; } = nameof(PlayerStateUpdatePacket);
        public int Score { get; set; }
        public float Energy { get; set; }
        public int CurrentCombo { get; set; }

        public void Dispatch()
        {
            
        }
    }
}