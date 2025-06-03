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
        dialogueRunner.StartDialogue(startNode);
    }

    public override void StopMinigame()
    {
    }

}
