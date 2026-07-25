using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuCursorOptions : MonoBehaviour
{
    //cursors
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D interactCursor;
    private Vector2 cursorHotSpot = Vector2.zero;

    //cursor methods for entering/exiting buttons/colliders (ported from GameManager)
    public void OnButtonCursorEnter()
    {
        Cursor.SetCursor(interactCursor, cursorHotSpot, CursorMode.Auto);
    }

    public void OnButtonCursorExit()
    {
        Cursor.SetCursor(defaultCursor, cursorHotSpot, CursorMode.Auto);
    }

    private void OnMouseEnter()
    {
        Cursor.SetCursor(interactCursor, cursorHotSpot, CursorMode.Auto);
    }

    private void OnMouseExit()
    {
        Cursor.SetCursor(defaultCursor, cursorHotSpot, CursorMode.Auto);
    }
}
