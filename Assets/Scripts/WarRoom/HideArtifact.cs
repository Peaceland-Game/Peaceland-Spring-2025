using UnityEngine;
using UnityEngine.UI;

public class HideArtifact : HoverButton
{
    public enum MemoryName
    {
        None=0,
        Florist =1,
        RJ =2,
        Child = 3,
        Villain = 4,
        Boris=5,
    }

    [SerializeField]
    private MemoryName lockingMemory; // The memory that will lock the artifact until it is completed

    [SerializeField]
    private Button artifactButton;

    [SerializeField]
    private string artifactName;


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
        if (lockingMemory != MemoryName.None)
        {
            // Check if the memory has been completed
            bool memoryCompleted = false;
            switch (lockingMemory)
            {
                case MemoryName.Florist:
                    memoryCompleted = GameManager.Instance.seenFloristMemory;
                    break;
                case MemoryName.RJ:
                    memoryCompleted = GameManager.Instance.seenRJMemory;
                    break;
                case MemoryName.Child:
                    memoryCompleted = GameManager.Instance.seenChildMemory;
                    break;
                case MemoryName.Villain:
                    memoryCompleted = GameManager.Instance.seenVillainMemory;
                    break;
                case MemoryName.Boris:
                    memoryCompleted = GameManager.Instance.seenBorisMemory;
                    break;
            }
            // If the memory is not completed, hide the artifact
            if (!memoryCompleted)
            {
                if (lockingMemory == MemoryName.RJ){
                    buttonText.text = "Unlocked after completing Romeo & Juliet memory";
                }
                else
                {
                    buttonText.text = "Unlocked after completing " + lockingMemory.ToString() + " memory";
                }
                    
                artifactButton.interactable = false;
            }
            else
            {
                buttonText.text = artifactName;
                artifactButton.interactable = true;
            }
        }
    }
}
