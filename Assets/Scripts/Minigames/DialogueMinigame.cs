using System.Collections;
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
        StartCoroutine(StartMinigameCoroutine());
    }

    private IEnumerator StartMinigameCoroutine()
    {
        yield return new WaitForSeconds(0.01f);
        StopDialogueIfRunning();
        dialogueRunner.StartDialogue(startNode);
    }

    public override void StopMinigame()
    {
        StopDialogueIfRunning();
    }

    private void StopDialogueIfRunning()
    {
        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.Stop();
        }
    }

}
