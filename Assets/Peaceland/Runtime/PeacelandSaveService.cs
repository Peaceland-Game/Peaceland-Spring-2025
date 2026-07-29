using System;
using System.IO;
using UnityEngine;

namespace Peaceland
{
    public sealed class PeacelandSaveService : MonoBehaviour
    {
        public const string LegacyDefaultFileName = "peaceland_save.json";

        private static PeacelandSaveService instance;

        [SerializeField] private bool autoSaveOnChange = true;

        private PeacelandGameSaveData data = new PeacelandGameSaveData();
        private int activeSlotIndex = -1;
        private bool legacyMigrationAttempted;

        public static PeacelandSaveService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PeacelandSaveService>();
                    if (instance != null)
                    {
                        instance.InitializeAsHost();
                    }
                }

                if (instance == null)
                {
                    PeacelandGameBootstrap.EnsureExists();
                    instance = FindFirstObjectByType<PeacelandSaveService>();
                    if (instance != null)
                    {
                        instance.InitializeAsHost();
                    }
                }

                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        public int ActiveSlotIndex => activeSlotIndex;
        public bool HasActiveSlot => PeacelandSaveSlots.IsValidSlotIndex(activeSlotIndex);

        public event Action DataLoaded;
        public event Action DataSaved;
        public event Action<int> ActiveSlotChanged;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            RestoreActiveSlotFromPrefs();
            LoadActiveSlotIfAny();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        internal void InitializeAsHost()
        {
            if (instance == null)
            {
                instance = this;
            }

            RestoreActiveSlotFromPrefs();
            LoadActiveSlotIfAny();
        }

        public PeacelandGameSaveData GetSnapshot()
        {
            return JsonUtility.FromJson<PeacelandGameSaveData>(JsonUtility.ToJson(data));
        }

        public void ApplySnapshot(PeacelandGameSaveData snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            data = snapshot;
            if (autoSaveOnChange && HasActiveSlot)
            {
                Save();
            }

            DataLoaded?.Invoke();
        }

        /// <summary>Path for the currently active slot, or legacy default when no slot selected.</summary>
        public string GetDefaultSavePath()
        {
            if (HasActiveSlot)
            {
                return GetSlotPath(activeSlotIndex);
            }

            return Path.Combine(Application.persistentDataPath, LegacyDefaultFileName);
        }

