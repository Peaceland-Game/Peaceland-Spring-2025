using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonUtils : MonoBehaviour
{
    public void EnableButton(Button button, bool enableImage=true)
    {
        Debug.Log("Enabling button: " + button.name);
        button.interactable = true;

        if (enableImage)
        {
            button.GetComponent<Image>().enabled = true;
            button.GetComponent<Image>().raycastTarget = true;
        }
    }

    public void DisableButton(Button button, bool disableImage = true)
    {
        Debug.Log("Disabling button: " + button.name);
        button.interactable = false;
        if (disableImage)
        {
            button.GetComponent<Image>().enabled = false;
            button.GetComponent<Image>().raycastTarget = false;
        }
    }

    public void ToggleButtonArray(bool enable, Button[] buttons, Button exempt = null, bool toggleImage=true)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (enable)
            {
                EnableButton(buttons[i], toggleImage);
            }
            else
            {
                DisableButton(buttons[i], toggleImage);

            }
        }
    }
}
