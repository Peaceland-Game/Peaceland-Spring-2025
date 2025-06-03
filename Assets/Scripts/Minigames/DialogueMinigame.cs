using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class DialogueMinigame : MinigameBehavior
{

    [SerializeField]
    private DialogueRunner dialogueRunner;

    [SerializeField]
    private string startNode;

    //Darken/Lighten Testing
    [SerializeField]
    private GameObject characterPortrait;

    public override void StartMinigame()
    {
        StartCoroutine(StartMinigameCoroutine());

        //Darken/Lighten Testing
        dialogueRunner.AddCommandHandler("lighten", Lighten);
        dialogueRunner.AddCommandHandler("darken", Darken);
    }

    private IEnumerator StartMinigameCoroutine()
    {
        yield return new WaitForSeconds(0.01f);
        dialogueRunner.StartDialogue(startNode);
    }

    public override void StopMinigame()
    {
    }

    //Darken/Lighten Testing
    private void Darken()
    {
        characterPortrait.GetComponent<SpriteRenderer>().color = new Color(194, 194, 194);
        Debug.Log("Darken!");
    }

    //Darken/Lighten Testing
    private void Lighten()
    {
        characterPortrait.GetComponent<SpriteRenderer>().color = Color.white;
        Debug.Log("Lighten!");
    }
}
