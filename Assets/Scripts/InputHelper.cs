using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHelper
{
    public static Vector2 GetPointerPosition() {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        if (Touchscreen.current != null)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }
        return Vector2.zero;
    }

    public static Vector3 GetPointerWorldPosition() {
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null) return Vector3.zero;
        Vector3 pos = cam.ScreenToWorldPoint(GetPointerPosition());
        return new Vector3(pos.x, pos.y, 0.0f);
    }
}
