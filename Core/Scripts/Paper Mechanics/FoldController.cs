using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Tilemaps;

public class FoldController : MonoBehaviour
{
    public static List<FoldController> pages = new();

    [SerializeField]
    private SpriteMask spriteMask;
    [SerializeField]
    private Transform frontPage;
    [SerializeField]
    private Transform backPage;
    [SerializeField]
    private Transform pageMask;

    private Transform playerTr;
    [SerializeField]
    private BoxCollider backCol;
    [SerializeField]
    private Collider maskCol;
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private LayerMask obstacleLayer;
    private bool playerInFront;


    [Header("Collision Holder")]
    [SerializeField]
    private MeshCollider mc;
    [SerializeField]
    private Vector3 depth;
    private List<Vector3> verts = new();
    private List<int> tris = new();


    private Vector2 foldPointStart;
    private Vector2 pageCenterStart;
    private float startAngle;
    private float startDistance;
    private Vector2 foldVector;
    private float foldableLength;
    public float foldableDeadzone;

    private bool folding;
    private bool canExitEdit;

    //Scale Variables
    private Vector3 pageScale;
    private Vector3 pageLossyScale;

    [SerializeField]
    private Vector2 foldDirection;

    private int pageIndex;

    private delegate Vector2 FoldPosCalculator(Vector2 pos);

    public void EditMode(bool mode)
    {
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
    }

    private void Start()
    {
        playerTr = Player.main.transform;

        pageIndex = pages.Count;
        Debug.Log(pageIndex);
        pages.Add(this);
        foldDirection.Normalize();

        folding = false;

        pageScale = backCol.transform.localScale;
        pageLossyScale = backCol.transform.lossyScale;

        pageCenterStart = backPage.position;
        foldableLength = pageScale.x - foldableDeadzone;

        if (mc.sharedMesh == null)
            mc.sharedMesh = new();

    }
    private void OnDestroy()
    {
        pages?.RemoveRange(0, pages.Count);
    }

