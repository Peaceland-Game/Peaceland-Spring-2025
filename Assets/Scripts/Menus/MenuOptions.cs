using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuOptions : MonoBehaviour
{
    //selected button (only important for controller support)
    [SerializeField] GameObject firstSelectedButton;

    //menu options
    void OnEnable()
    {
        MenuManager.Instance.FocusButton(firstSelectedButton);
    }

    public void PauseGame()
    {
        MenuManager.Instance.PauseGame();
    }

    public void OpenSettings()
    {
        MenuManager.Instance.OpenSettings();
    }

    public void OpenSaveExit()
    {
        MenuManager.Instance.OpenSaveExit();
    }
    public void QuitGame()
    {
        MenuManager.Instance.QuitGame();
    }

    public void MenuBack()
    {
        MenuManager.Instance.CloseMenu();
    }

    public void ReloadScene()
    {
        MenuManager.Instance.SwitchScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        MenuManager.Instance.LoadNextLevel();
    }
}
