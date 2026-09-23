using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Peaceland
{
    /// <summary>
    /// One JSON file per save slot: notebook + stats + progress + checkpoints.
    /// Lives on PeacelandGameBootstrap (DontDestroyOnLoad).
    /// NOTE: Save() needs an active slot. SaveLoad / GameStart selects one;
    /// playtests without a slot call EnsureActiveSlotBound() which prefers slot 0.
    /// </summary>
    public sealed class PeacelandSaveService : MonoBehaviour
    {
        public const string LegacyDefaultFileName = "peaceland_save.json";

        private static PeacelandSaveService instance;

        [SerializeField] private bool autoSaveOnChange = true;
        [Tooltip("0 keeps every checkpoint. Positive values keep only the newest entries.")]
        [Min(0)]
        [SerializeField] private int maxCheckpointHistory;

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
        /// <summary>True after the player (or EnsureActiveSlotBound) has selected slot 0+.</summary>
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
                if (string.IsNullOrWhiteSpace(json))
                {
                    summary = new PeacelandSaveSlotSummary
                    {
                        slotIndex = slotIndex,
                        hasData = false,
                    };
                    return true;
                }

                string trimmed = json.Trim();
                if (trimmed[0] != '{' || trimmed[trimmed.Length - 1] != '}')
                {
                    summary = new PeacelandSaveSlotSummary
                    {
                        slotIndex = slotIndex,
                        hasData = false,
                        isCorrupt = true,
                    };
                    return false;
                }

                PeacelandGameSaveData loaded = JsonUtility.FromJson<PeacelandGameSaveData>(json);
                if (loaded == null)
                {
                    summary = new PeacelandSaveSlotSummary
                    {
                        slotIndex = slotIndex,
                        hasData = false,
                        isCorrupt = true,
                    };
                    return false;
                }

                if (!loaded.IsOccupied())
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
                summary = new PeacelandSaveSlotSummary
                {
                    slotIndex = slotIndex,
                    hasData = false,
                    isCorrupt = true,
                };
                return false;
            }
        }

        public static int GetVisibleSlotCount(
            int minimum = PeacelandSaveSlots.InitialVisibleSlotCount,
            int emptySlotBuffer = 1)
        {
            int highestIndex = -1;
            string searchPattern =
                PeacelandSaveSlots.SlotFilePrefix + "*" + PeacelandSaveSlots.SlotFileSuffix;

            foreach (string path in Directory.GetFiles(Application.persistentDataPath, searchPattern))
            {
                if (PeacelandSaveSlots.TryParseSlotIndex(
                    Path.GetFileName(path),
                    out int slotIndex))
                {
                    highestIndex = Mathf.Max(highestIndex, slotIndex);
                }
            }

            return Mathf.Max(
                Mathf.Max(1, minimum),
                highestIndex + 1 + Mathf.Max(1, emptySlotBuffer));
        }

        public bool TryGetCheckpointSummaries(
            int slotIndex,
            out List<PeacelandCheckpointSummary> summaries)
        {
            summaries = new List<PeacelandCheckpointSummary>();
            if (!TryReadSlotData(slotIndex, out PeacelandGameSaveData loaded))
            {
                return false;
            }

            if (loaded.checkpoints == null)
            {
                return true;
            }

            for (int i = loaded.checkpoints.Count - 1; i >= 0; i--)
            {
                PeacelandCheckpointSnapshot checkpoint = loaded.checkpoints[i];
                if (checkpoint == null)
                {
                    continue;
                }

                summaries.Add(new PeacelandCheckpointSummary
                {
                    slotIndex = slotIndex,
                    checkpointId = checkpoint.checkpointId,
                    checkpointKey = checkpoint.checkpointKey,
                    displayName = checkpoint.displayName,
                    sceneName = checkpoint.sceneName,
                    savedUtc = checkpoint.savedUtc,
                    isActive = checkpoint.checkpointId == loaded.activeCheckpointId,
                });
            }

            return true;
        }

        public string CaptureCheckpoint(
            string checkpointKey,
            string displayName,
            string sceneName,
            bool replaceMatchingKey = false)
        {
            if (!HasActiveSlot || string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            NormalizeData();
            data.progress.lastSceneName = sceneName;
            data.checkpoints ??= new List<PeacelandCheckpointSnapshot>();

            if (replaceMatchingKey && !string.IsNullOrWhiteSpace(checkpointKey))
            {
                data.checkpoints.RemoveAll(
                    checkpoint => checkpoint != null
                        && checkpoint.checkpointKey == checkpointKey);
            }

            string savedUtc = DateTime.UtcNow.ToString("o");
            PeacelandCheckpointSnapshot snapshot = new PeacelandCheckpointSnapshot
            {
                checkpointId = Guid.NewGuid().ToString("N"),
                checkpointKey = string.IsNullOrWhiteSpace(checkpointKey)
                    ? sceneName
                    : checkpointKey,
                displayName = string.IsNullOrWhiteSpace(displayName)
                    ? sceneName
                    : displayName,
                sceneName = sceneName,
                savedUtc = savedUtc,
                stats = Clone(data.stats),
                progress = Clone(data.progress),
                notebook = Clone(data.notebook),
            };

            data.checkpoints.Add(snapshot);
            data.activeCheckpointId = snapshot.checkpointId;
            TrimCheckpointHistory();
            Save();
            return snapshot.checkpointId;
        }

        public bool ActivateSlotAtCheckpoint(int slotIndex, string checkpointId)
        {
            if (string.IsNullOrWhiteSpace(checkpointId)
                || !TryReadSlotData(slotIndex, out PeacelandGameSaveData loaded)
                || loaded.checkpoints == null)
            {
                return false;
            }

            PeacelandCheckpointSnapshot selected =
                loaded.checkpoints.Find(
                    checkpoint => checkpoint != null
                        && checkpoint.checkpointId == checkpointId);
            if (selected == null)
            {
                return false;
            }

            SetActiveSlot(slotIndex);
            data = loaded;
            NormalizeData();
            data.stats = Clone(selected.stats) ?? new PeacelandStatsSnapshot();
            data.progress = Clone(selected.progress) ?? new PeacelandProgressSnapshot();
            data.notebook = Clone(selected.notebook)
                ?? new Peaceland.Notebook.NotebookSaveData();
            data.progress.lastSceneName = selected.sceneName;
            data.activeCheckpointId = selected.checkpointId;
            Save();
            DataLoaded?.Invoke();
            return true;
        }

        /// <summary>Bind runtime save I/O to a slot and load existing data if present.</summary>
        public bool ActivateSlotAndContinue(int slotIndex)
        {
            if (!PeacelandSaveSlots.IsValidSlotIndex(slotIndex))
            {
                return false;
            }

            if (TryGetSlotSummary(slotIndex, out PeacelandSaveSlotSummary summary) && summary.isCorrupt)
            {
                Debug.LogError(
                    "PeacelandSaveService: refusing to continue corrupt slot "
                    + slotIndex + " without an explicit wipe.");
                return false;
            }

            SetActiveSlot(slotIndex);
            bool loaded = LoadFromPath(GetSlotPath(slotIndex), silentIfMissing: true);
            if (!loaded || !data.IsOccupied())
            {
                Debug.LogWarning("Slot " + slotIndex + " has no save data to continue.");
                ClearActiveSlotSelection();
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

        /// <summary>Adds delta then clamps to -5..+5. Same path as PeacelandStatManager.AddDelta.</summary>
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

        public bool TryGetProgressInt(string key, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(key)
                || data.progress == null
                || data.progress.intValues == null)
            {
                return false;
            }

            PeacelandIntProgressValue entry =
                data.progress.intValues.Find(item => item != null && item.key == key);
            if (entry == null)
            {
                return false;
            }

            value = entry.value;
            return true;
        }

        public void SetProgressInt(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            NormalizeData();
            PeacelandIntProgressValue entry =
                data.progress.intValues.Find(item => item != null && item.key == key);
            if (entry == null)
            {
                entry = new PeacelandIntProgressValue { key = key };
                data.progress.intValues.Add(entry);
            }

            entry.value = value;
            OnDataChanged();
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

        /// <summary>
        /// Binds runtime I/O to a slot when none is active so Notebook/stat playtests
        /// actually persist. Prefers continuing slot 0 when it already has data.
        /// </summary>
        public void EnsureActiveSlotBound(bool preferExistingSlotZero = true)
        {
            if (HasActiveSlot)
            {
                return;
            }

            const int defaultSlot = 0;
            if (preferExistingSlotZero
                && TryGetSlotSummary(defaultSlot, out PeacelandSaveSlotSummary summary)
                && summary.hasData
                && !summary.isCorrupt)
            {
                ActivateSlotAndContinue(defaultSlot);
                return;
            }

            if (preferExistingSlotZero
                && TryGetSlotSummary(defaultSlot, out summary)
                && summary.isCorrupt)
            {
                Debug.LogWarning(
                    "PeacelandSaveService: default slot 0 is corrupt; leaving no active slot until the player chooses.");
                return;
            }

            ActivateSlotAndStartNewGame(defaultSlot);
        }

        /// <summary>
        /// Writes the current document to peaceland_save_slot_{n}.json.
        /// If no slot is selected, binds slot 0 (unless that file is corrupt).
        /// </summary>
        public void Save()
        {
            if (!HasActiveSlot)
            {
                EnsureActiveSlotBound(preferExistingSlotZero: true);
            }

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
                EnsureActiveSlotBound(preferExistingSlotZero: true);
            }

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
                data = new PeacelandGameSaveData();
                DataLoaded?.Invoke();
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
            PeacelandCheckpointSnapshot latestCheckpoint =
                loaded.checkpoints != null && loaded.checkpoints.Count > 0
                    ? loaded.checkpoints[loaded.checkpoints.Count - 1]
                    : null;

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
                checkpointCount = loaded.checkpoints != null ? loaded.checkpoints.Count : 0,
                latestCheckpointName = latestCheckpoint != null
                    ? latestCheckpoint.displayName
                    : string.Empty,
            };
        }

        private bool TryReadSlotData(int slotIndex, out PeacelandGameSaveData loaded)
        {
            loaded = null;
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
                loaded = JsonUtility.FromJson<PeacelandGameSaveData>(File.ReadAllText(path));
                if (loaded != null && loaded.checkpoints == null)
                {
                    loaded.checkpoints = new List<PeacelandCheckpointSnapshot>();
                }

                EnsureLegacyCheckpoint(loaded);

                return loaded != null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "Failed to read save slot " + slotIndex + ": " + exception.Message);
                return false;
            }
        }

        private void TrimCheckpointHistory()
        {
            if (maxCheckpointHistory <= 0 || data.checkpoints == null)
            {
                return;
            }

            while (data.checkpoints.Count > maxCheckpointHistory)
            {
                data.checkpoints.RemoveAt(0);
            }
        }

        private static T Clone<T>(T source) where T : class
        {
            return source == null
                ? null
                : JsonUtility.FromJson<T>(JsonUtility.ToJson(source));
        }

        private static void EnsureLegacyCheckpoint(PeacelandGameSaveData loaded)
        {
            if (loaded == null
                || loaded.version >= PeacelandGameSaveData.CurrentVersion
                || loaded.checkpoints == null
                || loaded.checkpoints.Count > 0
                || loaded.progress == null
                || string.IsNullOrWhiteSpace(loaded.progress.lastSceneName))
            {
                return;
            }

            const string legacyCheckpointId = "legacy-latest";
            loaded.checkpoints.Add(new PeacelandCheckpointSnapshot
            {
                checkpointId = legacyCheckpointId,
                checkpointKey = "legacy/latest",
                displayName = loaded.progress.lastSceneName,
                sceneName = loaded.progress.lastSceneName,
                savedUtc = loaded.savedUtc,
                stats = Clone(loaded.stats),
                progress = Clone(loaded.progress),
                notebook = Clone(loaded.notebook),
            });
            loaded.activeCheckpointId = legacyCheckpointId;
        }

        private void NormalizeData()
        {
            data ??= new PeacelandGameSaveData();
            data.version = PeacelandGameSaveData.CurrentVersion;

            if (data.stats == null)
            {
                data.stats = new PeacelandStatsSnapshot();
            }

            if (data.progress == null)
            {
                data.progress = new PeacelandProgressSnapshot();
            }

            data.progress.currentDay = Mathf.Max(1, data.progress.currentDay);
            data.progress.intValues ??= new List<PeacelandIntProgressValue>();

            if (data.notebook == null)
            {
                data.notebook = new Peaceland.Notebook.NotebookSaveData();
            }

            data.checkpoints ??= new List<PeacelandCheckpointSnapshot>();
            EnsureLegacyCheckpoint(data);
        }

        private void OnDataChanged()
        {
            if (!HasActiveSlot)
            {
                EnsureActiveSlotBound(preferExistingSlotZero: true);
            }

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
