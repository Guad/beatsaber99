namespace BeatSaber99Client.Packets
{
    public static class SessionState
    {
        public static int PlayersLeft;
        public static void Clean()
        {
            PlayersLeft = 99;
        }
    }
}