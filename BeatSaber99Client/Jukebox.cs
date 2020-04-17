using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BeatSaber99Client.Packets;
using UnityEngine;

namespace BeatSaber99Client
{
    public class Jukebox : MonoBehaviour
    {
        public static ConcurrentQueue<EnqueueSongPacket> SongQueue = new ConcurrentQueue<EnqueueSongPacket>();
        public static Jukebox instance;

        private float songStart;
        private float songDuration;

        private PreloadedLevel nextLevel;
        private bool songPreloaded;

        public static void Init()
        {
            new GameObject("beatsaber99_jukebox").AddComponent<Jukebox>();
            Plugin.log.Info("Jukebox init");
        }

        void Update()
        {
            if (Client.Status == ClientStatus.Playing)
            {
                var now = Time.time;

                if (nextLevel == null && 
                    SongQueue.Count > 0 &&
                    !songPreloaded &&
                    now > songStart + (songDuration / 2))
                {
                    Plugin.log.Info("Starting preloading song.");
                    PreloadSong();
                }

                if (now > songStart + songDuration)
                {
                    NextSong();
                }
            }
        }

        public void Start()
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }

        public void TrackSong(float duration)
        {
            songStart = Time.time;
            songDuration = duration - 0.2f;

            Plugin.log.Info($"Jukebox tracking duration: {duration}, time now: {songStart}");
        }

        private void PreloadSong()
        {
            if (SongQueue.TryDequeue(out var song))
            {
                Plugin.log.Info($"Preloading song {song.LevelID}");

                songPreloaded = true;

                var characteristic = LevelLoader.Characteristics.First(c => c.serializedName == song.Characteristic);
                var level = LevelLoader.AllLevels.First(l => l.levelID == song.LevelID);
                var gameplay = GameplayModifiers.defaultModifiers;


                LevelLoader.PreloadBeatmapLevelAsync(
                    characteristic,
                    level,
                    song.Difficulty,
                    gameplay,
                    (preloadedLevel) =>
                    {
                        nextLevel = preloadedLevel;

                        nextLevel.speed = (float)song.Speed;
                        Plugin.log.Info($"Song {song.LevelID} has been preloaded!");
                    }
                );
            }
        }

        private void NextSong()
        {
            if (songPreloaded)
            {
                Plugin.log.Info("Loading next level");
                if (nextLevel != null)
                {
                    LevelLoader.SwitchLevel(nextLevel);
                    TrackSong(nextLevel.levelResult.beatmapLevel.songDuration / nextLevel.speed);
                    songPreloaded = false;
                    nextLevel = null;
                    Plugin.log.Info("Level switched.");
                }
            }
            else
            {
                // No more songs in the queue. Leave?
                Plugin.log.Info("Queue empty. Leaving session.");
                Client.Disconnect();
            }
        }
    }
}