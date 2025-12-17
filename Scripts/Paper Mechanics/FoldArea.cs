using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FoldArea : MonoBehaviour
{
    [SerializeField]
    private Vector2 foldDirection;
    [SerializeField]
    private FoldController controller;
    [SerializeField]
    private Texture2D[] cursorTextures;

    private static Texture2D[] cursorTexs = null;
    private bool drag = false;

    private void Start()
    {
        if (cursorTexs == null && cursorTextures != null && cursorTextures.Length > 0)
        {
            Cursor.visible = false;
            cursorTexs = cursorTextures;
            Cursor.SetCursor(cursorTexs[2], Vector2.zero, CursorMode.Auto);
        }

    }
    private void OnMouseDown()
    {
        controller.StartFold(transform.position, foldDirection);
        Cursor.SetCursor(cursorTexs[1], Vector2.zero, CursorMode.Auto);
        drag = true;
    }

    private void OnMouseUp()
    {
        controller.CancelFold();
        Physics.SyncTransforms();
        Cursor.SetCursor(cursorTexs[0], Vector2.zero, CursorMode.Auto);
        drag = false;
    }

    private void OnMouseEnter()
    {
        if(!drag)
        Cursor.SetCursor(cursorTexs[0], Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseExit()
    {
        if(!drag)
        Cursor.SetCursor(cursorTexs[2], Vector2.zero, CursorMode.Auto);
    }
}
