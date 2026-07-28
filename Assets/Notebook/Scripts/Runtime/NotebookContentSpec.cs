using System;
using UnityEngine;

namespace Peaceland.Notebook
{
    [Serializable]
    public sealed class NotebookContentSpec
    {
        [Tooltip("Stable id — must match NotebookEntryDefinition.entryId")]
        public string entryId;

        public NotebookContentKind kind = NotebookContentKind.Gameplay;

        public NotebookSection section = NotebookSection.Present;

        [Tooltip("Expected categoryId (e.g. newspaper, minigame, scene-collected)")]
        public string categoryId;

        [Tooltip("Gameplay entries should have non-empty body unless this is false")]
        public bool requireBodyText = true;

        [Tooltip("Harness expects a NotebookCollectTrigger wired in the named test scene")]
        public bool requireCollectTrigger;

        [Tooltip("Scene file name without path, e.g. NotebookTest_FloristItemCollect")]
        public string collectSceneName;

        [Tooltip("Optional note for developers / designers")]
        public string notes;
    }
}
