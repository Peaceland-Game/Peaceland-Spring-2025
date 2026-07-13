using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    protected Image buttonImage; //for artifacts, this is the text background image, for other buttons, this is the button image

    [SerializeField]
    protected TextMeshProUGUI buttonText;

    private bool descOpen = false;

    [SerializeField]
    ViewDescription viewDescription;

    [SerializeField]
    ExitDescription exitDescription;

    protected bool disabled = false;

    private void Awake()
    {
        // Subscribe to the OnViewDescClicked event
        viewDescription.OnViewDescriptionClicked += HoverButton_OnViewDescClicked;
        exitDescription.OnExitDescriptionClicked += HoverButton_OnExitDescClicked;
    }


    public void SetDisabledExternally(bool value)
    {
        disabled = value;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (disabled)
        {
            Debug.Log("Hover button is disabled, not showing hover button.");
            return;
        }
        if (descOpen)
        {
            Debug.Log("Description is open, not showing hover button.");
            return;
        }
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

    private void HoverButton_OnViewDescClicked(object sender, System.EventArgs e)
    {
        descOpen = true;
        buttonImage.enabled = false;
        if (buttonText != null)
        {
            buttonText.gameObject.SetActive(false);
        }
    }

    private void HoverButton_OnExitDescClicked(object sender, System.EventArgs e)
    {
        descOpen = false;
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
