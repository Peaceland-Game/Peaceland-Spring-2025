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

    IEnumerator LoadLevel(int levelIndex)
    {
        Debug.Log("Running fade out animation.");
        //    transition.SetTrigger("Start");

        FadeAnimation(true);

        yield return new WaitForSeconds(transitionTime);

        FadeAnimation(false);

        SceneManager.LoadScene(levelIndex);
    }

    /// <summary>
    /// Start the coroutine to play the WhiteFadein amimation on command
    /// </summary>
    public void FadeAnimation(bool _willFadeIn)
    {
        if (_willFadeIn)
        {
            StartCoroutine(PlayFadeInAnimation());
        }
        else
        {
            StartCoroutine(PlayFadeOutAnimation());
        }

    }

    /// <summary>
    /// Plays the WhiteFadein amimation on command
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayFadeInAnimation()
    {
        // Important note: The the second "i" is lowercase
        transition.Play("Base Layer.WhiteFadein", 0, 0.0f);
        yield return new WaitForSeconds(transitionTime);
    }

    // Plays the WhiteFadeOut animimation on command
    IEnumerator PlayFadeOutAnimation()
    {
        transition.Play("Base Layer.WhiteFadeOut", 0, 0.0f);
        yield return new WaitForSeconds(transitionTime);
    }
}
