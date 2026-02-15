using System.Collections;
using System.Collections.Generic;
using Folded;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class FoldArea : MonoBehaviour, IPauseEffected
{

    [SerializeField]
    private Sprite sprite;

    public Vector2 foldDirection;
    public Transform connectedPage;
    [SerializeField]
    private FoldController controller;
    [SerializeField]
    private Texture2D[] cursorTextures;

    private static Texture2D[] cursorTexs = null;
    private SpriteRenderer sr;

    [HideInInspector]
    public bool lastInteracted;

    private bool drag = false;
    private Vector2 initalPos;

    private RectTransform UIRect;
    private FoldAreaUI UIArea;

    private Vector2 uiSize = new(.03f, 0f); //Half

    private List<FoldArea> others;

    private void Awake()
    {
        Transform canvas = GameObject.FindWithTag("canvas").transform;

        UIRect = new GameObject("UI Fold Area", typeof(RectTransform)).GetComponent<RectTransform>();
        UIRect.SetParent(canvas);
        UIRect.anchoredPosition = Vector2.one * .5f;

        UIArea = UIRect.gameObject.AddComponent<FoldAreaUI>();
        UIArea.Set(this, sprite);

        UIArea.enabled = false;

        others = new(transform.parent.GetComponentsInChildren<FoldArea>());
        others.Remove(this);

        initalPos = transform.position;

    }

    private void Start()
    {
        if (cursorTexs == null && cursorTextures != null && cursorTextures.Length > 0)
        {
            Cursor.visible = false;
            cursorTexs = cursorTextures;
            Cursor.SetCursor(cursorTexs[2], Vector2.zero, CursorMode.Auto);
        }
        //sr = GetComponent<SpriteRenderer>();
        //sr.enabled = false;

        uiSize.y = uiSize.x * ((float)Screen.width / Screen.height);
        UIRect.offsetMax = Vector2.zero;
        UIRect.offsetMin = Vector2.zero;
        UIRect.anchoredPosition3D = Vector3.zero;
        UIRect.localScale = Vector3.one;

        enabled = false;

        ((IPauseEffected)this).Login();

    }

    
    private void OnDestroy()
    {
        //((IPauseEffected)this).Logout();
    }

    private void Update()
    {
        SetUIPosition();

        foreach (FoldArea foldArea in others)
            foldArea.SetUIVisibility();
    }
    public void OnPointerDown()
    {
        if(controller.AbleToFold())
        {
            bool _lastInteracted = lastInteracted;
            SetLastInteracted();
            foreach (FoldArea foldArea in others)
                foldArea.SetUIPosition();


            controller.StartFold(_lastInteracted || foldDirection.x * foldDirection.y != 0 ? transform.position : initalPos, foldDirection);
            Cursor.SetCursor(cursorTexs[1], Vector2.zero, CursorMode.Auto);
            drag = true;
            enabled = true;

            SetUIVisibility();
        }
    }

    public void OnPointerUp()
    {
        controller.CancelFold();
        Physics.SyncTransforms();
        Cursor.SetCursor(cursorTexs[0], Vector2.zero, CursorMode.Auto);
        drag = false;
        //sr.enabled = false;
        enabled = false;

        UIArea.enabled = true;
    }

    public void OnPointerEnter()
    {
        if(!drag)
        Cursor.SetCursor(cursorTexs[0], Vector2.zero, CursorMode.Auto);
        //sr.enabled = !drag && PauseMenu.isPaused;
    }

    public void OnPointerExit()
    {
        if(!drag)   
            Cursor.SetCursor(cursorTexs[2], Vector2.zero, CursorMode.Auto);
    }

    private void SetLastInteracted()
    {
        lastInteracted = true;

        foreach (FoldArea other in others)
            other.lastInteracted = false;
    }

    #region UI Functions
    public void SetUIPosition()
    {
        Vector2 pos = Camera.main.WorldToScreenPoint(lastInteracted ? transform.position : initalPos);
        pos.x /= Screen.width;
        pos.y /= Screen.height;

        UIRect.anchorMin = pos - uiSize + Vector2.Scale(uiSize, foldDirection);
        UIRect.anchorMax = pos + uiSize + Vector2.Scale(uiSize, foldDirection);
        UIRect.ForceUpdateRectTransforms();
    }
    public void SetUIVisibility()
    {
        if((lastInteracted || (Vector2.Dot(foldDirection, (Vector2)transform.position - initalPos) <= 0.1f)))
        {
            UIArea.enabled = true;
        }
        else
        {
            UIArea.enabled = false;
        }
    }
    #endregion


    #region IPause Functions
    public void Pause()
    {
        UIArea.enabled = true;
        enabled = true;
    }

    public void Resume()
    {
        UIArea.enabled = false;
        enabled = false;
    }
    #endregion



    private class FoldAreaUI : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
    {
        private static readonly Color halfWhite = new(1f,1f,1f,.2f);
        private static readonly Color fullWhite = new(1f, 1f, 1f, .8f);

        private FoldArea foldArea;
        private Image image;
        
        public void Set(FoldArea foldArea, Sprite sprite)
        {
            this.foldArea = foldArea;
            image = gameObject.AddComponent<Image>();
            image.sprite = sprite;
            enabled = false;

            image.color = halfWhite;
        }

        private void OnEnable()
        {
            if(image)
                image.enabled = true;
        }
        private void OnDisable()
        {
            image.enabled = false;
        }

        #region Pointer Functions
        public void OnPointerEnter(PointerEventData eventData)
        {
            foldArea.OnPointerEnter();
            image.color = fullWhite;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            foldArea.OnPointerDown();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foldArea.OnPointerExit();
            image.color = halfWhite;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            foldArea.OnPointerUp();
        }
        #endregion
    }
}


///<summary>Back Page struct that can be dragged through <see cref="FoldArea"/> </summary>
public struct ConnectedPage
{

}
