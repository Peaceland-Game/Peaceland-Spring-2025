using UnityEngine;
using Unity.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using UnityEditor.Rendering.Universal.ShaderGUI;

public class F26_EndingManager : GenericMemManager
{
    bool executedEnding = false;
    const int ENDING_THRESHOLD_KNOWLEDGE = 7;
    //Initializes variables and plays the starting dialogue
    void Start()
    {
        F26_GameManager.Instance.CurrentScene = "Day1Home";
        NextMinigame();
        NextOrder();
    }

    void Update()
    {
    }

    public override void NextMinigame()
    {
        if (!executedEnding)
        {
            executedEnding = true;
            if(F26_GameManager.Instance.KnowledgeStat >= ENDING_THRESHOLD_KNOWLEDGE)
            {
                currentMinigame = 0;
            }
            else
            {
                currentMinigame = 1;
            }
            minigames[currentMinigame].StartMinigame();
        }
    }
}
