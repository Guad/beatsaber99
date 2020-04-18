namespace BeatSaber99Client.Packets
{
    public class ConnectionPacket : IPacket
    {
        public string type { get; set; } = nameof(ConnectionPacket);
        public string name { get; set; }
        public string id { get; set; }
        public string platform { get; set; }
        public int version { get; set; }

        public void Dispatch() {}
    }
}