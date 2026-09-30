using System;

namespace Peaceland
{
    /// <summary>
    /// Hidden narrative stats. The 9/18/2026 big-team meeting settled on two:
    /// KindnessCruelty (how Marc treats the person in front of him) and
    /// InsightNaivety (how well he understands what he is looking at).
    /// The other two are kept only so old saves and assets still deserialize;
    /// they are hidden in the Inspector, refused by the Yarn command, and
    /// not shown on the hidden-stats page.
    /// </summary>
    public enum PeacelandStatId
    {
        [Obsolete("Retired 9/18/2026; the demo uses KindnessCruelty and InsightNaivety only.")]
        SelfishAltruistic = 0,
        InsightNaivety = 1,
        [Obsolete("Retired 9/18/2026; the demo uses KindnessCruelty and InsightNaivety only.")]
        NationalismRebellion = 2,
        KindnessCruelty = 3,
    }

    public static class PeacelandStats
    {
        /// <summary>The stats the game actually tracks, in display order.</summary>
        public static readonly PeacelandStatId[] Active =
        {
            PeacelandStatId.KindnessCruelty,
            PeacelandStatId.InsightNaivety,
        };

        public static bool IsActive(PeacelandStatId statId)
        {
            return Array.IndexOf(Active, statId) >= 0;
        }
    }
}
