using UnityEngine;
//using System.Collections;
using System.Linq;
using System.Collections.Generic;
//using UnityEditor.Tilemaps;

public class FoldController : MonoBehaviour
{
    #region static variables

    ///<summary>All FoldControllers</summary>
    public static List<FoldController> pages = new();
    ///<summary>In pause mode, indicates where player start. (Front page, Back page etc.)</summary>
    private static PlayerPlace playerPlace = PlayerPlace.Front;

    ///<summary>Inorder for camera follow class to use, borders of playground.</summary>
    public static Vector2 minBorder;
    public static Vector2 maxBorder;
    #endregion

    #region general variables
    ///<summary>Indicates the index of this controller on <see cref="pages"/> list.</summary>
    private int pageIndex;


    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private LayerMask obstacleLayer;

    #endregion

    #region Components and objects

    [Header("Components & Objects")]

    [SerializeField]
    private Transform frontPage;
    [SerializeField]
    private Transform backPage;

    [Space]

    [SerializeField, Tooltip("Sprite mask that hides player in edit mode. \n This component is attached to back page.")]
    private SpriteMask spriteMask;
    [SerializeField, Tooltip("Sprite mask that hides back page when folding.")]
    private Transform pageMask;

    [SerializeField, Tooltip("Collider of back page.")]
    private BoxCollider backCol;
    [SerializeField, Tooltip("Collider of front page.")]
    private BoxCollider frontCol;

    [Tooltip("Collider of sprite mask that hides back page when folding.")]
    public Collider maskCol;

    [SerializeField, Tooltip("Mesh Collider of collision holder that replace obstacles when page is folded into obstacles.")]
    private MeshCollider mc;

    private Transform playerTr;
    #endregion

    #region Folding Parameters
    [Tooltip("General folding direction of page. \n(Example: (1,0) for left page.)")]
    public Vector2 foldDirection;
    [Tooltip("Page doesnt fold more when its this away from mid page.")]
    public float foldableDeadzone;

    ///<summary>Local scale of pages.</summary>
    private Vector3 pageScale;
    ///<summary>World scale of pages.</summary>
    private Vector3 pageLossyScale;

    #endregion

    #region Folding Variables
    ///<summary>Offset from mouse fold start point to folding area.</summary>
    private Vector2 draggingOffset; //To get smoother dragging and not teleporting.
    ///<summary>Position of folding area when it is started to fold.</summary>
    private Vector2 foldPointStart;
    ///<summary>Initial position of back page to remember.</summary>
    private Vector2 pageCenterStart;
    ///<summary>Vector of folding when it is started to fold.</summary>
    private Vector2 foldVector;

    //Calculation variables for optimization.
    private float startAngle;
    private float startDistance;
    private float foldableLength;

    private bool folding;
    private bool canResume;

    #endregion

    #region Collision Holder Parameters;

    [SerializeField, Tooltip("The z size of replaced obstacles.")]
    private Vector3 depth;

    #endregion

    #region Collision Holder Variables;

    ///<summary>Vertices of collision holder mesh. (For optimization.)</summary>
    private List<Vector3> verts = new();
    ///<summary>Triangles of collision holder mesh. (For optimization.)</summary>
    private List<int> tris = new();

    #endregion

    #region delegates

    private delegate Vector2 FoldPosCalculator(Vector2 pos);

    #endregion

    #region Behaviour Functions

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

        Vector2 _dir = foldDirection;
        _dir.Scale(pageScale);

        foldableLength = _dir.magnitude - foldableDeadzone;

        if (mc.sharedMesh == null)
            mc.sharedMesh = new();

