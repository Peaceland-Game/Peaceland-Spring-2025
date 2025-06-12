using UnityEditor;
using UnityEngine;

public class ThornyFlowerZoom : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.transform.position = new Vector3(-3.3f, -0.3f, 1f);
        this.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = (Vector3)transform.localScale * (1.001f);
        transform.position = new Vector3(transform.position.x - (0.001f * transform.localScale.x), transform.position.y, this.transform.position.z);
        if (transform.localScale.x > 5)
        {
            Debug.Log("spawn");
            CutManager cm = new CutManager();
            cm.beginDethorn();
            this.gameObject.SetActive(false);
        }
    }
}
