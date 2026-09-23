using System;
using UnityEngine;
using Yarn.Unity;

namespace Peaceland
{
    /// <summary>
    /// Yarn command &lt;&lt;add_stat StatId delta&gt;&gt;. Put this on the same object as DialogueRunner.
    /// StatId must match PeacelandStatId names, e.g. KindnessCruelty.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DialogueRunner))]
    public sealed class YarnStatCommands : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner;

        private void Awake()
        {
            if (dialogueRunner == null)
            {
                dialogueRunner = GetComponent<DialogueRunner>();
            }

            dialogueRunner.AddCommandHandler<string, int>("add_stat", AddStat);
        }

        /// <summary>Yarn: &lt;&lt;add_stat KindnessCruelty 1&gt;&gt;. Unknown names log a warning and do nothing.</summary>
        private void AddStat(string statName, int delta)
        {
            if (!Enum.TryParse(statName, true, out PeacelandStatId statId))
            {
                Debug.LogWarning("Unknown Peaceland stat in Yarn command: " + statName, this);
                return;
            }

            PeacelandStatManager.Instance.AddDelta(statId, delta);
        }
    }
}
