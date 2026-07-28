using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookTabButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text labelText;

        public void Configure(Button targetButton, TMP_Text targetLabelText)
        {
            button = targetButton;
            labelText = targetLabelText;
        }

        public void Bind(string label, UnityAction onClick)
        {
            gameObject.SetActive(true);

            if (labelText != null)
            {
                labelText.text = label;
            }

            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
