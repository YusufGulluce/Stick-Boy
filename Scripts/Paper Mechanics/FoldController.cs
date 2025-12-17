using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class FoldController : MonoBehaviour
{
    //public static FoldController main;
    public static List<FoldController> pages = new();

    [SerializeField]
    private SpriteMask spriteMask;
    [SerializeField]
    private Transform frontPage;
    [SerializeField]
    private Transform backPage;
    [SerializeField]
    private Transform pageMask;

    [SerializeField]
    private Transform playerTr;
    [SerializeField]
    private Collider backCol;
    [SerializeField]
    private Collider maskCol;
    [SerializeField]
    private Collider keyCol;
    [SerializeField]
    private SpriteRenderer keySR;
    [SerializeField]
    private LayerMask playerLayer;
    private bool playerInFront;





    private Vector2 foldPointStart;
    private Vector2 pageCenterStart;
    private float startAngle;
    private float startDistance;
    private Vector2 foldVector;
    private float foldableLength;

    private bool folding;
    private bool canExitEdit;


    [SerializeField]
    private Vector2 foldDirection;

    private delegate Vector2 FoldPosCalculator(Vector2 pos);

    public void EditMode(bool mode)
    {
        //if (canExitEdit || mode)
        //{
        if (!mode)
            foreach (CollisionMask c in CollisionMask.all)
                c.UpdateArea();

        if (mode)
            CheckPlayerPage();
        else
        {
            playerTr.SetParent(null);
            spriteMask.enabled = false;
            PauseMenu.SetResume(false);
            enabled = false;
        }
        //}
    }

    private void Start()
    {
        pages.Add(this);
        foldDirection.Normalize();

        folding = false;

        pageCenterStart = backPage.position;
        foldableLength = backPage.localScale.x;
    }
    private void OnDestroy()
    {
        if (pages != null)
            pages.RemoveRange(0, pages.Count);
    }

    public void StartFold(Vector3 position, Vector2 vector)
    {
        if(enabled)
        {
            foldPointStart = position;
            foldVector = vector;

            Transform temp = new GameObject().transform;
            temp.position = foldPointStart;
            temp.SetParent(backPage, true);
            foldPointStart = (Vector2)temp.localPosition * frontPage.localScale.y;
            foldPointStart.x *= frontPage.localScale.x / frontPage.localScale.y;
            foldPointStart += pageCenterStart;
            Destroy(temp.gameObject);
            folding = true;

            foldPointStart.x = backPage.localScale.x * foldDirection.x * .5f + pageCenterStart.x;

            Vector2 vect = pageCenterStart - foldPointStart;
            startAngle = Vector2.SignedAngle(foldDirection, vect);
            startDistance = vect.magnitude;
        }
    }

    public void CancelFold()
    {
        if(enabled)
        {
            folding = false;

            int count;
            if (playerInFront)
                count = CheckPlayerIn(backCol);
            else
                count = CheckPlayerIn(maskCol);

            if (count > 0)
                PauseMenu.SetResume(false);
            else
                PauseMenu.SetResume(true);

            keyCol.enabled = CheckIn(backCol, keyCol.transform) <= 0;
            Color c = keySR.color;
            if (keyCol.enabled)
                c.a = 1f;
            else
                c.a = .5f;
            keySR.color = c;
        }
    }

    private void Update()
    {
        if (folding) Folding();
    }

    private void Folding()
    {
        Vector2 point0 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        point0 = ClosestPoint(point0);

        SetBack(point0, backPage);
        SetMask(point0, pageMask);

        int count;
        if (playerInFront)
        {
            count = CheckPlayerIn(backCol);
        }
        else
        {
            count = CheckPlayerIn(maskCol);
            //Debug.Log("its mask");
        }

        if (count > 0)
        {
            //Debug.Log("count: " + count);
            PauseMenu.SetResume(false);
        }
        else
            PauseMenu.SetResume(true);
    }

    private void SetBack(Vector2 point, Transform tr)
    {
        Vector2 currFoldPoint = point;
        currFoldPoint -= foldPointStart;
        float angle = Vector2.SignedAngle(Vector2.left, currFoldPoint) * 2;
        tr.rotation = Quaternion.Euler(0f, 0f, angle);

        angle += startAngle;
        angle += (foldDirection.x - 1f) * 90f;
        point += new Vector2(Mathf.Cos(angle * Mathf.PI / 180f), Mathf.Sin(angle * Mathf.PI / 180f)) * startDistance;

        tr.position = point;
    }

    private void SetMask(Vector2 point_,  Transform tr)
    {
        float d = (point_ - foldPointStart).magnitude;

        Vector2 currFoldPoint = point_;
        currFoldPoint -= foldPointStart;
        if (currFoldPoint == Vector2.zero)
            currFoldPoint = foldDirection;
        float angle = Vector2.SignedAngle(Vector2.left, currFoldPoint);
        tr.rotation = Quaternion.Euler(0f, 0f, angle);

        float dX = d / (2 * Mathf.Cos(angle * Mathf.PI / 180f));
        Vector2 point = foldPointStart + Vector2.left * dX;

        angle += startAngle;
        angle -= 180f;

        point += new Vector2(Mathf.Cos(angle * Mathf.PI / 180f), Mathf.Sin(angle * Mathf.PI / 180f)) * startDistance;

        tr.position = point;
    }

    private Vector2 ClosestPoint(Vector2 point)
    {
        float cosa = Vector2.Dot((point - foldPointStart).normalized, foldVector.normalized);
        Vector2 ret = foldPointStart + Mathf.Min((point - foldPointStart).magnitude, Mathf.Abs(foldableLength / foldVector.normalized.x)) * cosa * foldVector.normalized;

        if (Vector2.Dot(ret - foldPointStart, foldVector) <= 0)
            return foldPointStart;
        return ret;
    }

    //private int CheckPlayerIn(Collider2D col)
    //{
    //    Physics.SyncTransforms();
    //    int count = 0;
    //    for (int i = 0; i < 4; ++i)
    //    {
    //        Vector2 offset = Vector2.up * ((i / 2) * 2 - 1) + Vector2.right * ((i % 2) * 2 - 1);
    //        if (col.OverlapPoint((Vector2)playerTr.position + offset))
    //            count++;
    //    }
    //    return count;
    //}
    private int CheckPlayerIn(Collider col)
    {
        Physics.SyncTransforms();
        int count = 0;
        for (int i = 0; i < 4; ++i)
        {
            Vector2 offset = Vector2.up * ((i / 2) * 2 - 1) + Vector2.right * ((i % 2) * 2 - 1);
            if(col.ClosestPoint(playerTr.position + (Vector3)offset) == playerTr.position + (Vector3)offset)
                count++;
        }
        return count;
    }
    private int CheckIn(Collider bigCol, Transform smlCol)
    {
        Physics.SyncTransforms();
        int count = 0;
        for (int i = 0; i < 4; ++i)
        {
            Vector2 offset = ((i / 2) * 2 - 1) * smlCol.localScale.y * .5f * Vector2.up + ((i % 2) * 2 - 1) * smlCol.localScale.x * .5f * Vector2.right;
            if (bigCol.ClosestPoint(smlCol.position + (Vector3)offset) == smlCol.position + (Vector3)offset)
                count++;
        }
        return count;
    }

    private void CheckPlayerPage()
    {
        Physics.SyncTransforms();

        int count = CheckPlayerIn(backCol);
        if (count <= 0)
            PlayerInFront();
        else if (count < 4)
            PlayerInMid();
        else
            PlayerInBack();
        
    }

    private void PlayerInFront()
    {
        spriteMask.enabled = true;
        playerInFront = true;
        enabled = true;
        PauseMenu.SetResume(true);
    }

    private void PlayerInMid()
    {
        spriteMask.enabled = false;
        enabled = false;
        PauseMenu.SetResume(true);
    }

    private void PlayerInBack()
    {
        spriteMask.enabled = false;
        playerInFront = false;
        playerTr.SetParent(backPage);
        enabled = true;
        PauseMenu.SetResume(true);
    }
}
