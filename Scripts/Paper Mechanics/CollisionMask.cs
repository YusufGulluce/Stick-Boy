using UnityEngine;
using System.Collections;
using Unity.Collections;
using System.Collections.Generic;

public class CollisionMask : MonoBehaviour
{
    public static List<CollisionMask> all = new();

    [SerializeField]
    private Collider area;
    [SerializeField]
    private string ignoreTag;

    Bounds bound;
    bool isIn = false;
    private readonly List<int> IDs = new();

    private void Start()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(ignoreTag);

        foreach (GameObject c in objects)
            foreach(Collider cl in c.GetComponents<Collider>())
            {
                cl.hasModifiableContacts = true;
                IDs.Add(cl.GetInstanceID());
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
        isIn = bound.Contains(point);

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
