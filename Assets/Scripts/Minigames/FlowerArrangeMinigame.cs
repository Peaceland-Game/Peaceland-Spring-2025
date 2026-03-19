using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static UnityEngine.GraphicsBuffer;

public class FlowerArrangeMinigame : MinigameBehavior
{
    /// <summary>
    /// Reference to the GameObject used for the arrange minigame.
    /// </summary>
    [SerializeField] GameObject arrangeMinigame;

    /// <summary>
    /// Prefab used to instantiate flower GameObjects in the scene.
    /// </summary>
    [SerializeField] GameObject flowerPrefab;

    /// <summary>
    /// Prefab to instantiate as the target object.
    /// </summary>
    [SerializeField] GameObject targetPrefab;


    /// <summary>
    /// The positions of the flowers when the minigame starts
    /// </summary>
    [SerializeField] Vector3[] flowerLocations;

    /// <summary>
    /// The locations of the targets
    /// </summary>
    [SerializeField] Vector3[] targetLocations;

    /// <summary>
    /// The rotations of the targets
    /// </summary>
    [SerializeField] Vector3[] targetRotations;


    private bool isTransitioning;
    private float timer = 0.5f;

    /// <summary>
    /// Sets the blur on the camera
    /// </summary>
    private PostProcessVolume ppVolume;

    private void Start()
    {
        arrangeMinigame.GetComponent<DragManager>().OnCompleted += HandleCompleted;
    }

    private void Update()
    {
        if ( !isTransitioning)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            //Debug.Log("Transitioning to next minigame");
            isTransitioning = false;
            arrangeMinigame.GetComponent<DragManager>().Reset();
            FlowerShopManager.Instance.NextMinigame();
        }

    }

    /// <summary>
    /// Initiates a transition by setting the transitioning flag and resetting the timer.
    /// </summary>
    public void HandleCompleted()
    {
        isTransitioning = true;
        timer = 0.5f; // Reset timer for transition
    }

    public override void StartMinigame()
    {
        ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();

         int count = FlowerShopManager.GetCurrentOrder().flowers.Count;

        Vector3[] dragPos = new Vector3[count];
        Vector3[] targetPos = new Vector3[count];
        Vector3[] targetRot = new Vector3[count];
        FlowerType[] flowerTypes = new FlowerType[count];
        Sprite[] sprites = new Sprite[count];

        for (int i = 0; i < count; i++)
        {
            dragPos[i] = flowerLocations[i];
            targetPos[i] = targetLocations[i];
            targetRot[i] = targetRotations[i];
            flowerTypes[i] = FlowerShopManager.GetCurrentOrder().flowers[i].flowerType;
            sprites[i] = FlowerShopManager.GetFlowerTopSprite(flowerTypes[i]);
        }


        arrangeMinigame.GetComponent<DragManager>().CreateDragToTarget(
            FlowerShopManager.GetCurrentOrder().flowers.Count, 
            flowerPrefab, 
            targetPrefab, 
            dragPos, 
            targetPos, 
            targetRot, 
            flowerTypes,
            sprites);

        //Set the arranging minigame to active
        arrangeMinigame.SetActive(true);

        //Adds the blur to minigames with added difficulty
        if (GameManager.Instance.difficulty > 1)
        {
            ppVolume.enabled = true;
            ppVolume.weight = 1;
            if (GameManager.Instance.difficulty >= 2)
            { //Scales from 2 to 11
                ppVolume.weight = 0.45f + (GameManager.Instance.difficulty * 0.05f);
            }
        }
    }


    public override void StopMinigame()
    {
        arrangeMinigame.GetComponent<DragManager>().Reset();

        //Remove the blur from minigames with added difficulty
        ppVolume.enabled = false;

        //deactivate the minigame
        arrangeMinigame.SetActive(false);
    }
}
