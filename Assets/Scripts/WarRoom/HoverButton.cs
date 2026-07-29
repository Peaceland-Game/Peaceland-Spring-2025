using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This class provides functionality for buttons that become visible only upon being hovered over;
/// can be modified to work as a general class, but currently just works for artifact buttons
/// performed.
public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    protected Image hoverImage; //for artifacts, this is the text background image, for other buttons, this is the button image

    [SerializeField]
    protected TextMeshProUGUI buttonText;   

    private bool descOpen = false;

    [SerializeField]
    public Button artifactButton;
    
    ViewDescription viewDescription;

    [SerializeField]
    ExitDescription exitDescription;

    protected bool disabled = false;    //can be set externally to prevent hovering while in description view mode

    private void Awake()
    {
        artifactButton.enabled = true;
        viewDescription = artifactButton.GetComponent<ViewDescription>();

        // Subscribe to the OnViewDescClicked event
        viewDescription.OnViewDescriptionClicked += HoverButton_OnViewDescClicked;
        exitDescription.OnExitDescriptionClicked += HoverButton_OnExitDescClicked;
        hoverImage.enabled = false;
        buttonText.gameObject.SetActive(false);
    }

    /// <summary>
    /// called by ToggleNeighbours to prevent hover button from working while
    /// the artifact is meant to be disabled
    /// </summary>
    public void SetDisabledExternally(bool value)
    {
        disabled = value;
    }

    /// <summary>
    /// unless hoverbutton is intentionally disabled, enables button image and
    /// text as soon as pointer enters object
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //returns immediately if descOpen or disabled
        if (disabled || descOpen)
        {
            return;
        }
        
        //enables image and text
        hoverImage.enabled = true;
        if (buttonText != null)
        {
            buttonText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// disables button image and text as soon as pointer exits object
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        hoverImage.enabled = false;
        if (buttonText != null)
        {
            buttonText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// disables image and button when in desc view; 
    /// sets descOpen to true
    /// </summary>
    private void HoverButton_OnViewDescClicked(object sender, System.EventArgs e)
    {
        descOpen = true;
        hoverImage.enabled = false;
        if (buttonText != null)
        {
            buttonText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// upon exiting description view, set descOpen
    /// to false
    /// </summary>
    private void HoverButton_OnExitDescClicked(object sender, System.EventArgs e)
    {
        descOpen = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hoverImage.enabled = false;
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
