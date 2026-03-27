using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class RJMemManager : GenericMemManager
{
    /* Evan's Notes:
     * As of 3/27/2026, there are no differing behaviors between this manager and its parent.
     * However, that is likely to change with the introduction of the minigames.
     * In the event that it does not change, this script can be deleted and replaced with GenericMemManager
     *      once it is no longer abstract.
     */

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NextMinigame();
        NextOrder();
        // Connect dialogue runner
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
    }
}
