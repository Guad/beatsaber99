using System;
using System.Collections;
using System.Linq;
using BS_Utils.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BeatSaber99Client.Items
{
    public class BeatmapSpawnManager : MonoBehaviour
    {
        public static BeatmapSpawnManager instance;

        private BeatmapObjectManager _beatmapObjectManager;
        private BeatmapObjectSpawnController _spawnController;

        public static void Init()
        {
            // beatmapObjectManager = Resources.FindObjectsOfTypeAll<BeatmapObjectManager>().FirstOrDefault();
            // 

            if (instance == null)
                instance = new GameObject("beatsaber99_spawnmanager").AddComponent<BeatmapSpawnManager>();
        }

        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            _spawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().FirstOrDefault();

            // _beatmapObjectManager = Resources.FindObjectsOfTypeAll<BeatmapObjectManager>().FirstOrDefault();

            // _beatmapObjectManager.SpawnBasicNote();
            /*_beatmapObjectManager.noteWasSpawnedEvent += controller =>
            {
                // Item: switch note color
                controller.noteData.TransformNoteAOrBToRandomType();
                FloatBehavior behavior = controller.noteTransform?.gameObject.GetComponent<FloatBehavior>();
            };*/


            if (_spawnController == null)
                Plugin.log.Info("Spawn manager was null!");

            // StartCoroutine(WaitForLevelDataCoroutine());
        }

        /*
        IEnumerator WaitForLevelDataCoroutine()
        {
            BeatmapData beatmapData;
            do
            {
                BeatmapObjectCallbackController callbackController = Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().First();
                beatmapData = callbackController.GetField<BeatmapData>("_beatmapData");
                yield return new WaitForEndOfFrame();
            } while (beatmapData == null);

            Plugin.log.Info("Beatmap data is not null!");
        }
        */

        public void ReplaceNextXNotesWith(float duration, Action<NoteData> callback)
        {
            BeatmapObjectCallbackController callbackController = Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().First();
            BeatmapData beatmapData = callbackController.GetField<BeatmapData>("_beatmapData");
            BeatmapObjectData[] objects;
            NoteData note;

            Plugin.log.Info($"Replacing notes: {_spawnController}, beatmapData: {beatmapData}");
            var movementData =
                _spawnController.GetField<BeatmapObjectSpawnMovementData>("_beatmapObjectSpawnMovementData");

            Plugin.log.Info($"Mov data: {movementData}");

            Plugin.log.Info($"Replacing notes: {_spawnController}, {movementData}");

            float start = (Time.time - Jukebox.instance.songStart) + movementData.spawnAheadTime + 0.1f;
            float end = start + duration + 2f;

            foreach (BeatmapLineData line in beatmapData.beatmapLinesData)
            {
                Plugin.log.Info($"Beatmap line: {line}");
                objects = line.beatmapObjectsData;
                foreach (BeatmapObjectData beatmapObject in objects)
                {
                    Plugin.log.Info($"Beatmap object: {beatmapObject}");
                    if (beatmapObject.beatmapObjectType == BeatmapObjectType.Note)
                        if (beatmapObject.time > start && beatmapObject.time < end)
                        {
                            note = beatmapObject as NoteData;

                            // note.SetNoteToAnyCutDirection();
                            // note.TransformNoteAOrBToRandomType();

                            callback?.Invoke(note);
                        }

                }
            }
        }

        public void ReplaceNextXNotesWithAnyDirection(float duration)
        {
            ReplaceNextXNotesWith(duration, data =>
            {
                Plugin.log.Info($"AnyDirection note: {data}");
                Plugin.log.Info($"Setting note data for {data.id}");
                data.SetNoteToAnyCutDirection();
                data.TransformNoteAOrBToRandomType();
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