using System;
using System.Collections;
using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Yarn command &lt;&lt;rhythm_beat Kind [lead]&gt;&gt;. Put this on the same object as DialogueRunner.
/// The dialogue waits on the line until the beat lands, and a miss only asks again, so a
/// writer can put a gesture between any two lines without touching the sequence asset:
///
///   Organizer: Stay with the pressure.
///   &lt;&lt;rhythm_beat Hold&gt;&gt;
///   Organizer: Again. Again. Again.
///   &lt;&lt;rhythm_beat MultiTap 1.2&gt;&gt;
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(DialogueRunner))]
public sealed class RhythmYarnCommands : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [Tooltip("Optional. Found in the scene when empty.")]
    [SerializeField] private RhythmNarrativeMinigame rhythm;
    [SerializeField] private float defaultLeadTime = 0.85f;

    private void Awake()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = GetComponent<DialogueRunner>();
        }

        dialogueRunner.AddCommandHandler<string, float>("rhythm_beat", PlayBeat);
    }

    /// <summary>Yarn: &lt;&lt;rhythm_beat Hold&gt;&gt; or &lt;&lt;rhythm_beat Hold 1.2&gt;&gt;. Unknown kinds log a warning and do nothing.</summary>
    private IEnumerator PlayBeat(string kindName, float leadTime = 0f)
    {
        if (!Enum.TryParse(kindName, true, out RhythmBeatKind kind))
        {
            Debug.LogWarning("Unknown rhythm beat kind in Yarn command: " + kindName, this);
            yield break;
        }

        if (rhythm == null)
        {
            rhythm = FindFirstObjectByType<RhythmNarrativeMinigame>(FindObjectsInactive.Include);
        }

        if (rhythm == null)
        {
            Debug.LogWarning("rhythm_beat has no RhythmNarrativeMinigame in the scene to play on.", this);
            yield break;
        }

        yield return rhythm.PlayBeatUntilSuccess(kind, leadTime > 0f ? leadTime : defaultLeadTime);
    }
}
