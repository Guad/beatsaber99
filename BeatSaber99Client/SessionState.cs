﻿namespace BeatSaber99Client
{
    public static class SessionState
    {
        public static int PlayersLeft;
        public static int CurrentCombo;
        public static int Score;
        public static float Energy;

        public static string CurrentItem;
        public static void Clean()
        {
            PlayersLeft = 99;
            CurrentCombo = 0;
            Score = 0;
            Energy = 0;
            CurrentItem = null;
        }
    }
}