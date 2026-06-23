using UnityEngine;
using UnityEngine.UI;

public class IrregularClickable : MonoBehaviour
{
    public float alphaThreshold = 0.9f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
