using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Manages dialogue for an interaction in the flower shop
/// Each interaction has an intro, an outro, and some short dialogue during gameplay.
/// </summary>
[RequireComponent(typeof(DialogueRunner))]
public class FlowerShopDialogue : MonoBehaviour
{
    /// <summary>
    /// Node that begins intro dialogue for a character.
    /// </summary>
    [SerializeField]
    private string introNode;

    /// <summary>
    /// Node that begins outro dialogue for a character.
    /// </summary>
    [SerializeField]
    private string outroNode;

    /// <summary>
    /// Node for random dialogue when doing well.
    /// </summary>
    [SerializeField]
    private string gameplayGoodNode;

    private DialogueRunner dialogueRunner;

    void Start()
    {
        dialogueRunner = GetComponent<DialogueRunner>();
    }

    /// <summary>
    /// Play the intro dialogue
    /// </summary>
    public void DoIntroDialogue()
    {
        dialogueRunner.StartDialogue(introNode);
    }

    /// <summary>
    /// Play the outro dialogue
    /// </summary>
    public void DoOutroDialogue()
    {
        dialogueRunner.StartDialogue(outroNode);
    }
}
