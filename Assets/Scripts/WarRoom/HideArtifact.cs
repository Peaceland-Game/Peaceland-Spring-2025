using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script to hide an artifact in wall view of the war room if it hasn't been unlocked yet;
/// artifact becomes no longer hidden/unlocked if its condition has already been fulfilled
/// </summary>
public class HideArtifact : HoverButton
{
    /// existing "achievements" that can be used to lock/unlock artifacts;
    /// more expected to be added over time
    public enum UnlockCondition
    {
        Placeholder,
        CompleteFlorist,
        CompleteRJ,
        CompleteChild,
        CompleteVillain,
        CompleteBoris,
    }

    [SerializeField]
    private UnlockCondition unlockCondition; // The condition that will lock the artifact until it is completed

    [SerializeField]
    private string artifactName;    //name of the artifact that will be made visible to the player once the artifact is unlocked

    [SerializeField]
    private Image[] artifactImages=null; //used for when multiple images are used for the artifact (like the collection of children's toys)

    [SerializeField]
    private bool changeColor = true;    //whether or not the artifact's color should be changed when disabled (almsot always a "yes"/true)

    private GameManager gm;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //text background (hoverImage) and artifact name are initially disabled
        hoverImage.enabled = false;
        if (buttonText != null)
        {
            buttonText.gameObject.SetActive(false);
        }
        artifactButton.interactable = false;

        //GameManager is a singleton
        //gm = GameManager.Instance; 
    }

    // Update is called once per frame
    void Update()
    {
        // artifact is completely disabled (set externally) when another artifact's description is currently open
        if (disabled)
        {
            hoverImage.color = new Color(0, 0, 0, 0); // Make the background transparent
            buttonText.text = "";
            artifactButton.interactable = false;
            return;
        }

        // Check if the condition has been completed
        bool unlocked = false;
        //switch (unlockCondition)
        //{
        //    case UnlockCondition.Placeholder:
        //        unlocked = gm.placeholderCondition;
        //        break;
        //    case UnlockCondition.CompleteFlorist:
        //        unlocked = GameManager.Instance.seenFloristMemory;
        //        break;
        //    case UnlockCondition.CompleteRJ:
        //        unlocked = GameManager.Instance.seenRJMemory;
        //        break;
        //    case UnlockCondition.CompleteChild:
        //        unlocked = GameManager.Instance.seenChildMemory;
        //        break;
        //    case UnlockCondition.CompleteVillain:
        //        unlocked = GameManager.Instance.seenVillainMemory;
        //        break;
        //    case UnlockCondition.CompleteBoris:
        //        unlocked = GameManager.Instance.seenBorisMemory;
        //        break;
        //}

        // If the condition is not completed, hide the artifact
        if (!unlocked)
        {
            hoverImage.color = new Color(0, 0, 0, 0); // Make the background transparent
            buttonText.text = "???";
            buttonText.color = Color.white;
            artifactButton.interactable = false;

            if (artifactImages != null && artifactImages.Length != 0) //when there are multiple artifact images
            {
                foreach (Image img in artifactImages)
                {
                    if(changeColor)
                    {
                        img.color = new Color(0, 0, 0, 0.882f); // Darken artifact images
                    }                    
                }
            }
            else //just one artifact image
            {
                if (changeColor)
                    artifactButton.GetComponent<Image>().color = new Color(0, 0, 0, 0.882f); // darken img
                
            }
        }
        else //if the condition is completed, make the artifact visible and interactable
        {

            hoverImage.color = new Color(1, 1, 1, 0.267f); // Make the background white
            buttonText.color = Color.black;
            buttonText.text = artifactName;
            artifactButton.interactable = true;

            if (artifactImages != null && artifactImages.Length != 0) //multiple images
            {
                foreach (Image img in artifactImages)
                {
                    if (changeColor)    
                        img.color = new Color(255, 255, 255, 1); // reveal images
                }

            }
            else //just one image
            {
                if (changeColor)
                    artifactButton.GetComponent<Image>().color = new Color(255, 255, 255, 1); // reveal img
            }
        }
    }
    
}
