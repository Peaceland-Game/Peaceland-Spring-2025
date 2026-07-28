using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// General helper containing functions for button toggling (more can be added if needed)
/// </summary>
public class ButtonUtils : MonoBehaviour
{
    /// turns a button interactable; also can enable image as raycast target
    /// if enableImage is toggled on
    /// <param name="button">the button to enable</param>
    /// <param name="enableImage">true to allow image enabling, false otherwise</param>
    public void EnableButton(Button button, bool enableImage=true)
    {
        button.interactable = true;

        if (enableImage)
        {
            button.GetComponent<Image>().enabled = true;
            button.GetComponent<Image>().raycastTarget = true;
        }
    }

    /// turns a button non-interactable; also can disable image as raycast target
    /// if enableImage is toggled on
    /// <param name="button">the button to disable</param>
    /// <param name="enableImage">true to allow image disabling, false otherwise</param>
    public void DisableButton(Button button, bool disableImage = true)
    {
        button.interactable = false;
        if (disableImage)
        {
            button.GetComponent<Image>().enabled = false;
            button.GetComponent<Image>().raycastTarget = false;
        }
    }

    /// enables/disables an array of buttons (optionally one button can be skipped/exempt)
    /// <param name="enable">true to enable, false to disable</param>
    /// <param name="buttons">the array of buttons to toggle</param>
    /// <param name="exempt">the button to skip (leave as empty/null if non applicable)</param>
    /// <param name="toggleImage">whether or not to toggle the image (true by default)</param>
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
