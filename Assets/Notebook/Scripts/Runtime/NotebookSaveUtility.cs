using System.Collections.Generic;
using System.Linq;
using Peaceland;
using UnityEngine;

namespace Peaceland.Notebook
{
    /// <summary>
    /// PlayerPrefs save helpers for notebook state when no NotebookController is active (cross-scene collect).
    /// </summary>
    public static class NotebookSaveUtility
    {
        public const string DefaultSaveKey = "peaceland.notebook.state";

        public static bool IsCollected(string entryId, string saveKey = DefaultSaveKey)
        {
            if (string.IsNullOrWhiteSpace(entryId))
            {
                return false;
            }

            NotebookEntryStateData state = FindState(entryId, saveKey);
            return state != null && state.isCollected;
        }

        public static bool TryMarkCollected(string entryId, string saveKey = DefaultSaveKey)
        {
            if (string.IsNullOrWhiteSpace(entryId))
            {
                return false;
            }

            NotebookSaveData saveData = Load(saveKey);
            NotebookEntryStateData state = saveData.states.FirstOrDefault(item => item != null && item.entryId == entryId);
            if (state == null)
            {
                state = new NotebookEntryStateData { entryId = entryId };
                saveData.states.Add(state);
            }

            if (state.isCollected)
            {
                return false;
            }

            state.isCollected = true;
            state.isReviewed = false;
            state.collectedOrder = GetNextCollectedOrder(saveData);
            Save(saveData, saveKey);
            return true;
        }

        public static int CountCollected(string saveKey = DefaultSaveKey)
        {
            NotebookSaveData saveData = Load(saveKey);
            int count = 0;
            for (int i = 0; i < saveData.states.Count; i++)
            {
                NotebookEntryStateData state = saveData.states[i];
                if (state != null && state.isCollected)
                {
                    count++;
                }
            }

            return count;
        }

        public static void MarkManyCollected(IEnumerable<string> entryIds, string saveKey = DefaultSaveKey)
        {
            if (entryIds == null)
            {
                return;
            }

            NotebookSaveData saveData = Load(saveKey);
            bool changed = false;
            foreach (string entryId in entryIds)
            {
                if (string.IsNullOrWhiteSpace(entryId))
                {
                    continue;
                }

                NotebookEntryStateData state = saveData.states.FirstOrDefault(item => item != null && item.entryId == entryId);
                if (state == null)
                {
                    state = new NotebookEntryStateData { entryId = entryId };
                    saveData.states.Add(state);
                }

                if (state.isCollected)
                {
                    continue;
                }

                state.isCollected = true;
                state.isReviewed = false;
                state.collectedOrder = GetNextCollectedOrder(saveData);
                changed = true;
            }

            if (changed)
            {
                Save(saveData, saveKey);
            }
        }

        private static NotebookEntryStateData FindState(string entryId, string saveKey)
        {
            NotebookSaveData saveData = Load(saveKey);
            return saveData.states.FirstOrDefault(item => item != null && item.entryId == entryId);
        }

        private static int GetNextCollectedOrder(NotebookSaveData saveData)
        {
            int max = 0;
            for (int i = 0; i < saveData.states.Count; i++)
            {
                NotebookEntryStateData state = saveData.states[i];
                if (state != null && state.collectedOrder > max)
                {
                    max = state.collectedOrder;
                }
            }

            return max + 1;
        }

        private static NotebookSaveData Load(string saveKey)
        {
            if (PeacelandSaveService.HasInstance)
            {
                return PeacelandSaveService.Instance.GetNotebookData();
            }

            if (!PlayerPrefs.HasKey(saveKey))
            {
                return new NotebookSaveData();
            }

            string json = PlayerPrefs.GetString(saveKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new NotebookSaveData();
            }

            NotebookSaveData saveData = JsonUtility.FromJson<NotebookSaveData>(json);
            return saveData ?? new NotebookSaveData();
        }

        private static void Save(NotebookSaveData saveData, string saveKey)
        {
            if (PeacelandSaveService.HasInstance)
            {
                PeacelandSaveService.Instance.Save();
                return;
            }

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }
    }
}
