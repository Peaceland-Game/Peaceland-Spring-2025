using System;
using UnityEngine;

namespace Peaceland.Notebook
{
    [CreateAssetMenu(fileName = "NewArtifact", menuName = "Peaceland/Notebook/Artifact Definition")]
    public class ArtifactDefinition : ScriptableObject
    {
        [Header("Artifact Info")]
        [SerializeField] private string id = "artifact_id";
        [SerializeField] private string title = "Artifact Title";
        [SerializeField] private Sprite icon;

        [Header("Grouping")]
        [Tooltip("Chapter name, e.g. Romeo & Juliet, Museum, Florist Memory, Child Memory, Town Square")]
        [SerializeField] private string chapter = "Romeo & Juliet";
        [Tooltip("Category heading, e.g. MEMORIES or PRESENT")]
        [SerializeField] private string category = "MEMORIES";
        [SerializeField] private int sortOrder = 0;

        [Header("Description")]
        [Tooltip("Placeholder description of what the artifact is and represents. Easily editable!")]
        [TextArea(3, 8)]
        [SerializeField] private string description = "[Placeholder] Description of what this artifact is and what it represents.";

        [Header("Initial State")]
        [Tooltip("If true, this artifact is already collected when the game starts.")]
        [SerializeField] private bool collectedAtStart = false;

        public string Id => string.IsNullOrEmpty(id) ? name : id;
        public string Title => title;
        public Sprite Icon => icon;
        public string Chapter => string.IsNullOrEmpty(chapter) ? "General" : chapter;
        public string Category => string.IsNullOrEmpty(category) ? "MEMORIES" : category.ToUpper();
        public int SortOrder => sortOrder;
        public string Description => description;
        public bool CollectedAtStart => collectedAtStart;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = name;
            }
        }
    }
}
