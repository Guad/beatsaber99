namespace BeatSaber99Client.Packets
{
    public class EnqueueSongPacket : IPacket
    {
        public string Characteristic { get; set; }
        public string LevelID { get; set; }
        public BeatmapDifficulty Difficulty { get; set; }


        public void Dispatch()
        {
            Plugin.log.Info($"Enqueued song {LevelID}");
            Jukebox.SongQueue.Enqueue(this);
        }
    }
}