using UnityEngine;
using UnityEngine.UI;
using System;

public class ToggleNeighbours : ButtonUtils
{
    [SerializeField]
    public Button[] neighbours;

    [SerializeField]
    public Button self;

    [SerializeField]
    private bool disableOnStart = false;

    public event EventHandler OnDisableNeighbours;
    public event EventHandler OnEnableNeighbours;

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

    public void DisableNeighbours()
    {
        //Debug.Log("Disabling neighbours...");
        ToggleButtonArray(false, neighbours, null, false);
        foreach (var btn in neighbours)
        {
            btn.GetComponent<Image>().raycastTarget = false;
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
        //OnDisableNeighbours?.Invoke(this, EventArgs.Empty);
    }

    public void EnableNeighbours()
    {
        ToggleButtonArray(true, neighbours, null, false);
        foreach (var btn in neighbours)
        {
            btn.GetComponent<Image>().raycastTarget = true;
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
        //OnEnableNeighbours?.Invoke(this, EventArgs.Empty);
    }
}
