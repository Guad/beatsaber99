using System.Linq;
using System.Threading;
using BeatSaber99Client.Game;
using BeatSaber99Client.Session;

namespace BeatSaber99Client.Packets
{
    public class StartPacket : IPacket
    {
        public int TotalPlayers { get; set; }
        public BeatmapDifficulty Difficulty { get; set; }
        public string LevelID { get; set; }
        public long ServerStartTime { get; set; }

        public void Dispatch()
        {
            var t = new Thread(() =>
            {
                var now = TimeSynchronizationPacket.UnixTimeMilliseconds();
                var when = ServerStartTime + Client.ServerTimeOffset;

                Plugin.log.Debug($"Start packet, when: {when}, now: {now}");

                while (when > now)
                {
                    Thread.Sleep(16);
                    now = TimeSynchronizationPacket.UnixTimeMilliseconds();
                }

                Executor.Enqueue(() =>
                {
                    SessionState.PlayersLeft = TotalPlayers;
                    Client.Status = ClientStatus.Playing;

                    var level = LevelLoader.AllLevels.First(lvl => lvl.levelID == LevelID);
                    LevelLoader.LoadBeatmapLevelAsync(LevelLoader.StandardCharacteristic, level, Difficulty, null);

                    Jukebox.instance.TrackSong(level.songDuration);

                    Plugin.log.Info("Starting synchronized level!");
                });
            });

            Client.Status = ClientStatus.Starting;
            t.Start();
        }
    }
}