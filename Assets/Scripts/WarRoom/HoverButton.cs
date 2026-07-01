using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    protected Image buttonImage;

    [SerializeField]
    protected TextMeshProUGUI buttonText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.enabled = true;
        if (buttonText != null)
        {
            buttonText.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.enabled = false;
        if (buttonText != null)
        {
            buttonText.gameObject.SetActive(false);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonImage.enabled = false;
        if (buttonText != null)
        {
            buttonText.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
