using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHelper
{
    public static Vector2 GetPointerPosition() {

        Vector2 pos;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            pos = Touchscreen.current.position.ReadValue();
        }
        else {
            pos = Mouse.current.position.ReadValue();
        }

        return pos;
    }

    public static Vector3 GetPointerWorldPosition() {
        Vector3 pos = Camera.main.ScreenToWorldPoint(GetPointerPosition());
        return new Vector3(pos.x, pos.y, 0.0f);
    }
}
