using System.Collections;
using UnityEngine;

public class FadeTransitionMinigame : MinigameBehavior
{
    [SerializeField]
    private float transitionWaitTime;

    [SerializeField]
    private int newBackgroundIndex;     // The Manager object in scene has a list of backgrounds and indecies

    public override void StartMinigame()
    {
        Debug.Log("Begin fade transition minigame");
        StartCoroutine(PlayFadeTransition());
    }

    public override void StopMinigame()
    {
        // Does nothing, putting Debug message for debuging purposes
        Debug.Log("Fade transition minigame over");
    }

    // Plays the Fade Animation
    IEnumerator PlayFadeTransition()
    {
        // Play fade in animation
        GameManager.Instance.CurrentMemoryManager.LevelLoader.FadeAnimation(true);
        // Wait for animation to finish
        yield return new WaitForSeconds(transitionWaitTime);

        // Update background
        GameManager.Instance.CurrentMemoryManager.ChangeBackgroundSprite(newBackgroundIndex);

        // Play fade out animation
        GameManager.Instance.CurrentMemoryManager.LevelLoader.FadeAnimation(false);
        // Wait for animation to finish
        yield return new WaitForSeconds(transitionWaitTime);


        // Trigger the next minigame
        GameManager.Instance.CurrentMemoryManager.NextMinigame();
    }
}
