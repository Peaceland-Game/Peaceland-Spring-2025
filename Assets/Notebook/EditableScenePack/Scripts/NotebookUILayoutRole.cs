namespace Peaceland.Notebook.EditableScenePack
{
    /// <summary>
    /// Identifies a hand-placed UI rect for audits and tooling (Hierarchy labels).
    /// </summary>
    public enum NotebookUILayoutRole
    {
        Unspecified = 0,
        HudOpenButton = 1,
        CollectibleHint = 2,
        CollectedToast = 3,
        BookOpenRoot = 10,
        BookBackground = 11,
        PagesViewport = 12,
        BookmarkRailLeft = 13,
        BookmarkRailRight = 14,
        DirectoryChrome = 20,
        SectionChrome = 21,
        PageNumbers = 22,
        PlaytestBar = 30,
        HiddenStatsPanel = 31,
        Other = 99,
    }
}
