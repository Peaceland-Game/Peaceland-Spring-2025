using UnityEngine;

public class GuideLine : MonoBehaviour
{
    /// <summary>
    /// run any visual effects we want the guide line to do, then destroy it
    /// </summary>
    public void DestroySequence()
    {
        Destroy(gameObject);
    }
}
