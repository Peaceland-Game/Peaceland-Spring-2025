using UnityEngine;
using Unity.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using UnityEditor.Rendering.Universal.ShaderGUI;

public class F26_TownSquareManager : GenericMemManager
{

    //Initializes variables and plays the starting dialogue
    void Start()
    {
        F26_GameManager.Instance.CurrentScene = "Day1TownSquare";
        NextMinigame();
        NextOrder();
    }

    /// <summary>
    /// While the constant repeated use of if statements may seem a bit messy and repetitive,
    /// it does help manage the constant event-after-event loop that goes on in this scene after each bit of dialogue.
    /// 
    /// The fight dialogue was split into two segments, which is the conversation with Branko, and the one with Katarina.
    /// This allows characters (Placeholders in the scene) to appear when being talked to, and disappear when not (Only works with Branko at the moment).
    /// 
    /// The code currently has a segment to move on to the next scene after the conversation with Katarina
    /// </summary>
    void Update()
    {
    }
}
