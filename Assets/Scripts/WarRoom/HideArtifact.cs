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
    private bool togglePlaceholderCondition; //boolean for when Placeholder is the unlock condition, so we can toggle it on and off for testing


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
                buttonText.text = "?";
                
                    
                artifactButton.interactable = false;
            }
            else
            {
                buttonText.text = artifactName;
                artifactButton.interactable = true;
            }
        }
    
}
