using UnityEngine;
using UnityEngine.UI;

public class ScrollView : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    private GameObject scrollView;

    [SerializeField]
    private GameObject content;

    [SerializeField]
    private GameObject scrollBar;

    private ScrollRect scrollRect;
    void Start()
    {
        if (content.GetComponent<RectTransform>().rect.height <= scrollView.GetComponent<RectTransform>().rect.height)
        {
            scrollRect = scrollView.GetComponent<ScrollRect>();
            scrollRect.enabled = false;
            scrollBar.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
