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
        Debug.Log("Start trigger = " + transition.GetBool("Start"));
        transition.SetTrigger("Start");
        Debug.Log("Start trigger = " + transition.GetBool("Start"));

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }

    public void FadeAnimation()
    {
        StartCoroutine(PlayFadeAnimation());
    }

    IEnumerator PlayFadeAnimation()
    {
        //transition.ResetTrigger("Start");
        //Debug.Log("Start trigger = " + transition.GetBool("Start"));
        //yield return new WaitForSeconds(transitionTime);
        //transition.SetTrigger("Start");
        //Debug.Log("Start trigger = " + transition.GetBool("Start"));

        Debug.Log("Playing fade animation");
        transition.Play("Base Layer.WhiteFadein", 0, 0.0f);
        yield return new WaitForSeconds(transitionTime);
    //    transition.Play("Base Layer.WhiteFadeIn", 0, 0.0f);
    }
}
