using System.Collections.Generic;
using UnityEngine;

public class HouseRandomizer : MonoBehaviour
{

    //variables
    [SerializeField]
    public Sprite[] spriteOptions; //an array to hold the sprites
    [SerializeField]
    public Transform cameraTransform;   // Main Camera

    private float spawnTriggerDistance = 15f; // How far ahead of the camera to spawn sprites
    private float destroyDistance = 15f;      // How far behind the camera to delete sprites
    private float spacingBetweenSprites = 7; // Distance between each spawned sprite

    private float nextSpawnX = 0f;      // The X position where the next sprite will spawn
    private List<GameObject> activeSprites = new List<GameObject>(); //A list to hold the rendered sprite objects

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //load a couple sprites in right away at the start
        for (int i = 0; i < 4; i++)
        {
            SpawnSprite();
        }

    }

    // Update is called once per frame
    void Update()
    {
        // Create new sprites if needed
        if (cameraTransform.position.x + spawnTriggerDistance > nextSpawnX)
        {
            SpawnSprite();
        }

        // Clean up old sprites 
        CleanupOldSprites();
    }

    void SpawnSprite()
    {
        if (spriteOptions == null || spriteOptions.Length == 0) return; //make sure there actually are items in the array

        // Create a brand new GameObject for the sprite
        GameObject newSpriteObj = new GameObject("SpawnedSprite");

        // Set its position along the X axis
        newSpriteObj.transform.position = new Vector3(nextSpawnX, transform.position.y, transform.position.z);

        // Add a SpriteRenderer and assign a random sprite to it
        SpriteRenderer spriteRenderer = newSpriteObj.AddComponent<SpriteRenderer>();
        int randomIndex = Random.Range(0, spriteOptions.Length);
        spriteRenderer.sprite = spriteOptions[randomIndex];
        spriteRenderer.sortingOrder = -2;

        //manually adjust them to spawn higher on the screen
        newSpriteObj.transform.position = new Vector3(nextSpawnX, 6.55f, transform.position.z);

        // Make it a child of this parent object to keep the Hierarchy clean
        newSpriteObj.transform.SetParent(transform);

        //Scale it down lsightly
        newSpriteObj.transform.localScale = new Vector3(.8f, .8f, .8f);

        // Track the object and update the next X position
        activeSprites.Add(newSpriteObj);
        nextSpawnX += spacingBetweenSprites;
    }

    void CleanupOldSprites()
    {
        // Check if the oldest active sprite is too far to the left of the camera
        if (activeSprites.Count > 0 && activeSprites[0].transform.position.x < cameraTransform.position.x - destroyDistance)
        {
            GameObject objToDestroy = activeSprites[0];
            activeSprites.RemoveAt(0);
            Destroy(objToDestroy);
        }
    }

    public void ResetHouses()
    {
        //delete all old houses
        foreach (GameObject obj in activeSprites)
        {
            Destroy(obj);
        }
        activeSprites.Clear();

        //reset spawn position
        nextSpawnX = 0f;

        //spawn initial four houses so you don't start ona  screen with no houses
        for (int i = 0; i < 4; i++)
        {
            SpawnSprite();
        }

    }
}
