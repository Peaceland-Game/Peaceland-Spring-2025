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

            // The player loses if they fall too far behind the camera
            if (player.transform.position.x < cam.transform.position.x - loseDistance)
            {
                DisplayGameOver();
            }
            // The player wins after reaching the goal tile
            else if(player.transform.position.x >= goalTile.transform.position.x)
            {
                DisplayVictory();
            }
        }
    }

    /// <summary>
    /// Moves the camera forward by the camera speed
    /// </summary>
    private void MoveCamera()
    {
        cam.transform.position = new Vector3(cam.transform.position.x + camSpeed,
                   cam.transform.position.y, cam.transform.position.z);
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
