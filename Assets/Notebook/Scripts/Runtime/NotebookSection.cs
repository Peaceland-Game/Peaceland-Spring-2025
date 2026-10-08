namespace Peaceland.Notebook
{
    /// <summary>
    /// Book sections. Directory is the opening spread. HiddenStats is the debug / design readout,
    /// not a player-facing chapter tab in release.
    /// </summary>
    public enum NotebookSection
    {
        Directory = 0,
        Present = 1,
        Memory1 = 2,
        Memory2 = 3,
        HiddenStats = 4,
    }
}
