using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;

    public float transitionTime = 1f;

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit!");
    }

    /// <summary>
    /// Loads the next level in the build index sequence
    /// </summary>
    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    /// <summary>
    /// Loads a level based on a given build index
    /// </summary>
    /// <param name="buildIndex">An int of the scene to load.</param>
    public void LoadLevelByBuildIndex(int buildIndex)
    {
        StartCoroutine(LoadLevel(buildIndex));
    }

    /// <summary>
    /// Loads a level by a given index and plays fade animations.
    /// </summary>
    /// <param name="levelIndex">An integer containing the build index to load.</param>
    /// <returns>Returns a yield statement</returns>
    IEnumerator LoadLevel(int levelIndex)
    {
        Debug.Log("Running fade out animation.");
        //    transition.SetTrigger("Start");

        WhiteFadeAnimation(true);

        yield return new WaitForSeconds(transitionTime);

        WhiteFadeAnimation(false);

        SceneManager.LoadScene(levelIndex);
    }

    /// <summary>
    /// Start the coroutine to play a White Fade amimation on command
    /// </summary>
    public void WhiteFadeAnimation(bool _willFadeIn)
    {
        if (_willFadeIn)
        {
            StartCoroutine(PlayWhiteFadeInAnimation());
        }
        else
        {
            StartCoroutine(PlayWhiteFadeOutAnimation());
        }

    }

    /// <summary>
    /// Plays the WhiteFadein amimation on command
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayWhiteFadeInAnimation()
    {
        // Important note: The the second "i" is lowercase
        transition.Play("Base Layer.WhiteFadein", 0, 0.0f);
        yield return new WaitForSeconds(transitionTime);
    }

    // Plays the WhiteFadeOut animimation on command
    IEnumerator PlayWhiteFadeOutAnimation()
    {
        transition.Play("Base Layer.WhiteFadeOut", 0, 0.0f);
        yield return new WaitForSeconds(transitionTime);
    }

    /// <summary>
    /// Start the coroutine to play a black fade amimation on command
    /// </summary>
    public void BlackFadeAnimation(bool _willFadeIn)
    {
        if (_willFadeIn)
        {
            StartCoroutine(PlayBlackFadeInAnimation());
        }
        else
        {
            StartCoroutine(PlayBlackFadeOutAnimation());
        }

    }

    /// <summary>
    /// Plays the BlackFadeIn amimation on command
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayBlackFadeInAnimation()
    {
        transition.Play("Base Layer.BlackFadeIn", 0, 0.0f);
        yield return new WaitForSeconds(transitionTime);
    }

    // Plays the BlackFadeOut animimation on command
    IEnumerator PlayBlackFadeOutAnimation()
    {
        transition.Play("Base Layer.BlackFadeOut", 0, 0.0f);
        yield return new WaitForSeconds(transitionTime);
    }
}
