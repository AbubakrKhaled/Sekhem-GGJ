using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    // drag your cursor image here in the inspector
    public Texture2D cursorTexture;

    // the 'hotspot' is the exact pixel that clicks
    // for a pointer, this is usually (0, 0) (top-left)
    // for a crosshair, this is usually half the width/height (center)
    public Vector2 hotSpot = Vector2.zero;

    void Start()
    {
        // changing the cursor is just one line of code
        // cursormode.auto lets the operating system handle it (smoothest)
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }
}