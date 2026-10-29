using UnityEngine;

[DefaultExecutionOrder(-12000)]
public sealed class MazeLayerBinding : MonoBehaviour
{
    [SerializeField] private string layerName;
    [SerializeField] private bool includeChildren = true;

    private void Awake()
    {
        Apply();
    }

    public void Configure(string value)
    {
        layerName = value;
        Apply();
    }

    private void Apply()
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            Debug.LogError("Required maze layer is missing: " + layerName, this);
            return;
        }

        gameObject.layer = layer;
        if (!includeChildren)
        {
            return;
        }

        foreach (Transform item in GetComponentsInChildren<Transform>(true))
        {
            item.gameObject.layer = layer;
        }
    }
}
