using UnityEngine;
using UnityEngine.UI;

public class ButtonUtils : MonoBehaviour
{
    public void EnableButton(Button button)
    {
        button.interactable = true;
        button.GetComponent<Image>().enabled = true;
    }

    public void DisableButton(Button button)
    {
        button.interactable = false;
        button.GetComponent<Image>().enabled = false;
    }
}
