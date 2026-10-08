using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Lets a memory manager (GenericMemManager and its F26_ subclasses) run the maze like any other
/// entry in its Minigames list. StartMinigame shows the maze and starts the errand list;
/// when the last objective is done the maze hides and the manager moves on to the next minigame.
///
/// Setup: put the whole maze (player, walls, NPCs, triggers, MazeObjectiveSequence) under one
/// root object, switch that root OFF in the scene, and drag it into Maze Root.
/// </summary>
public sealed class MazeMinigame : MinigameBehavior
{
    [Tooltip("Parent of the whole maze. Leave it switched off in the scene; StartMinigame switches it on.")]
    [SerializeField] private GameObject mazeRoot;
    [Tooltip("The errand list. Found under Maze Root when empty.")]
    [SerializeField] private MazeObjectiveSequence sequence;
    [Tooltip("When the maze is done, call NextMinigame on the scene's memory manager.")]
    [SerializeField] private bool advanceMemoryManager = true;
    [Tooltip("Hide the maze again when it is done.")]
    [SerializeField] private bool hideWhenDone = true;
    [Tooltip("Runs when the maze is done, before the manager moves on.")]
    [SerializeField] private UnityEvent onMazeFinished;

    private bool running;

    public override void StartMinigame()
    {
        if (mazeRoot == null)
        {
            Debug.LogWarning(name + ": Maze Root is empty, so there is no maze to start.", this);
            return;
        }

        if (sequence == null)
        {
            sequence = mazeRoot.GetComponentInChildren<MazeObjectiveSequence>(true);
        }

        if (sequence == null)
        {
            Debug.LogWarning(name + ": no MazeObjectiveSequence under Maze Root, so the maze can never finish.", this);
        }
        else
        {
            sequence.SequenceCompleted -= HandleFinished;
            sequence.SequenceCompleted += HandleFinished;
        }

        running = true;
        bool replay = sequence != null && sequence.CurrentIndex >= 0;
        mazeRoot.SetActive(true);
        if (replay)
        {
            sequence.Restart();
        }
    }

    public override void StopMinigame()
    {
        running = false;
        if (sequence != null)
        {
            sequence.SequenceCompleted -= HandleFinished;
        }

        if (mazeRoot != null)
        {
            mazeRoot.SetActive(false);
        }
    }

    private void HandleFinished()
    {
        if (!running)
        {
            return;
        }

        running = false;
        onMazeFinished?.Invoke();

        if (hideWhenDone && mazeRoot != null)
        {
            mazeRoot.SetActive(false);
        }

        if (advanceMemoryManager)
        {
            GenericMemManager manager = FindFirstObjectByType<GenericMemManager>();
            if (manager != null)
            {
                manager.NextMinigame();
            }
            else
            {
                Debug.LogWarning(name + ": maze finished but there is no memory manager in the scene to move on.", this);
            }
        }
    }
}