        public static string GetSlotPath(int slotIndex)
        {
            if (!PeacelandSaveSlots.IsValidSlotIndex(slotIndex))
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex));
            }

            return Path.Combine(Application.persistentDataPath, PeacelandSaveSlots.GetSlotFileName(slotIndex));
        }

        public bool TryGetSlotSummary(int slotIndex, out PeacelandSaveSlotSummary summary)
        {
            summary = default;
            if (!PeacelandSaveSlots.IsValidSlotIndex(slotIndex))
            {
                return false;
            }

            EnsureLegacySaveMigratedToSlotZero();

            string path = GetSlotPath(slotIndex);
            if (!File.Exists(path))
            {
                summary = new PeacelandSaveSlotSummary
                {
                    slotIndex = slotIndex,
                    hasData = false,
                };
                return true;
            }

            try
            {
                string json = File.ReadAllText(path);
                PeacelandGameSaveData loaded = JsonUtility.FromJson<PeacelandGameSaveData>(json);
                if (loaded == null || !loaded.IsOccupied())
                {
                    summary = new PeacelandSaveSlotSummary
                    {
                        slotIndex = slotIndex,
                        hasData = false,
                    };
                    return true;
                }

                summary = BuildSummary(slotIndex, loaded);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Failed to read save slot " + slotIndex + ": " + exception.Message);
                summary = new PeacelandSaveSlotSummary { slotIndex = slotIndex, hasData = false };
                return false;
            }
        }

        /// <summary>Bind runtime save I/O to a slot and load existing data if present.</summary>
        public bool ActivateSlotAndContinue(int slotIndex)
        {
            if (!PeacelandSaveSlots.IsValidSlotIndex(slotIndex))
            {
                return false;
            }

            SetActiveSlot(slotIndex);
            bool loaded = LoadFromPath(GetSlotPath(slotIndex), silentIfMissing: true);
            if (!loaded || !data.IsOccupied())
            {
                Debug.LogWarning("Slot " + slotIndex + " has no save data to continue.");
                return false;
            }

            return true;
        }

        /// <summary>Bind runtime save I/O to a slot and write a fresh game document.</summary>
        public void ActivateSlotAndStartNewGame(int slotIndex)
        {
            if (!PeacelandSaveSlots.IsValidSlotIndex(slotIndex))
            {
                return;
            }

            SetActiveSlot(slotIndex);
            data = PeacelandGameSaveData.CreateNewGame();
            Save();
            DataLoaded?.Invoke();
        }

        public void ClearActiveSlotSelection()
        {
            activeSlotIndex = -1;
            PlayerPrefs.SetInt(PeacelandSaveSlots.ActiveSlotPlayerPrefsKey, -1);
            PlayerPrefs.Save();
            data = new PeacelandGameSaveData();
            DataLoaded?.Invoke();
            ActiveSlotChanged?.Invoke(-1);
        }

        public bool DeleteSlot(int slotIndex)
        {
            if (!PeacelandSaveSlots.IsValidSlotIndex(slotIndex))
            {
                return false;
            }

            string path = GetSlotPath(slotIndex);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (activeSlotIndex == slotIndex)
            {
                data = new PeacelandGameSaveData();
                DataLoaded?.Invoke();
            }

            return true;
        }

        /// <summary>Moves one slot file into an empty slot without overwriting data.</summary>
        public bool MoveSlot(int sourceSlotIndex, int destinationSlotIndex)
        {
            if (!PeacelandSaveSlots.IsValidSlotIndex(sourceSlotIndex)
                || !PeacelandSaveSlots.IsValidSlotIndex(destinationSlotIndex)
                || sourceSlotIndex == destinationSlotIndex)
            {
                return false;
            }

            string sourcePath = GetSlotPath(sourceSlotIndex);
            string destinationPath = GetSlotPath(destinationSlotIndex);
            if (!File.Exists(sourcePath) || File.Exists(destinationPath))
            {
                return false;
            }

            File.Move(sourcePath, destinationPath);
            if (activeSlotIndex == sourceSlotIndex)
            {
                SetActiveSlot(destinationSlotIndex);
            }

            return true;
        }

        public Peaceland.Notebook.NotebookSaveData GetNotebookData()
        {
            if (data.notebook == null)
            {
                data.notebook = new Peaceland.Notebook.NotebookSaveData();
            }

            return data.notebook;
        }

        /// <summary>Returns a detached notebook snapshot from a slot without activating it.</summary>
        public bool TryGetNotebookSnapshot(int slotIndex, out Peaceland.Notebook.NotebookSaveData notebook)
        {
            notebook = new Peaceland.Notebook.NotebookSaveData();
            if (!PeacelandSaveSlots.IsValidSlotIndex(slotIndex))
            {
                return false;
            }

            string path = GetSlotPath(slotIndex);
            if (!File.Exists(path))
            {
                return true;
            }

            try
            {
                PeacelandGameSaveData loaded = JsonUtility.FromJson<PeacelandGameSaveData>(File.ReadAllText(path));
                if (loaded == null || loaded.notebook == null)
                {
                    return true;
                }

                notebook = CloneNotebook(loaded.notebook);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Failed to read notebook from save slot " + slotIndex + ": " + exception.Message);
                return false;
            }
        }

        /// <summary>Replaces and persists the notebook owned by the active slot.</summary>
        public bool SaveNotebookToActiveSlot(Peaceland.Notebook.NotebookSaveData notebookData)
        {
            if (!HasActiveSlot)
            {
                Debug.LogWarning("Cannot save notebook because no save slot is active.");
                return false;
            }

            data.notebook = CloneNotebook(notebookData);
            Save();
            DataLoaded?.Invoke();
            return true;
        }

        public int GetStat(PeacelandStatId statId)
        {
            return data.stats.Get(statId);
        }

        public void SetStat(PeacelandStatId statId, int value)
        {
            data.stats.Set(statId, value);
            OnDataChanged();
        }

        public void AddStat(PeacelandStatId statId, int delta)
        {
            SetStat(statId, data.stats.Get(statId) + delta);
        }

        public bool HasProgressFlag(string flagId)
        {
            if (string.IsNullOrWhiteSpace(flagId) || data.progress == null)
            {
                return false;
            }

            return data.progress.trueFlags.Contains(flagId);
        }

        public void SetProgressFlag(string flagId, bool value)
        {
            if (string.IsNullOrWhiteSpace(flagId))
            {
                return;
            }

            if (data.progress == null)
            {
                data.progress = new PeacelandProgressSnapshot();
            }

            if (value)
            {
                if (!data.progress.trueFlags.Contains(flagId))
                {
                    data.progress.trueFlags.Add(flagId);
                }
            }
            else
            {
                data.progress.trueFlags.Remove(flagId);
            }

            OnDataChanged();
        }

        public string GetLastSceneName()
        {
            return data.progress != null ? data.progress.lastSceneName : string.Empty;
        }

        public void SetLastSceneName(string sceneName)
        {
            if (data.progress == null)
            {
                data.progress = new PeacelandProgressSnapshot();
            }

            data.progress.lastSceneName = sceneName ?? string.Empty;
            OnDataChanged();
        }

        public void SetSaveDisplayProgress(string locationName, int day)
        {
            if (data.progress == null)
            {
                data.progress = new PeacelandProgressSnapshot();
            }

            data.progress.displayLocationName = locationName ?? string.Empty;
            data.progress.currentDay = Mathf.Max(1, day);
            OnDataChanged();
        }

        public void Save()
        {
            if (!HasActiveSlot)
            {
                Debug.LogWarning("PeacelandSaveService.Save skipped - no active save slot selected.");
                return;
            }

            data.MarkOccupied();
            data.version = PeacelandGameSaveData.CurrentVersion;
            data.savedUtc = DateTime.UtcNow.ToString("o");
            WriteJson(GetSlotPath(activeSlotIndex), data);
            DataSaved?.Invoke();
        }

        public void Load()
        {
            if (!HasActiveSlot)
            {
                data = new PeacelandGameSaveData();
                DataLoaded?.Invoke();
                return;
            }

            LoadFromPath(GetSlotPath(activeSlotIndex), silentIfMissing: true);
        }

        public bool SaveToPath(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return false;
            }

            data.MarkOccupied();
            data.version = PeacelandGameSaveData.CurrentVersion;
            data.savedUtc = DateTime.UtcNow.ToString("o");
            WriteJson(absolutePath, data);
            DataSaved?.Invoke();
            return true;
        }

        public bool LoadFromPath(string absolutePath, bool silentIfMissing = false)
        {
            if (string.IsNullOrWhiteSpace(absolutePath) || !File.Exists(absolutePath))
            {
                if (!silentIfMissing)
                {
                    Debug.LogWarning("Peaceland save not found at " + absolutePath);
                }

                data = new PeacelandGameSaveData();
                DataLoaded?.Invoke();
                return false;
            }

            try
            {
                string json = File.ReadAllText(absolutePath);
                PeacelandGameSaveData loaded = JsonUtility.FromJson<PeacelandGameSaveData>(json);
                data = loaded ?? new PeacelandGameSaveData();
                NormalizeData();
                DataLoaded?.Invoke();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError("Failed to load Peaceland save: " + exception.Message);
                return false;
            }
        }

        public void ResetAll()
        {
            if (!HasActiveSlot)
            {
                data = new PeacelandGameSaveData();
                DataLoaded?.Invoke();
                return;
            }

            data = PeacelandGameSaveData.CreateNewGame();
            Save();
            DataLoaded?.Invoke();
        }

        public void ClearNotebookSection()
        {
            data.notebook = new Peaceland.Notebook.NotebookSaveData();
            Save();
            DataLoaded?.Invoke();
        }

        public void ReplaceNotebookData(Peaceland.Notebook.NotebookSaveData notebookData)
        {
            data.notebook = notebookData ?? new Peaceland.Notebook.NotebookSaveData();
        }

        private void SetActiveSlot(int slotIndex)
        {
            activeSlotIndex = slotIndex;
            PlayerPrefs.SetInt(PeacelandSaveSlots.ActiveSlotPlayerPrefsKey, slotIndex);
            PlayerPrefs.Save();
            ActiveSlotChanged?.Invoke(slotIndex);
        }

        private void RestoreActiveSlotFromPrefs()
        {
            activeSlotIndex = PlayerPrefs.GetInt(PeacelandSaveSlots.ActiveSlotPlayerPrefsKey, -1);
            if (!PeacelandSaveSlots.IsValidSlotIndex(activeSlotIndex))
            {
                activeSlotIndex = -1;
            }
        }

        private void LoadActiveSlotIfAny()
        {
            if (!HasActiveSlot)
            {
                data = new PeacelandGameSaveData();
                return;
            }

            LoadFromPath(GetSlotPath(activeSlotIndex), silentIfMissing: true);
        }

        private void EnsureLegacySaveMigratedToSlotZero()
        {
            if (legacyMigrationAttempted)
            {
                return;
            }

            legacyMigrationAttempted = true;

            string slotZeroPath = GetSlotPath(0);
            if (File.Exists(slotZeroPath))
            {
                return;
            }

            string legacyPath = Path.Combine(Application.persistentDataPath, LegacyDefaultFileName);
            if (!File.Exists(legacyPath))
            {
                return;
            }

            try
            {
                File.Copy(legacyPath, slotZeroPath);
                Debug.Log("Migrated legacy save to slot 0: " + slotZeroPath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Legacy save migration failed: " + exception.Message);
            }
        }

        private static PeacelandSaveSlotSummary BuildSummary(int slotIndex, PeacelandGameSaveData loaded)
        {
            int notebookCount = loaded.notebook != null && loaded.notebook.states != null
                ? loaded.notebook.states.Count
                : 0;

            return new PeacelandSaveSlotSummary
            {
                slotIndex = slotIndex,
                hasData = loaded.IsOccupied(),
                savedUtc = loaded.savedUtc,
                lastSceneName = loaded.progress != null ? loaded.progress.lastSceneName : string.Empty,
                displayLocationName = loaded.progress != null ? loaded.progress.displayLocationName : string.Empty,
                currentDay = loaded.progress != null ? Mathf.Max(1, loaded.progress.currentDay) : 1,
                kindnessCruelty = loaded.stats != null ? loaded.stats.kindnessCruelty : 0,
                collectedNotebookEntryCount = notebookCount,
            };
        }

        private void NormalizeData()
        {
            if (data.stats == null)
            {
                data.stats = new PeacelandStatsSnapshot();
            }

            if (data.progress == null)
            {
                data.progress = new PeacelandProgressSnapshot();
            }

            data.progress.currentDay = Mathf.Max(1, data.progress.currentDay);

            if (data.notebook == null)
            {
                data.notebook = new Peaceland.Notebook.NotebookSaveData();
            }
        }

        private void OnDataChanged()
        {
            if (!HasActiveSlot)
            {
                return;
            }

            if (autoSaveOnChange)
            {
                Save();
            }
        }

        private static void WriteJson(string absolutePath, PeacelandGameSaveData payload)
        {
            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(payload, prettyPrint: true);
            File.WriteAllText(absolutePath, json);
        }

        private static Peaceland.Notebook.NotebookSaveData CloneNotebook(
            Peaceland.Notebook.NotebookSaveData notebookData)
        {
            if (notebookData == null)
            {
                return new Peaceland.Notebook.NotebookSaveData();
            }

            string json = JsonUtility.ToJson(notebookData);
            return JsonUtility.FromJson<Peaceland.Notebook.NotebookSaveData>(json)
                ?? new Peaceland.Notebook.NotebookSaveData();
        }
    }
}
