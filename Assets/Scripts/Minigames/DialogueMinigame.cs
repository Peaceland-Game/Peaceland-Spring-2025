using UnityEngine;
using Yarn.Unity;

public class DialogueMinigame : MinigameBehavior
{

    [SerializeField]
    private DialogueRunner dialogueRunner;

    [SerializeField]
    private string startNode;

    public override void StartMinigame()
    {
        dialogueRunner.StartDialogue(startNode);
    }

    public override void StopMinigame()
    {}
}
