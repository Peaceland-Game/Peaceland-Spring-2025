using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SneakManager : MinigameBehavior
{
    // Fields
    private bool isRunning;     // Bool to track if the minigame is active
    private Camera cam;         // Reference to the Main Camera
    [SerializeField]
    private Vector3 startPos;   // Camera starting position
    [SerializeField]
    private float camSpeed;     // Movement speed of camera
    private SneakPlayer player; // Reference to the player
    [SerializeField]
    private GameObject backgroundLoop; // Reference to the background

    [SerializeField]
    private GameObject goalTile;    // Reference to the goal tile

    [SerializeField]
    private Vector3[] sentrySpawners;       // List of points to spawn Sentries
    private int currentSentrySpawner = 0;   // Current spawner position to check against
    [SerializeField]
    private float sentrySpawnOffset;        // Ditance to offset Sentries from their spawners
    [SerializeField]
    private SneakSentry sentryPrefab;       // Reference to Sentry prefab to instantiate from
    private List<SneakSentry> sentryList = new List<SneakSentry>(); // List of active Sentries
    [SerializeField]
    private float loseDistance;     // Distance from camera to remove old objects

    // UI fields
    [SerializeField]
    private GameObject sneakUI;     // UI for when the game is running
    [SerializeField]
    private GameObject gameOverScreen;  // UI for when the player has been caught
    [SerializeField]
    private GameObject victoryScreen;   // UI for when the player has reached the goal

    // Level Loader
    protected LevelLoader LL;
    public LevelLoader LevelLoader { get { return LL; } }


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
        LL = FindFirstObjectByType<LevelLoader>();
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
            if (!IsObjectVisible(player)
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
                SpawnSentry(new Vector3(sentrySpawners[currentSentrySpawner].x + sentrySpawnOffset, 2.5f, 0));
                currentSentrySpawner++;
            }

            // Remove offscreen sentries
            CleanUpOldSentries();

        }
    }

    /// <summary>
    /// Moves the camera forward by the camera speed
    /// </summary>
    private void MoveCamera()
    {
        cam.transform.Translate(camSpeed * Time.deltaTime, 0, 0);
    }

    /// <summary>
    /// Move all active Sentries each frame
    /// </summary>
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
            Instantiate(sentryPrefab, spawnPos, Quaternion.AngleAxis(45f, Vector3.forward))
            );
    }

    /// <summary>
    /// Destroy sentries once they move off screen
    /// </summary>
    private void CleanUpOldSentries()
    {
        for (int i = 0; i < sentryList.Count; i++)
        {
            // If a sentry is off screen, delete it
            if (sentryList[i].transform.position.x < cam.transform.position.x - loseDistance) 
            {
                Destroy(sentryList[i].gameObject);
                sentryList.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Reset all elements of the minigame to the default state
    /// </summary>
    public void ResetSneakGame()
    {
        cam.transform.position = startPos;
        player.ResetPlayer();
        BackgroundLoop[] backgroundPanels = backgroundLoop.GetComponentsInChildren<BackgroundLoop>();
        foreach (BackgroundLoop b in backgroundPanels)
        {
            b.ResetBackground();
        }
        gameOverScreen.SetActive(false);
        victoryScreen.SetActive(false);
        sneakUI.SetActive(true);
        isRunning = true;

        currentSentrySpawner = 0;
        for (int i = 0; i < sentryList.Count; i++)
        {
            Destroy(sentryList[i].gameObject);
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

    public void loadNextLevel()
    {
        LL.LoadLevelByBuildIndex(2);
    }

    /// <summary>
    /// Checks if the given object is on camera, returning a combined boolean for each axis.
    /// </summary>
    /// <param name="player">The SneakPlayer, but its could be genericized for potential futurue use in other circumstances</param>
    /// <returns></returns>
    public bool IsObjectVisible(SneakPlayer player)
    {
        Vector3 viewPoint = cam.WorldToViewportPoint(player.transform.position);

        // Check each axis to make sure the object is on camera, basically a stand in for checking the object agianst the bounds of the canvas, that way it adapts to PC or tablet mode
        bool isXVisible = viewPoint.x >= 0f && viewPoint.x <= 1f;
        bool isYVisible = viewPoint.y >= 0f && viewPoint.y <= 1f;
        bool isInFrontOfCamera = viewPoint.z > 0f;

        return isXVisible && isYVisible && isInFrontOfCamera;
    }

    /// <summary>
    /// Cloese the application, for use in the test builds of the escape minigame.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();

        Debug.Log("Game has been quit.");
    }
}
