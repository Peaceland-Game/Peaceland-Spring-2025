namespace Peaceland.Notebook
{
    /// <summary>
    /// How an entry is treated by the content harness (production contract vs test-only).
    /// </summary>
    public enum NotebookContentKind
    {
        /// <summary>Shippable gameplay unlock — must exist, validate, and usually have a collect source.</summary>
        Gameplay = 0,

        /// <summary>Harness / test scene only — validated lightly, not required for ship checklist.</summary>
        TestHarness = 1,

        /// <summary>Auto-generated pagination stress entries (DummyPage_*).</summary>
        PaginationDummy = 2,
    }
}
