using UnityEngine;
using UnityEngine.UI;

public class HideArtifact : HoverButton
{
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
    private UnlockCondition unlockCondition; // The memory that will lock the artifact until it is completed

    [SerializeField]
    private Button artifactButton;

    [SerializeField]
    private string artifactName;

    [SerializeField]
    private Image textBackground;

    [SerializeField]
    private Image[] artifactImages=null; //used for when multiple images are used for the artifact (like the collection of children's toys)


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonImage.enabled = false;
        if (buttonText != null)
        {
            buttonText.gameObject.SetActive(false);
        }
        artifactButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
            // Check if the memory has been completed
            bool unlocked = false;
            switch (unlockCondition)
            {
                case UnlockCondition.Placeholder:
                    unlocked = GameManager.Instance.placeholderCondition;
                    break;
                case UnlockCondition.CompleteFlorist:
                    unlocked = GameManager.Instance.seenFloristMemory;
                    break;
                case UnlockCondition.CompleteRJ:
                    unlocked = GameManager.Instance.seenRJMemory;
                    break;
                case UnlockCondition.CompleteChild:
                    unlocked = GameManager.Instance.seenChildMemory;
                    break;
                case UnlockCondition.CompleteVillain:
                    unlocked = GameManager.Instance.seenVillainMemory;
                    break;
                case UnlockCondition.CompleteBoris:
                    unlocked = GameManager.Instance.seenBorisMemory;
                    break;
            }
            // If the memory is not completed, hide the artifact
            if (!unlocked)
            {
                textBackground.color = new Color(0, 0, 0, 0); // Make the background transparent
                buttonText.text = "???";
                buttonText.color = Color.white;
                buttonImage.enabled = false;
                if (artifactImages != null)
                {
                    foreach (Image img in artifactImages)
                    {
                        img.color = new Color(0, 0, 0, 0.882f); // Darken images
                    }
               
                }
                artifactButton.interactable = false;
            }
            else
            {
                textBackground.color = new Color(1, 1, 1, 0.267f); // Make the background white
                buttonText.color = Color.black;
                buttonText.text = artifactName;
                artifactButton.interactable = true;

                if (artifactImages != null)
                {
                    foreach (Image img in artifactImages)
                    {
                        img.color = new Color(255, 255, 255, 1); // reveal images
                    }

                }
            }
        }
    
}