    public void StartFold(Vector3 position, Vector2 vector)
    {
        if(enabled)
        {
            mc.sharedMesh.Clear();
            verts.Clear();
            tris.Clear();

            foldPointStart = position;
            foldVector = vector;

            Transform temp = new GameObject().transform;
            temp.position = foldPointStart;
            temp.SetParent(backCol.transform, true);
            foldPointStart = (Vector2)temp.localPosition * pageScale.y;
            foldPointStart.x *= pageScale.x / pageScale.y;
            foldPointStart += pageCenterStart;
            Destroy(temp.gameObject);
            folding = true;

            foldPointStart.x = pageScale.x * foldDirection.x * .5f + pageCenterStart.x;

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
            PauseMenu.SetResume(count <= 0);

            InvokeFoldEffecteds();
            PlaceCollisionHolders();
            if(verts.Count > 0)
            {
                mc.sharedMesh.SetVertices(verts);
                mc.sharedMesh.SetTriangles(tris, 0);
                mc.sharedMesh = mc.sharedMesh;
            }
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
        PauseMenu.SetResume(count <= 0);
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

        tr.position = new (point.x, point.y, tr.position.z);
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

    private void PlaceCollisionHolders()
    {
        float cos = backPage.transform.rotation.eulerAngles.z;
        float sin = Mathf.Sin(cos * Mathf.PI / 180f);
        cos = Mathf.Cos(cos * Mathf.PI / 180f);
        float x = pageLossyScale.x / 2;
        float y = pageLossyScale.y / 2;

        Vector3[] corners =
            {
            (Vector2)backPage.transform.position + new Vector2(x * cos - y * sin, x * sin + y * cos),
            (Vector2)backPage.transform.position + new Vector2(-x * cos - y * sin, -x * sin + y * cos),
            (Vector2)backPage.transform.position + new Vector2(-x * cos + y * sin, -x * sin - y * cos),
            (Vector2)backPage.transform.position + new Vector2(x * cos + y * sin, x * sin - y * cos)
            };

        List<RaycastHit> enters;
        List<RaycastHit> exits;

        Ray ray = new()
        {
            direction = corners[^1],
            origin = corners[^1]
        };
        for (int i = 0; i < 4; ++i)
        {
            float length = (ray.origin - corners[i]).magnitude;
            ray.direction = ray.origin - corners[i];
            ray.origin = corners[i];

            //Debug.Log(length);
            enters = new(Physics.RaycastAll(ray, length, obstacleLayer));
            exits = new(Physics.RaycastAll(new(ray.origin + ray.direction * length, -ray.direction), length, obstacleLayer));


            foreach (RaycastHit enterRH in enters)
            {
                bool pairMatched = false;
                foreach(RaycastHit exitRH in exits)
                {
                    if(exitRH.collider == enterRH.collider)
                    {
                        PlaceCollisionHolder(enterRH.point, exitRH.point);

                        pairMatched = true;
                        exits.Remove(exitRH);
                        break;
                    }

                }
                if(!pairMatched)
                {
                    PlaceCollisionHolder(enterRH.point, ray.origin + ray.direction * length);
                }
            }
            foreach (RaycastHit exitRH in exits)
            {
                PlaceCollisionHolder( ray.origin, exitRH.point);
            }
        }
    }


    private void PlaceCollisionHolder(params Vector3[] points)
    {
        Vector3 temp = Vector3.Cross(points[0] - points[1], Vector3.forward);
        if (Vector2.Dot(temp, maskCol.bounds.center - temp) < 0)
        {
            temp = points[0];
            points[0] = points[1];
            points[1] = temp;
        }

        verts.AddRange(new Vector3[] { points[0] - depth, points[0] + depth, points[1] - depth, points[1] + depth });
        tris.AddRange(new int[] { verts.Count - 4, verts.Count - 3, verts.Count - 1, verts.Count - 4, verts.Count - 1, verts.Count - 2});

    }

    private void InvokeFoldEffecteds()
    {
        // 2. Calculate the center in world space (handles offset colliders)
        Vector3 center = backCol.transform.TransformPoint(backCol.center);

        // 3. Calculate half-extents based on local size and global scale
        // We use Abs() to handle cases where the object might have negative scale
        Vector3 size = Vector3.Scale(backCol.size, backCol.transform.lossyScale);
        Vector3 halfExtents = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z)) * 0.5f;

        // 4. Perform the OverlapBox using the object's actual rotation
        Collider[] colliders = Physics.OverlapBox(center, halfExtents, backCol.transform.rotation);

        List<Folded.IFoldEffected> effecteds = new();

        foreach (Collider c in colliders)
            if(c.GetComponent<Folded.IFoldEffected>() != null)
            {
                effecteds.Add(c.GetComponent<Folded.IFoldEffected>());
                Debug.Log(c.name);
            }


        Folded.IFoldEffected.ChangeFolds(effecteds, pageIndex);
    }
}




namespace Folded
{
    public interface IFoldEffected
    {
        public static List<IFoldEffected>[] onFolds = {new(), new() };
        public static void FoldOffAll(int page)
        {
            if(onFolds[page] != null)
                while(onFolds[page].Count > 0)
                {
                    onFolds[page][0].FoldedOff();
                    onFolds[page].RemoveAt(0);
                }
        }
        public static void ChangeFolds(List<IFoldEffected> newFoldList, int page)
        {
            // 1. Safety Checks: Ensure lists are not null
            if (onFolds[page] == null) onFolds[page] = new List<IFoldEffected>();
            newFoldList ??= new List<IFoldEffected>(); // Treat null input as an empty list

            var currentList = onFolds[page];

            // 2. Identify items to Remove:
            // Everything currently in the list that is NOT in the new input
            var itemsToRemove = currentList.Except(newFoldList).ToList();

            // 3. Identify items to Add:
            // Everything in the new input that is NOT currently in the list
            var itemsToAdd = newFoldList.Except(currentList).ToList();

            // 4. Process Removals (Turn Off and Remove)
            foreach (var item in itemsToRemove)
            {
                item.FoldedOff();
                // We remove from the actual list logic below, 
                // but calling FoldedOff here handles the behavior.
            }

            // 5. Process Additions (Turn On)
            foreach (var item in itemsToAdd)
            {
                item.FoldedOn();
            }

            // 6. Update the State
            // Assign the new list directly to ensure order and contents match exactly.
            onFolds[page] = new List<IFoldEffected>(newFoldList);
        }

        public void FoldApply(int page)
        {
            if (!onFolds[page].Contains(this))
            {
                onFolds[page].Add(this);
                FoldedOn();
            }
        }

        public void FoldedOn();
        public void FoldedOff();
    }
}