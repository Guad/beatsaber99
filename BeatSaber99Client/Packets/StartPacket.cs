using System.Linq;

namespace BeatSaber99Client.Packets
{
    public class StartPacket : IPacket
    {
        public int TotalPlayers { get; set; }
        public BeatmapDifficulty Difficulty { get; set; }
        public string LevelID { get; set; }

        public void Dispatch()
        {
            SessionState.PlayersLeft = TotalPlayers;
            var level = LevelLoader.AllLevels.First(lvl => lvl.levelID == LevelID);
            LevelLoader.LoadBeatmapLevelAsync(LevelLoader.StandardCharacteristic, level, Difficulty, null);

            Jukebox.instance.TrackSong(level.songDuration);

            Client.Status = ClientStatus.Playing;
        }
    }
}