using UnityEngine;

namespace Peaceland
{
    /// <summary>
    /// Hidden stats API for any scene. Values live on the active save slot and clamp to -5..+5.
    /// KindnessCruelty is the primary good/evil axis.
    /// </summary>
    public sealed class PeacelandStatManager : MonoBehaviour
    {
        private static PeacelandStatManager instance;

        public static PeacelandStatManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PeacelandStatManager>();
                }

                if (instance == null)
                {
                    PeacelandGameBootstrap.EnsureExists();
                    instance = FindFirstObjectByType<PeacelandStatManager>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>Current clamped value for this axis. Creates the bootstrap host if needed.</summary>
        public int Get(PeacelandStatId statId)
        {
            return PeacelandSaveService.Instance.GetStat(statId);
        }

        public void Set(PeacelandStatId statId, int value)
        {
            PeacelandSaveService.Instance.SetStat(statId, value);
        }

        /// <summary>
        /// Adds delta and clamps. Use this from gameplay / Yarn, not Set, unless you are restoring a save.
        /// </summary>
        public void AddDelta(PeacelandStatId statId, int delta)
        {
            PeacelandSaveService.Instance.AddStat(statId, delta);
        }

        public string FormatAllStats()
        {
            return "S/A=" + Get(PeacelandStatId.SelfishAltruistic)
                + " I/N=" + Get(PeacelandStatId.InsightNaivety)
                + " N/RA=" + Get(PeacelandStatId.NationalismRebellion)
                + " K/C=" + Get(PeacelandStatId.KindnessCruelty);
        }
    }
}
