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

    [SerializeField]
    private GameObject dialogueSystem;

    [SerializeField]
    private GameObject checklistCanvas;

    //Bool to help keep track of which camera is active
    private bool isMainCameraActive = false;

    /// <summary>
    /// Switches between the gameplay scene and the dialogue scene
    /// </summary>
    [YarnCommand("switchCameras")]
    public void SwitchCameras()
    {
        if (!isMainCameraActive)
        {
            dialogueSystem.SetActive(false);
            cameras[1].gameObject.SetActive(false);
            cameras[0].gameObject.SetActive(true);
            checklistCanvas.SetActive(true);
        }
        else
        {
            checklistCanvas.SetActive(false);
            cameras[0].gameObject.SetActive(false);
            cameras[1].gameObject.SetActive(true);
            dialogueSystem.SetActive(true);
        }
        
        isMainCameraActive = !isMainCameraActive;
    }
}
