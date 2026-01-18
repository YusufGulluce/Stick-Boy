using System;
using UnityEngine;

namespace Folded.Core
{
    public class Waypoints : MonoBehaviour
    {
        public Vector3[] points;

        public Vector3 this[int index]
        {
            get { return points[index]; }
            set { points[index] = value; }
        }
        public int Length
        {
            get { return points.Length; }
        }
    }
}

