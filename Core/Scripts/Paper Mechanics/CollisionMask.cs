using UnityEngine;
using System.Collections;
using Unity.Collections;
using System.Collections.Generic;

public class CollisionMask : MonoBehaviour
{
    public static List<CollisionMask> all = new();

    [SerializeField]
    public Collider area;
    //[SerializeField]
    //private string ignoreTag;
    [SerializeField]
    private Transform ignoreParentObject;

    Bounds bound;
    bool isIn = false;
    private readonly List<int> IDs = new();

    public static CollisionMask[] InCollsionMask(Transform check)
    {
        List<CollisionMask> list = new();

         foreach (CollisionMask mask in all)
            if (mask.ignoreParentObject == check)
                list.Add(mask);
        return list.ToArray();
    }

    private void Start()
    {
        //List<GameObject> objects = new (GameObject.FindGameObjectsWithTag(ignoreTag));

        //foreach (GameObject c in objects)
        //    foreach(Collider cl in c.GetComponents<Collider>())
        //    {
        //        cl.hasModifiableContacts = true;
        //        IDs.Add(cl.GetInstanceID());
        //    }

        foreach(Collider c in ignoreParentObject.gameObject.GetComponentsInChildren<Collider>())
        {
            c.hasModifiableContacts = true;
            IDs.Add(c.GetInstanceID());    
        }

        bound = area.bounds;

        all.Add(this);
    }
    private void OnEnable()
    {
        Physics.ContactModifyEvent += ModificationEvent;
    }

    private void OnDisable()
    {
        Physics.ContactModifyEvent -= ModificationEvent;
    }

    public void ModificationEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
    {
        foreach (var pair in pairs)
            for (int i = 0; i < pair.contactCount; ++i)
            {

                bool contain = false;
                foreach (int id in IDs)
                    if(id == pair.colliderInstanceID || id == pair.otherColliderInstanceID)
                    {
                        contain = true;
                        break;
                    }
                OverlapPoint(pair.GetPoint(i));

                if (isIn && contain) pair.IgnoreContact(i);
            }
    }

    public void OverlapPoint(Vector3 point)
    {
        
        isIn = Contains(point);
    }

    public bool Contains(Vector3 point)
    {
        return point == area.ClosestPoint(point);
    }

    public void UpdateArea()
    {
        Physics.SyncTransforms();
        bound = area.bounds;
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }
}
