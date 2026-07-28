using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Contains functionality for enabling/disabling the neighbor artifact buttons
/// of an artifact
/// </summary>
public class ToggleNeighbours : ButtonUtils
{
    [SerializeField]
    public Button[] neighbours;

    [SerializeField]
    public Button self;

    [SerializeField]
    private bool disableOnStart = false;    //if enabled, calls DisableNeighbours() on start

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (disableOnStart)
        {
            DisableNeighbours();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Contains functionality for enabling/disabling the neighbor artifact buttons
    /// of an artifact
    /// </summary>
    public void DisableNeighbours()
    {
        foreach (var btn in neighbours)
        {
            DisableButton(btn);
            if(btn == self)
            {
                continue;
            }
            btn.GetComponent<Image>().raycastTarget = false;

            //externally disables disabled bool in HideArtifact/HoverButton in order to 
            //disable hover mechanics
            var hide = btn.GetComponent<HideArtifact>();
            if (hide != null)
            {
                hide.SetDisabledExternally(true);
            }
            var hover = btn.GetComponent<HoverButton>();
            if (hover != null) { 
                hover.SetDisabledExternally(true);
            }
        }
    }

    public void EnableNeighbours()
    {
        foreach (var btn in neighbours)
        {
            EnableButton(btn);
            btn.GetComponent<Image>().raycastTarget = true;

            //externally reenables disabled bool in HideArtifact/HoverButton in order to 
            //disable hover mechanics
            var hide = btn.GetComponent<HideArtifact>();
            if (hide != null) {
                hide.SetDisabledExternally(false);
            }
            var hover = btn.GetComponent<HoverButton>();
            if (hover != null)
            {
                hover.SetDisabledExternally(false);
            }
            
        }
    }
}
