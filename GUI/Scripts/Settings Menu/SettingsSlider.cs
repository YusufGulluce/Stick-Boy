using UnityEngine;
using System.Collections;

using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class SettingsSlider : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private ChangeableValue value;
    [SerializeField]
    public RectTransform rect;
    [SerializeField]
    public RectTransform line;
    [SerializeField]
    public RectTransform boxRect;
    [SerializeField]
    public RectTransform panelRect;

    public void Set(ChangeableValue value)
    {
        this.value = value;
        SetDot(value.value);

        Debug.Log(value);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        float x = MouseToBoxX(Input.mousePosition);
        SetDot(x);
        value.value = x;
    }
    public void OnDrag(PointerEventData eventData)
    {
        float x = MouseToBoxX(Input.mousePosition);
        SetDot(x);
        value.value = x;
    }

    private float MouseToBoxX(Vector2 pos)
    {
        float x = pos.x / Screen.width;
        if(panelRect != null)
        {
            x -= panelRect.anchorMin.x;
            x /= panelRect.anchorMax.x - panelRect.anchorMin.x;
        }
        if(boxRect != null)
        {
            x -= boxRect.anchorMin.x;
            x /= boxRect.anchorMax.x - boxRect.anchorMin.x;
        }
        if(line != null)
        {
            x -= line.anchorMin.x;
            x /= line.anchorMax.x - line.anchorMin.x;
        }

        x = Mathf.Min(Mathf.Max(0f, x), 1f);
        return x;
    }

    private void SetDot(float x)
    {
        x *= 1 - boxRect.rect.height / boxRect.rect.width;
        float minX = x;
        float maxX = x + boxRect.rect.height / boxRect.rect.width;

        rect.anchorMin = new(minX, 0f);
        rect.anchorMax = new(maxX, 1f);

    }

}
