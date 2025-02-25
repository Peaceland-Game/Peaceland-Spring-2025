using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the Checklist script
    /// </summary>
    [SerializeField]
    private Checklist checklist;

    /// <summary>
    /// List of cameras to handle swapping "scenes"
    /// </summary>
    [SerializeField]
    private List<Camera> cameras;

    /// <summary>
    /// The dialogue system game object for the current memory
    /// </summary>
    [SerializeField]
    private GameObject dialogueSystem;

    /// <summary>
    /// The actual dialogue runner in the scene
    /// </summary>
    private DialogueRunner dialogueRunner;

    /// <summary>
    /// The canvas that holds the checklist
    /// </summary>
    [SerializeField]
    private GameObject checklistCanvas;

    /// <summary>
    /// Bool to help keep track of which camera is active
    /// </summary>
    private bool isMainCameraActive = false;

    private void Start()
    {
        dialogueRunner = dialogueSystem.GetComponent<DialogueRunner>();
    }

    /// <summary>
    /// Switches between the gameplay scene and the dialogue scene
    /// </summary>
    [YarnCommand("switchCameras")]
    public void SwitchCameras()
    {
        //If the main camera is not active (gameplay is not active, and dialogue is), then continue
        if (!isMainCameraActive)
        {
            //Turn off the dialogue section
            dialogueSystem.SetActive(false);
            cameras[1].gameObject.SetActive(false);

            //Turn on the gameplay section
            cameras[0].gameObject.SetActive(true);
            checklistCanvas.SetActive(true);
        }
        //Otherwise the dialogue camera is active, and continue below
        else
        {
            //Turn off the gameplay section
            checklistCanvas.SetActive(false);
            cameras[0].gameObject.SetActive(false);

            //Turn on the dialogue section
            cameras[1].gameObject.SetActive(true);
            dialogueSystem.SetActive(true);
        }
        
        //Then switch the camera bool to indicate the camera has finished switching
        isMainCameraActive = !isMainCameraActive;
    }
}
