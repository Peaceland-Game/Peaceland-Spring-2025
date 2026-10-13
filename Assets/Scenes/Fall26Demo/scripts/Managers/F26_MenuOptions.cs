using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class F26_MenuOptions : MonoBehaviour
{
    //selected button (only important for controller support)
    [SerializeField] GameObject firstSelectedButton;

    //menu options
    void OnEnable()
    {
        F26_MenuManager.Instance.FocusButton(firstSelectedButton);
    }

    public void PauseGame()
    {
        F26_MenuManager.Instance.OpenPauseMenu();
    }

    public void OpenSettings()
    {
        F26_MenuManager.Instance.OpenSettings();
    }

    public void OpenSaveExit()
    {
        F26_MenuManager.Instance.OpenSaveExit();
    }

    public void OpenDialogueReplay()
    {
        F26_MenuManager.Instance.OpenDialogueReplay();
    }

    public void QuitGame()
    {
        F26_MenuManager.Instance.QuitGame();
    }

    public void MenuBack()
    {
        F26_MenuManager.Instance.CloseMenu();
    }

    public void ReloadScene()
    {
        F26_MenuManager.Instance.SwitchScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        F26_MenuManager.Instance.LoadNextLevel();
    }
}
