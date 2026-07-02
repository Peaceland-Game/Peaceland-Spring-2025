using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonUtils : MonoBehaviour
{
    //[SerializeField]
    //public TextMeshProUGUI buttonText;

    public void EnableButton(Button button)
    {
        button.interactable = true;
        button.GetComponent<Image>().enabled = true;
        //if (buttonText != null)
        //{
        //    buttonText.gameObject.SetActive(true);
        //}
        //TextMeshPro[] childTMPs = GetComponentsInChildren<TextMeshPro>();
        //foreach (var child in childTMPs)
        //{
        //    child.enabled = true;
        //}
    }

    public void DisableButton(Button button)
    {
        button.interactable = false;
        button.GetComponent<Image>().enabled = false;
        //if (buttonText != null)
        //{
        //    buttonText.gameObject.SetActive(false);
        //}
        //TextMeshPro[] childTMPs = GetComponentsInChildren<TextMeshPro>();
        //foreach (var child in childTMPs)
        //{
        //    child.enabled = false;
        //}
    }

    public void ToggleButtonArray(bool enable, Button[] buttons, Button exempt = null)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            //if (buttons[i] == exempt)
            //{
            //    continue;
            //}
            if (enable)
            {
                EnableButton(buttons[i]);
            }
            else
            {
                DisableButton(buttons[i]);

            }
        }
    }
}
