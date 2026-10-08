using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

/// <summary>
/// Runs the maze as a there-and-back-again errand list. Exactly one trigger is armed
/// at a time, so "walk there, hear what it has to say, walk to the next one" is the
/// whole loop. List the same trigger twice, with another in between, and the player
/// has to go back to it - that is all the back and forth is.
/// </summary>
public sealed class MazeObjectiveSequence : MonoBehaviour
{
    [Tooltip("Visited in order. The same trigger may appear more than once.")]
    [SerializeField] private MazeEventTrigger2D[] objectives;

    [Tooltip("Found in the scene when empty.")]
    [SerializeField] private MazePlayerController2D player;
    [Tooltip("Optional. Found in the scene when empty. Without one the sequence advances as soon as a trigger fires.")]
    [SerializeField] private DialogueRunner dialogueRunner;

    [Tooltip("How long to wait for a node to actually start before giving up on it.")]
    [SerializeField] private float dialogueStartGrace = 0.25f;

    [Header("Events")]
    [Tooltip("Runs each time a new objective becomes the one to reach.")]
    [SerializeField] private UnityEvent onObjectiveChanged;
    [Tooltip("Runs when the player is caught and has to reach the current objective again.")]
    [SerializeField] private UnityEvent onObjectiveReset;
    [Tooltip("Runs once when the last objective is done. MazeMinigame listens for this on its own.")]
    [SerializeField] private UnityEvent onSequenceComplete;

    private int currentIndex = -1;
    private Coroutine objectiveRoutine;

    /// <summary>Raised once when the last objective is reached, right after onSequenceComplete.</summary>
    public event System.Action SequenceCompleted;

    public int CurrentIndex => currentIndex;
    public bool IsComplete => objectives != null && currentIndex >= objectives.Length;

    public MazeEventTrigger2D CurrentObjective =>
        objectives != null && currentIndex >= 0 && currentIndex < objectives.Length
            ? objectives[currentIndex]
            : null;

    private void Awake()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<MazePlayerController2D>(FindObjectsInactive.Include);
        }

        if (dialogueRunner == null)
        {
            dialogueRunner = FindFirstObjectByType<DialogueRunner>(FindObjectsInactive.Include);
        }
    }

    private void OnEnable()
    {
        if (objectives == null)
        {
            return;
        }

        foreach (MazeEventTrigger2D objective in objectives)
        {
            if (objective != null)
            {
                objective.PlayerEntered -= HandlePlayerEntered;
                objective.PlayerEntered += HandlePlayerEntered;
            }
        }
    }

    private void OnDisable()
    {
        if (objectives == null)
        {
            return;
        }

        foreach (MazeEventTrigger2D objective in objectives)
        {
            if (objective != null)
            {
                objective.PlayerEntered -= HandlePlayerEntered;
            }
        }
    }

    private void Start()
    {
        if (currentIndex < 0)
        {
            GoTo(0);
        }
    }

    /// <summary>Starts the errand list over from the first objective and sends the player to the start point.</summary>
    public void Restart()
    {
        if (objectiveRoutine != null)
        {
            StopCoroutine(objectiveRoutine);
            objectiveRoutine = null;
        }

        if (player != null)
        {
            player.ResetToStart();
        }

        GoTo(0);
    }

    /// <summary>
    /// Medium punishment: the walk is lost but the story is not. Hook this to the
    /// NPC's onPlayerCaught - it re-arms the objective the player was on their way to,
    /// which they now have to reach again from the start point.
    /// </summary>
    public void ResetCurrentObjective()
    {
        if (objectives == null || currentIndex >= objectives.Length)
        {
            // Already complete; re-arming would fire onSequenceComplete a second time.
            return;
        }

        if (objectiveRoutine != null)
        {
            StopCoroutine(objectiveRoutine);
            objectiveRoutine = null;
            if (player != null)
            {
                player.SetMovementEnabled(true);
            }
        }

        onObjectiveReset?.Invoke();
        GoTo(currentIndex);
    }

    private void GoTo(int index)
    {
        DisarmAll();
        currentIndex = Mathf.Max(0, index);

        if (objectives == null || currentIndex >= objectives.Length)
        {
            onSequenceComplete?.Invoke();
            SequenceCompleted?.Invoke();
            return;
        }

        MazeEventTrigger2D objective = objectives[currentIndex];
        if (objective == null)
        {
            // A hole in the list is an authoring mistake, not a reason to strand the player.
            Debug.LogWarning($"Objective {currentIndex} is empty; skipping it.", this);
            GoTo(currentIndex + 1);
            return;
        }

        // Re-armed every time so the same trigger can serve more than one objective.
        objective.ResetTrigger();
        objective.SetArmed(true);
        onObjectiveChanged?.Invoke();
    }

    private void DisarmAll()
    {
        if (objectives == null)
        {
            return;
        }

        foreach (MazeEventTrigger2D objective in objectives)
        {
            if (objective != null)
            {
                objective.SetArmed(false);
            }
        }
    }

    private void HandlePlayerEntered(MazeEventTrigger2D objective)
    {
        if (objective != CurrentObjective || objectiveRoutine != null)
        {
            return;
        }

        objectiveRoutine = StartCoroutine(RunObjective());
    }

    private IEnumerator RunObjective()
    {
        if (player != null)
        {
            player.SetMovementEnabled(false);
        }

        // Polled rather than hooked to onDialogueComplete. Yarn raises that event from
        // Stop() as well as from the end of a node, so listening for it advances the
        // sequence a step early whenever anything stops a dialogue.
        float waited = 0f;
        while (dialogueRunner != null
               && !dialogueRunner.IsDialogueRunning
               && waited < dialogueStartGrace)
        {
            waited += Time.deltaTime;
            yield return null;
        }

        while (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
        {
            yield return null;
        }

        if (player != null)
        {
            player.SetMovementEnabled(true);
        }

        objectiveRoutine = null;
        GoTo(currentIndex + 1);
    }
}
