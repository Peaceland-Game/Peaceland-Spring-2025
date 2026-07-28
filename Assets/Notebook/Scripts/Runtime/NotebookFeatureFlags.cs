namespace Peaceland.Notebook
{
    public static class NotebookFeatureFlags
    {
        /// <summary>
        /// Hidden-stats notebook section is playtest/debug only and not shown in release builds.
        /// </summary>
        public static bool IncludeHiddenStatsSection
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsSectionVisible(NotebookSection section)
        {
            if (section == NotebookSection.HiddenStats)
            {
                return IncludeHiddenStatsSection;
            }

            return true;
        }
    }
}
