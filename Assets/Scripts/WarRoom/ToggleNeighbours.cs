using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ToggleNeighbours : ButtonUtils
{
    [SerializeField]
    public Button[] neighbours;

    [SerializeField]
    public Button self;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisableNeighbours()
    {
        ToggleButtonArray(false, neighbours, self);
    }

    public void EnableNeighbours()
    {
        ToggleButtonArray(true, neighbours);
    }
}
