using System.Collections.Generic;
using UnityEngine;

public class SneakManager : MinigameBehavior
{
    // Fields
    private bool isRunning;
    private Camera cam;
    [SerializeField]
    private Vector3 startPos;
    [SerializeField]
    private float camSpeed;
    private SneakPlayer player;

    [SerializeField]
    private GameObject goalTile;

    [SerializeField]
    private Vector3[] sentrySpawners;
    private int currentSentrySpawner = 0;
    [SerializeField]
    private SneakSentry sentryPrefab;
    private List<SneakSentry> sentryList = new List<SneakSentry>();

    // UI fields
    [SerializeField]
    private float loseDistance;
    [SerializeField]
    private GameObject sneakUI;
    [SerializeField]
    private GameObject gameOverScreen;
    [SerializeField]
    private GameObject victoryScreen;


    /// <summary>
    /// Starts the minigame sequence.
    /// </summary>
    public override void StartMinigame()
    {
        cam = FindFirstObjectByType<Camera>();
        player = FindFirstObjectByType<SneakPlayer>();
        sneakUI.SetActive(true);
        gameOverScreen.SetActive(false);
        victoryScreen.SetActive(false);
        isRunning = true;
    }

    /// <summary>
    /// Ends minigame sequence.
    /// </summary>
    public override void StopMinigame()
    {
        Debug.Log("Finished Sneaking Minigame");
        gameObject.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // TEST CODE! Likely not needed for final version.
        StartMinigame();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning) 
        {
            player.MovePlayer();
            MoveCamera();
            MoveSentries();

            // The player loses if they fall too far behind the camera or is caught
            if (player.transform.position.x < cam.transform.position.x - loseDistance
                || player.IsCaught)
            {
                DisplayGameOver();
            }
            // The player wins after reaching the goal tile
            else if(player.transform.position.x >= goalTile.transform.position.x)
            {
                DisplayVictory();
            }

            // Check if the player has passed the current sentry spawn point
            if (currentSentrySpawner < sentrySpawners.Length &&
                player.transform.position.x >= sentrySpawners[currentSentrySpawner].x)
            {
                SpawnSentry(new Vector3(sentrySpawners[currentSentrySpawner].x + 8, 0, 0));
                currentSentrySpawner++;
            }

            // TODO: Clean-up method for old sentries
            
        }
    }

    /// <summary>
    /// Moves the camera forward by the camera speed
    /// </summary>
    private void MoveCamera()
    {
        cam.transform.Translate(camSpeed, 0, 0);
    }


    private void MoveSentries()
    {
        foreach (SneakSentry sentry in sentryList)
        {
            sentry.Move();
        }
    }

    /// <summary>
    /// Spawns a new Sentry
    /// </summary>
    /// <param name="spawnPos">A vector 3 position at which to spawn the sentry</param>
    private void SpawnSentry(Vector3 spawnPos)
    {
        sentryList.Add(
            SneakSentry.Instantiate(sentryPrefab, spawnPos, new Quaternion())
            );
    }


    /// <summary>
    /// Reset all elements of the minigame to the default state
    /// </summary>
    public void ResetSneakGame()
    {
        cam.transform.position = startPos;
        player.ResetPlayer();
        gameOverScreen.SetActive(false);
        victoryScreen.SetActive(false);
        sneakUI.SetActive(true);
        isRunning = true;

        currentSentrySpawner = 0;
        for (int i = 0; i < sentryList.Count; i++)
        {
            Destroy(sentryList[i].gameObject);
        //    sentryList.Remove(sentry);
        }
        sentryList.Clear();
    }

    /// <summary>
    /// Opens Game Over menu
    /// </summary>
    private void DisplayGameOver()
    {
        isRunning = false;
        sneakUI.SetActive(false);
        gameOverScreen.SetActive(true);
    }

    /// <summary>
    /// Opens Victory Menu
    /// </summary>
    private void DisplayVictory()
    {
        isRunning = false;
        sneakUI.SetActive(false);
        victoryScreen.SetActive(true);
    }
}
