namespace BeatSaber99Client.Session
{
    public static class ScoreManager
    {
        public static void Init()
        {
            Client.ClientStatusChanged += ClientOnClientStatusChanged;
        }

        private static void ClientOnClientStatusChanged(object sender, ClientStatus e)
        {
            if (e == ClientStatus.Playing && Client.Status == ClientStatus.Matchmaking)
            {
                BS_Utils.Gameplay.ScoreSubmission.ProlongedDisableSubmission("Beat Saber 99");
            }
            else if (e == ClientStatus.Waiting && Client.Status == ClientStatus.Playing)
            {
                BS_Utils.Gameplay.ScoreSubmission.RemoveProlongedDisable("Beat Saber 99");
            }
        }
    }
}