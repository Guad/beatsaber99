using System;
using System.Collections;
using System.Linq;
using BeatSaber99Client.Session;
using BS_Utils.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BeatSaber99Client.Items
{
    /// <summary>
    ///  Manages the in-game note spawning and altering the current level.
    /// </summary>
    public class BeatmapSpawnManager : MonoBehaviour
    {
        public static BeatmapSpawnManager instance;

        public static void Init()
        {
            if (instance == null)
                instance = new GameObject("beatsaber99_spawnmanager").AddComponent<BeatmapSpawnManager>();
        }

        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }


        public void ReplaceNextXNotesWith(float duration, Action<NoteData> callback)
        {
            BeatmapObjectCallbackController callbackController = Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().First();
            BeatmapData beatmapData = callbackController.GetField<BeatmapData>("_beatmapData");
            BeatmapObjectData[] objects;
            NoteData note;

            var spawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().FirstOrDefault();
            if (spawnController == null)
                Plugin.log.Info("Spawn manager was null!");

            var movementData =
                spawnController.GetField<BeatmapObjectSpawnMovementData>("_beatmapObjectSpawnMovementData");

            
            float start = (Time.time - Jukebox.instance.songStart) + movementData.spawnAheadTime + 0.1f;
            float end = start + duration + 2f;

            foreach (BeatmapLineData line in beatmapData.beatmapLinesData)
            {
                objects = line.beatmapObjectsData;
                foreach (BeatmapObjectData beatmapObject in objects)
                {
                    if (beatmapObject.beatmapObjectType == BeatmapObjectType.Note)
                        if (beatmapObject.time > start && beatmapObject.time < end)
                        {
                            note = beatmapObject as NoteData;
                            callback?.Invoke(note);
                        }

                }
            }
        }

        public void ReplaceNextXNotesWithAnyDirection(float duration)
        {
            ReplaceNextXNotesWith(duration, data =>
            {
                data.SetNoteToAnyCutDirection();
            });
        }

        public void ReplaceNextXNotesWithEachother(float duration)
        {
            ReplaceNextXNotesWith(duration, data =>
            {
                data.SwitchNoteType();
            });
        }

        public void ReplaceNextXNotesWithRandomBombs(float duration, float chance)
        {
            ReplaceNextXNotesWith(duration, data =>
            {
                float r = Random.Range(0f, 1f);

                if (r < chance)
                {
                    data.SetProperty("noteType", NoteType.Bomb);
                }
            });
        }

        public IEnumerator ReplaceNextXNotesWithGhostNotesCoroutine(float duration)
        {
            var spawncontroller = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();

            spawncontroller.SetPrivateField("_ghostNotes", true);

            yield return new WaitForSeconds(duration);

            spawncontroller.SetPrivateField("_ghostNotes", false);
        }

        public IEnumerator ReplaceNextXNotesWithDisappearingArrowsCoroutine(float duration)
        {
            var spawncontroller = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();

            spawncontroller.SetPrivateField("_disappearingArrows", true);

            yield return new WaitForSeconds(duration);

            spawncontroller.SetPrivateField("_disappearingArrows", false);
        }

    }
}