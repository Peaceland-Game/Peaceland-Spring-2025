using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using static OrderObject;

public class Interactmanager : MinigameBehavior
{
    [SerializeField] GameObject InteractablePrefab;

    [SerializeField] Vector3[] locations;

    [SerializeField] Vector2[] rotations;

    public Interactable[] interactables;

    private int numObjects;


    public override void StartMinigame()
    {
        numObjects = FlowerShopManager.GetCurrentOrder().flowers.Count;
        interactables = new Interactable[numObjects];

        for (int i = 0; i < numObjects; i++)
        {
            GameObject newInteractable = Instantiate(InteractablePrefab);
            interactables[i] = newInteractable.GetComponent<Interactable>();
            newInteractable.transform.parent = transform;

            interactables[i].gameObject.transform.localPosition = locations[i];

        }

        gameObject.SetActive(true);

    }
    public override void StopMinigame()
    {
        for (int i = 0; i < interactables.Length; i++)
        {
            Destroy(interactables[i]);
        }

        gameObject.SetActive(false);
    }

    public void DoClick()
    {
        
       // if (gameObject.activeInHierarchy) {
            Vector3 touch_wp = InputHelper.GetPointerWorldPosition();
        Debug.Log(interactables[1]);
            //Debug.Log("STHN");
            for (int i = 0; i < numObjects; i++)
            {
                if (!(interactables[i].transform.position.x - interactables[i].GetComponent<SpriteRenderer>().bounds.size.x/2 < touch_wp.x ||
                       interactables[i].transform.position.x + interactables[i].GetComponent<SpriteRenderer>().bounds.size.x / 2 > touch_wp.x ||
                       interactables[i].transform.position.y - interactables[i].GetComponent<SpriteRenderer>().bounds.size.y / 2 < touch_wp.y ||
                       interactables[i].transform.position.y + interactables[i].GetComponent<SpriteRenderer>().bounds.size.y / 2 > touch_wp.y)){
                    Debug.Log("CLICKED!!!!!");
                }
        //}


        }
    }
}