        UpdateBorders();

    }
    private void Update()
    {
        if (folding) Folding();
    }

    private void OnDestroy()
    {
        pages?.RemoveRange(0, pages.Count);
    }
    #endregion

    #region Folding Functions

    //Edit Mode
    public void EditMode(bool mode)
    {
        if (!mode)
            foreach (CollisionMask c in CollisionMask.all)
                c.UpdateArea();

        if (mode)
            CheckPlayerPlace();
        else
        {
            playerTr.SetParent(null);
            spriteMask.enabled = false;
            PauseMenu.SetResume(false);
            enabled = false;
        }
    }

    //Main Folding 
    public void StartFold(Vector3 position, Vector2 vector)
    {
        if (enabled)
        {
            mc.sharedMesh.Clear();
            verts.Clear();
            tris.Clear();

            draggingOffset = position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

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

            //foldPointStart.x = pageScale.x * foldDirection.x * .5f + pageCenterStart.x;
            if (foldDirection.x != 0)
                foldPointStart.x = pageScale.x * foldDirection.x * .5f + pageCenterStart.x;
            else if (foldDirection.y != 0)
                foldPointStart.y = pageScale.y * foldDirection.y * .5f + pageCenterStart.y;

            Vector2 vect = pageCenterStart - foldPointStart;
            startAngle = Vector2.SignedAngle(foldDirection, vect);
            startDistance = vect.magnitude;

        }
    }
    private void Folding()
    {
        Vector2 point0 = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) + draggingOffset;
        point0 = ClosestPoint(point0);

        SetBack(point0, backPage);
        SetMask(point0, pageMask);

        int count = 0;
        if (playerPlace == PlayerPlace.Front)
        {
            foreach (FoldController page in pages)
                if (count <= 0) count = page.CheckPlayerIn(page.frontCol);
            if (count >= 4)
            {
                count = 0;
                foreach (FoldController page in pages)
                    if (count <= 0) count = page.CheckPlayerIn(page.backCol);
                if (count <= 0)
                {
                    count = 0;
                    foreach (FoldController page in pages)
                        if (count <= 0) count = page.CheckPlayerIn(page.maskCol);
                }
            }
            else
                count = 4;
        }
        else if (playerPlace == PlayerPlace.Back)
        {
            foreach (FoldController page in pages)
                if (count <= 0) count = page.CheckPlayerIn(page.maskCol);
        }
        else if (playerPlace == PlayerPlace.Ground)
        {
            foreach (FoldController page in pages)
                if (count <= 0) count = page.CheckPlayerIn(page.maskCol);
            count = count >= 4 ? 0 : 4;
        }
        PauseMenu.SetResume(count <= 0);
    }
    public void CancelFold()
    {
        if (enabled)
        {
            folding = false;

            int count = 0;
            if (playerPlace == PlayerPlace.Front)
            {
                foreach (FoldController page in pages)
                    if (count <= 0) count = page.CheckPlayerIn(page.backCol);
            }
            else if (playerPlace == PlayerPlace.Back)
            {
                foreach (FoldController page in pages)
                    if (count <= 0) count = page.CheckPlayerIn(page.maskCol);
            }
            else if (playerPlace == PlayerPlace.Ground)
            {
                foreach (FoldController page in pages)
                    if (count <= 0) count = page.CheckPlayerIn(page.maskCol);
                count = count >= 4 ? 0 : 4;
            }
            PauseMenu.SetResume(count <= 0);
            UpdateBorders();

            InvokeFoldEffecteds();
            PlaceCollisionHolders();
            if (verts.Count > 0)
            {
                mc.sharedMesh.SetVertices(verts);
                mc.sharedMesh.SetTriangles(tris, 0);
                mc.sharedMesh = mc.sharedMesh;
            }
        }
    }

    //Page Position Settlement
    private void SetBack(Vector2 point, Transform tr)
    {
        Vector2 currFoldPoint = point;
        currFoldPoint -= foldPointStart;

        float angle = Vector2.SignedAngle(foldDirection, currFoldPoint) * 2;

        tr.rotation = Quaternion.Euler(0f, 0f, angle);

        angle += startAngle;
        angle += Vector2.SignedAngle(Vector2.right, foldDirection);
        point += new Vector2(Mathf.Cos(angle * Mathf.PI / 180f), Mathf.Sin(angle * Mathf.PI / 180f)) * startDistance;

        tr.position = new(point.x, point.y, tr.position.z);
    }
    private void SetMask(Vector2 point_, Transform tr)
    {
        float d = (point_ - foldPointStart).magnitude;

        Vector2 currFoldPoint = point_;
        currFoldPoint -= foldPointStart;
        if (currFoldPoint == Vector2.zero)
            currFoldPoint = foldDirection;

        Vector2 _dir = foldDirection;
        _dir.Scale(_dir);

        float angle = Vector2.SignedAngle(-_dir, currFoldPoint);
        tr.rotation = Quaternion.Euler(0f, 0f, angle);


        float dX = d / (2 * Mathf.Cos(angle * Mathf.PI / 180f));
        //dX = 0f;
        Vector2 point = foldPointStart - _dir * dX;

        angle += startAngle;
        angle -= 180f;
        point += (Vector2)(Quaternion.Euler(0f, 0f, angle) * _dir * startDistance);
        //point += new Vector2(-Mathf.Sin(angle * Mathf.PI / 180f), Mathf.Cos(angle * Mathf.PI / 180f)) * startDistance;

        tr.position = point;
    }

    //Calculation Functions
    private Vector2 ClosestPoint(Vector2 point)
    {
        float cosa = Vector2.Dot((point - foldPointStart).normalized, foldVector.normalized);

        float divisor = foldVector.x != 0f && foldVector.y != 0 ? Mathf.Sqrt(.5f) : 1f;


        Vector2 ret = foldPointStart + Mathf.Min((point - foldPointStart).magnitude, foldableLength / divisor) * cosa * foldVector.normalized;

        if (Vector2.Dot(ret - foldPointStart, foldVector) <= 0)
        {
            return foldPointStart;
        }
        return ret;
    }

    //Player Placement
    public static bool OnDesk(Vector3 point)
    {
        foreach (FoldController page in pages)
            if (page.maskCol.ClosestPoint(point) == point) return true;
        return false;
    }
    public bool AbleToFold()
    {
        return enabled;
    }

    private static void CheckPlayerPlace()
    {

        int frontCount = 0;
        int backCount = 0;
        int groundCount = 0;

        foreach (FoldController page in pages)
            if(page.gameObject.activeInHierarchy)
            {
                frontCount += page.CheckPlayerIn(page.frontCol);
                backCount += page.CheckPlayerIn(page.backCol);
                groundCount += page.CheckPlayerIn(page.maskCol);
            }

        if((frontCount <= 0 && backCount <= 0 ) || (groundCount >= 4))   //Player on ground. (Not touching any piece of paper.)
        {
            playerPlace = PlayerPlace.Ground;

            foreach (FoldController page in pages)
                if (page.gameObject.activeInHierarchy)
                {
                    page.spriteMask.enabled = true;
                    page.enabled = true;
                }
        }
        else if((frontCount > 0 && frontCount < 4)
            || (backCount > 0 && backCount < 4)
            || (groundCount > 0 && groundCount < 4))   //Player on mid. (Between any two or more different regions.)
        {
            playerPlace = PlayerPlace.Mid;
            Player.main.sr.maskInteraction = SpriteMaskInteraction.None;

            foreach (FoldController page in pages)
                if (page.gameObject.activeInHierarchy)
                {
                    page.enabled = true;
                    page.spriteMask.enabled = false;
                    if((backCount <= 0 && groundCount <= 0 && page.CheckPlayerIn(page.frontCol) > 0) ||
                        (page.CheckPlayerIn(page.backCol) > 0) || (page.CheckPlayerIn(page.maskCol) > 0))
                    {
                        page.enabled = false;
                    }
                }
        }
        else if(backCount >= 4 && groundCount <= 0)     //Player on back.
        {
            playerPlace = PlayerPlace.Back;

            //foreach (FoldController page in pages)

            foreach (FoldController page in pages)
                if(page.gameObject.activeInHierarchy)
                {
                    page.spriteMask.enabled = false;
                    page.enabled = true;
                    if(page.CheckPlayerIn(page.backCol) > 0)
                    {
                        page.playerTr.SetParent(page.backPage);
                    }
                }
        }               
        else                                        //Player on front.
        {
            playerPlace = PlayerPlace.Front;

            foreach (FoldController page in pages)
            {
                page.enabled = true;
                page.spriteMask.enabled = true;
            }
        }


        foreach (FoldController page in pages)
            page.canResume = true;
        PauseMenu.SetResume(true);

    }
    private int CheckPlayerIn(Collider col)
    {
        Physics.SyncTransforms();
        int count = 0;
        for (int i = 0; i < 4; ++i)
        {
            Vector2 offset = Vector2.up * ((i / 2) * 2 - 1) + Vector2.right * ((i % 2) * 2 - 1);
            if (col.ClosestPoint(playerTr.position + (Vector3)offset) == playerTr.position + (Vector3)offset)
                count++;
        }
        return count;
    }


    //public bool SetPlayerPage()
    //{
    //    int count = CheckPlayerIn(backCol);
    //    int groundCount = CheckPlayerIn(maskCol);
    //    if (count <= 0 && groundCount <= 0)
    //    {
    //        PlayerInFront();
    //        return false;
    //    }
    //    else if (groundCount >= 4)
    //        PlayerInGround();
    //    else if (count < 4 || (count >= 4 && groundCount > 0))
    //        PlayerInMid();
    //    else
    //        PlayerInBack();
    //    return true;
    //}



    //private void PlayerInFront()
    //{
    //    spriteMask.enabled = true;
    //    enabled = true;
    //    PauseMenu.SetResume(true);

    //    playerPlace = PlayerPlace.Front;
    //}
    //private void PlayerInMid()
    //{
    //    spriteMask.enabled = false;
    //    enabled = false;
    //    PauseMenu.SetResume(true);
    //    Player.main.sr.maskInteraction = SpriteMaskInteraction.None;

    //    playerPlace = PlayerPlace.Mid;
    //}
    //private void PlayerInBack()
    //{
    //    spriteMask.enabled = false;
    //    playerTr.SetParent(backPage);
    //    enabled = true;
    //    PauseMenu.SetResume(true);

    //    playerPlace = PlayerPlace.Back;
    //}
    //private void PlayerInGround()
    //{
    //    spriteMask.enabled = true;
    //    enabled = true;
    //    PauseMenu.SetResume(true);

    //    playerPlace = PlayerPlace.Ground;
    //}

    //IFoldEffected Functions
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
            if (c.GetComponent<Folded.IFoldEffected>() != null)
            {
                effecteds.Add(c.GetComponent<Folded.IFoldEffected>());
                Debug.Log(c.name);
            }


        Folded.IFoldEffected.ChangeFolds(effecteds, pageIndex);
    }

    #endregion

    #region Collision Holder Functions
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
                foreach (RaycastHit exitRH in exits)
                {
                    if (exitRH.collider == enterRH.collider)
                    {
                        PlaceCollisionHolder(enterRH.point, exitRH.point);

                        pairMatched = true;
                        exits.Remove(exitRH);
                        break;
                    }

                }
                if (!pairMatched)
                {
                    PlaceCollisionHolder(enterRH.point, ray.origin + ray.direction * length);
                }
            }
            foreach (RaycastHit exitRH in exits)
            {
                PlaceCollisionHolder(ray.origin, exitRH.point);
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
        tris.AddRange(new int[] { verts.Count - 4, verts.Count - 3, verts.Count - 1, verts.Count - 4, verts.Count - 1, verts.Count - 2 });

    }
    #endregion

    #region Shared Static Functions
    private static void UpdateBorders()
    {
        minBorder = Vector2.zero;
        maxBorder = Vector2.zero;
        foreach (FoldController page in pages)
        {
            if (page.transform.position.x - page.pageLossyScale.x * .5f < minBorder.x) minBorder.x = page.transform.position.x - page.pageLossyScale.x * .5f;
            if (page.transform.position.x + page.pageLossyScale.x * .5f > maxBorder.x) maxBorder.x = page.transform.position.x + page.pageLossyScale.x * .5f;
            if (page.transform.position.y - page.pageLossyScale.y * .5f < minBorder.y) minBorder.y = page.transform.position.y - page.pageLossyScale.y * .5f;
            if (page.transform.position.y + page.pageLossyScale.y * .5f > maxBorder.y) maxBorder.y = page.transform.position.y + page.pageLossyScale.y * .5f;
        }

        Debug.Log("min border: " + minBorder + "\nmax border: " + maxBorder);
    }
    #endregion

    #region enums
    private enum PlayerPlace
    {
        Front,
        Back,
        Mid,
        Ground
    }
    #endregion
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