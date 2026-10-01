using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

/// <summary>
/// Exploration trigger. It can invoke authored UnityEvents and optionally start a Yarn node.
/// Entering a trigger is not a failure condition.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public sealed class MazeEventTrigger2D : MonoBehaviour
{
    [SerializeField] private bool oneShot = true;
    [SerializeField] private UnityEvent onPlayerEntered;
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string startNode;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        MazePlayerController2D player = other.GetComponentInParent<MazePlayerController2D>();
        if (player == null || (oneShot && hasTriggered))
        {
            return;
        }

        hasTriggered = true;
        onPlayerEntered?.Invoke();

        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning && !string.IsNullOrWhiteSpace(startNode))
        {
            dialogueRunner.StartDialogue(startNode);
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger == null)
        {
            return;
        }

        Gizmos.color = new Color(0.25f, 0.9f, 0.95f, 0.35f);
        Gizmos.DrawCube(trigger.bounds.center, trigger.bounds.size);
    }
}
