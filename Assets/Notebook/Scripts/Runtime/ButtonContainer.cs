using UnityEngine;
using UnityEngine.UI;

public class ButtonContainer : MonoBehaviour
{
    [SerializeField] private int maxButtons = 2;
    [SerializeField] private Button buttonPrefab; // only needed if you spawn buttons from code

    public bool IsFull => CountButtons() >= maxButtons;

    // Returns the new button, or null if the container is already full.
    public Button TryAddButton()
    {
        if (IsFull)
        {
            Debug.Log($"{name} already has {maxButtons} buttons.");
            return null;
        }
        return Instantiate(buttonPrefab, transform);
    }

    // Keeps the first buttons and removes any extras.
    private void OnTransformChildrenChanged()
    {
        int count = 0;
        foreach (Transform child in transform)
        {
            if (!child.TryGetComponent<Button>(out _)) continue;

            count++;
            if (count > maxButtons)
            {
                Debug.LogWarning($"{name} can only hold {maxButtons} buttons; removing {child.name}.");
                Destroy(child.gameObject);
            }
        }
    }

    private int CountButtons()
    {
        int count = 0;
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<Button>(out _)) count++;
        }
        return count;
    }
}
