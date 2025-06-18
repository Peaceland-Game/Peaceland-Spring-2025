using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ThornyFlowerZoom : MonoBehaviour
{
    private float zoomDelay = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Reset();
    }

    /// <summary>
    /// Resets the thorny rose so it can zoom in again 
    /// </summary>
    public void Reset()
    {
        this.transform.position = new Vector3(0f, -0.3f, 1f);
        this.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
        this.gameObject.SetActive(true);
        zoomDelay = 1f;
    }

    /// <summary>
    /// Pauses for a second, then grows the flower sprite to look like a zoom-in, then 
    /// activates the dethorning minigame and disables this object.
    /// </summary>
    void Update()
    {
        if (zoomDelay < 0f)
            {
            transform.localScale = (Vector3)transform.localScale * (1.001f);
            transform.position = new Vector3(transform.position.x - (0.0016f * transform.localScale.x), transform.position.y, this.transform.position.z);
            if (transform.localScale.x > 5)
            {
                Debug.Log("spawn");
                CutManager cm = gameObject.AddComponent(typeof(CutManager)) as CutManager;
                cm.beginDethorn();
                this.gameObject.SetActive(false);
            }
        }
        else {
            zoomDelay -= Time.deltaTime;
        }

    }
}
