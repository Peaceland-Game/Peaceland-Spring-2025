using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class MuseumIntroManager : MonoBehaviour
{
    //List of "minigames"
    [SerializeField]
    private List<MinigameBehavior> minigames;
    private int currentMinigame = -1;
  
    // Singleton Instace
    public static MuseumIntroManager Instance { get; private set; }

    [SerializeField]
    private DialogueRunner dialogueRunner;

    private GameManager gm;

    /// <summary>
    /// The index of the current minigame that this memory is on
    /// </summary>
    /// <returns>The index of the current minigame this memory is on</returns>
    public static int GetCurrentMinigameIndex()
    {
        return Instance.currentMinigame;
    }

    /// <summary>
    /// The current minigame that this memory is on
    /// </summary>
    /// <returns>The current minigame this memory is on</returns>
    public static MinigameBehavior GetCurrentMinigame()
    {
        return Instance.minigames[GetCurrentMinigameIndex()];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gm = GameManager.Instance;

        Instance = this;
        dialogueRunner.onDialogueComplete.AddListener(NextMinigame);
    }

    // Update is called once per frame
    void Update()
    {
        //when the intro sprawl is done, start Marc's dialogue
        if (gm.introSprawlDone == true && gm.marcStart == false)
        {
            gm.marcStart = true;
            NextMinigame();
        }

    }

    /// <summary>
    /// Changes the state of the flower shop memory
    /// </summary>
    public void NextMinigame()
    {
        //As long as the current minigame is at a valid index (greater than 0), stop the minigame at that index
        if (currentMinigame >= 0) minigames[currentMinigame].StopMinigame();

        //Increment to the next minigame
        currentMinigame++;

        //If the current minigame is higher or equal to the number of minigames,end dialogue for intro
        if (currentMinigame >= minigames.Count)
        {
            gm.miraIntroDone = true;
            return;  
        }

        //Start the next minigame
        minigames[currentMinigame].StartMinigame();
    }
}
