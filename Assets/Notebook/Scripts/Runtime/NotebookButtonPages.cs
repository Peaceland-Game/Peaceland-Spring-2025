using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotebookButtonPages : MonoBehaviour
{
    [SerializeField] private int buttonsPerPage = 2;
    [SerializeField] private Button buttonPrefab;       
    [SerializeField] private Button nextPageButton;     
    [SerializeField] private Button previousPageButton; 

    // Each entry is a direct child of the container that holds a button
    private readonly List<GameObject> slots = new List<GameObject>();
    private int currentPage;

    public int CurrentPage => currentPage;
    public int PageCount => Mathf.Max(1, Mathf.CeilToInt(slots.Count / (float)buttonsPerPage));

    private void Awake()
    {
        if (nextPageButton) nextPageButton.onClick.AddListener(NextPage);
        if (previousPageButton) previousPageButton.onClick.AddListener(PreviousPage);
        Rescan();
    }

    private void OnTransformChildrenChanged() => Rescan();

    public Button AddButton() => Instantiate(buttonPrefab, transform);

    public void NextPage() => ShowPage(Mathf.Min(currentPage + 1, PageCount - 1));
    public void PreviousPage() => ShowPage(currentPage - 1);

    // A page with no buttons left on it simply shows none.
    public void ShowPage(int page)
    {
        currentPage = Mathf.Max(0, page);
        Refresh();
    }

    private void Rescan()
    {
        slots.Clear();
        foreach (Transform child in transform)
        {
            // `true` makes it find buttons that are currently hidden on other pages.
            Button b = child.GetComponentInChildren<Button>(true);
            if (b == null || b == nextPageButton || b == previousPageButton) continue;
            slots.Add(child.gameObject);
        }
        Refresh();
    }

    private void Refresh()
    {
        int first = currentPage * buttonsPerPage;
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetActive(i >= first && i < first + buttonsPerPage);
        }

        if (nextPageButton) nextPageButton.interactable = currentPage < PageCount - 1;
        if (previousPageButton) previousPageButton.interactable = currentPage > 0;
    }
}