using UnityEngine;

public class SneakManager : MinigameBehavior
{
    // Fields
    private Camera cam;
    [SerializeField]
    private float camSpeed;
    private SneakPlayer player;
    
    

    public override void StartMinigame()
    {
        cam = FindFirstObjectByType<Camera>();
        player = FindFirstObjectByType<SneakPlayer>();
    }

    public override void StopMinigame()
    {
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
        player.MovePlayer();
        MoveCamera();

    }

    private void MoveCamera()
    {
        cam.transform.position = new Vector3(cam.transform.position.x + camSpeed,
                   cam.transform.position.y, cam.transform.position.z);
    }
}
