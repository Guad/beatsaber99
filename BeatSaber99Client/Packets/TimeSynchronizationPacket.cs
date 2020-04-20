using System;
using BeatSaber99Client.Session;

namespace BeatSaber99Client.Packets
{
    public class TimeSynchronizationPacket : IPacket
    {
        public string type { get; set; } = nameof(TimeSynchronizationPacket);

        public long PeerTime { get; set; }
        public long ProcessTime { get; set; }

        public void Dispatch()
        {
            var receive = UnixTimeMilliseconds();

            var offset = ((ProcessTime - PeerTime) + (ProcessTime - receive)) / 2;

            Plugin.log.Info($"Time offset from server: {offset}");

            // Once we synchronized clocks, we can start matchmaking
            Client.ServerTimeOffset = offset;
            Client.StartMatchmaking();
        }

        public static long UnixTimeMilliseconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}