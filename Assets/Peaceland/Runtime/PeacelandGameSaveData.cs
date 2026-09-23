using System;
using System.Collections.Generic;

namespace Peaceland
{
    [Serializable]
    public sealed class PeacelandCheckpointSnapshot
    {
        public string checkpointId;
        public string checkpointKey;
        public string displayName;
        public string sceneName;
        public string savedUtc;
        public PeacelandStatsSnapshot stats = new PeacelandStatsSnapshot();
        public PeacelandProgressSnapshot progress = new PeacelandProgressSnapshot();
        public Peaceland.Notebook.NotebookSaveData notebook =
            new Peaceland.Notebook.NotebookSaveData();
    }

    [Serializable]
    public sealed class PeacelandStatsSnapshot
    {
        public int selfishAltruistic;
        public int insightNaivety;
        public int nationalismRebellion;
        public int kindnessCruelty;

        public int Get(PeacelandStatId statId)
        {
            switch (statId)
            {
                case PeacelandStatId.SelfishAltruistic:
                    return selfishAltruistic;
                case PeacelandStatId.InsightNaivety:
                    return insightNaivety;
                case PeacelandStatId.NationalismRebellion:
                    return nationalismRebellion;
                case PeacelandStatId.KindnessCruelty:
                    return kindnessCruelty;
                default:
                    return 0;
            }
        }

        public void Set(PeacelandStatId statId, int value)
        {
            int clamped = Clamp(value);
            switch (statId)
            {
                case PeacelandStatId.SelfishAltruistic:
                    selfishAltruistic = clamped;
                    break;
                case PeacelandStatId.InsightNaivety:
                    insightNaivety = clamped;
                    break;
                case PeacelandStatId.NationalismRebellion:
                    nationalismRebellion = clamped;
                    break;
                case PeacelandStatId.KindnessCruelty:
                    kindnessCruelty = clamped;
                    break;
            }
        }

        /// <summary>Hidden stats are always stored in -5..+5.</summary>
        public static int Clamp(int value)
        {
            return Math.Max(-5, Math.Min(5, value));
        }
    }

    [Serializable]
    public sealed class PeacelandIntProgressValue
    {
        public string key;
        public int value;
    }

    [Serializable]
    public sealed class PeacelandProgressSnapshot
    {
        public List<string> trueFlags = new List<string>();
        public List<PeacelandIntProgressValue> intValues =
            new List<PeacelandIntProgressValue>();
        public string lastSceneName = string.Empty;
        public string displayLocationName = string.Empty;
        public int currentDay = 1;
    }

    /// <summary>
    /// One save document. version 3 includes checkpoints and occupied.
    /// </summary>
    [Serializable]
    public sealed class PeacelandGameSaveData
    {
        public const int CurrentVersion = 3;

        public int version = CurrentVersion;
        public string savedUtc;
        public bool occupied;
        public PeacelandStatsSnapshot stats = new PeacelandStatsSnapshot();
        public PeacelandProgressSnapshot progress = new PeacelandProgressSnapshot();
        public Peaceland.Notebook.NotebookSaveData notebook = new Peaceland.Notebook.NotebookSaveData();
        public List<PeacelandCheckpointSnapshot> checkpoints =
            new List<PeacelandCheckpointSnapshot>();
        public string activeCheckpointId;

        public static PeacelandGameSaveData CreateNewGame()
        {
            PeacelandGameSaveData fresh = new PeacelandGameSaveData();
            fresh.MarkOccupied();
            return fresh;
        }

        public void MarkOccupied()
        {
            occupied = true;
        }

        public bool IsOccupied()
        {
            if (occupied)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(savedUtc))
            {
                return true;
            }

            if (progress != null && progress.trueFlags != null && progress.trueFlags.Count > 0)
            {
                return true;
            }

            if (notebook != null && notebook.states != null && notebook.states.Count > 0)
            {
                return true;
            }

            if (stats != null)
            {
                if (stats.selfishAltruistic != 0
                    || stats.insightNaivety != 0
                    || stats.nationalismRebellion != 0
                    || stats.kindnessCruelty != 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
