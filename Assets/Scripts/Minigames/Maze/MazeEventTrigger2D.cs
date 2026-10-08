using System;
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
    [Tooltip("Fire only the first time the player walks in.")]
    [SerializeField] private bool oneShot = true;
    [Tooltip("Runs when the player walks in.")]
    [SerializeField] private UnityEvent onPlayerEntered;
    [Tooltip("Optional. Found in the scene when empty and Start Node is set.")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [Tooltip("Yarn node to play when the player walks in. Leave empty for no dialogue.")]
    [SerializeField] private string startNode;

    private bool hasTriggered;
    private bool armed = true;

    /// <summary>
    /// Raised before the UnityEvent and the Yarn node, so a sequence can take over.
    /// </summary>
    public event Action<MazeEventTrigger2D> PlayerEntered;

    /// <summary>
    /// A disarmed trigger is inert. MazeObjectiveSequence keeps exactly one armed
    /// so the player always has one place to be going.
    /// </summary>
    public void SetArmed(bool value)
    {
        armed = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        MazePlayerController2D player = other.GetComponentInParent<MazePlayerController2D>();
        if (player == null || !armed || (oneShot && hasTriggered))
        {
            return;
        }

        hasTriggered = true;
        PlayerEntered?.Invoke(this);
        onPlayerEntered?.Invoke();

        if (string.IsNullOrWhiteSpace(startNode))
        {
            return;
        }

        if (dialogueRunner == null)
        {
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        }

        if (dialogueRunner == null)
        {
            Debug.LogWarning(name + ": Start Node '" + startNode + "' is set but the scene has no DialogueRunner, so no dialogue plays.", this);
            return;
        }

        if (!dialogueRunner.IsDialogueRunning)
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
