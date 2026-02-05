using System;
using UnityEngine;

namespace Folded.Core
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Flame : MonoBehaviour
    {
        //Private Variables
        private float radius;

        private void Start()
        {
            radius = GetComponent<CircleCollider2D>().radius;
            DestroyImmediate(GetComponent<CircleCollider2D>());
        }

        private void FixedUpdate()
        {
            if(CheckCollisionMask())
            {

            }
        }

        private bool CheckCollisionMask()
        {
            foreach(CollisionMask mask in CollisionMask.all)            
                if (((Vector2)(mask.area.ClosestPoint(transform.position) - transform.position)).magnitude <= radius) return true;            
            return false;
        }
    }
}

