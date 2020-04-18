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

        public float songStart;
        public float songDuration;

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
                var gameplay = GameplayModifiers.defaultModifiers;


                if (song.LevelID.StartsWith("bsaber.com/"))
                {
                    var split = song.LevelID.Split('/');
                    CustomSongInjector.StartSongDownload("https://beatsaver.com/api/download/key/" + split[1],
                        (level) =>
                        {
                            LevelLoader.PreloadBeatmapLevelAsync(
                                characteristic,
                                level,
                                song.Difficulty,
                                gameplay,
                                (preloadedLevel) =>
                                {
                                    if (preloadedLevel == null)
                                    {
                                        Plugin.log.Info("Level did not preload correctly..");
                                        return;
                                    }

                                    nextLevel = preloadedLevel;
                                    nextLevel.speed = (float)song.Speed;

                                    Plugin.log.Info($"Song {song.LevelID} has been preloaded!");
                                }
                            );
                        });
                }
                else
                {
                    var level = LevelLoader.AllLevels.First(l => l.levelID == song.LevelID);
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
        }

        private void NextSong()
        {
            if (songPreloaded)
            {
                Plugin.log.Info("Loading next level");
                if (nextLevel != null)
                {
                    float duration = nextLevel.levelResult.beatmapLevel.songDuration;

                    if (duration < 1f)
                    {
                        var c = nextLevel.levelResult.beatmapLevel.beatmapLevelData.audioClip;
                        duration = c.length;
                    }

                    Plugin.log.Info($"Next level song duration: {duration}");

                    TrackSong(duration / nextLevel.speed);
                    LevelLoader.SwitchLevel(nextLevel);
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