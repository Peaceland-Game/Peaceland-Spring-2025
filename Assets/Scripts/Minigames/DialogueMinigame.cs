using System.Collections;
using UnityEngine;
using UnityEngine.Events;
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
        if (dialogueRunner == null || !dialogueRunner.IsDialogueRunning)
        {
            return;
        }

        // Yarn's Stop() raises onDialogueComplete, which scenes wire up to "move on to the
        // next minigame". Leaving a minigame is not the same as finishing its dialogue, so
        // hand Stop an empty event and put the real one back afterwards.
        UnityEvent completed = dialogueRunner.onDialogueComplete;
        dialogueRunner.onDialogueComplete = new UnityEvent();
        try
        {
            dialogueRunner.Stop();
        }
        finally
        {
            dialogueRunner.onDialogueComplete = completed;
        }
    }

}
