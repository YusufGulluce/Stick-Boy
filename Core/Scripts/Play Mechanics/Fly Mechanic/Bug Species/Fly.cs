using System;
using UnityEngine;
using UnityEngine.Splines;
using Folded.Editor;
using UnityEditor;

namespace Folded.Core
{
    public class Fly : BugAbstractor
    {
        //General Variables

        //Components & Objects
        [SerializeField]
        private GameObject pathObject;

        [SerializeField, Tooltip("Transform to edit in path algorithms.")]
        private Transform externalTransform;
        [SerializeField, Tooltip("Transform to edit in interneal movement aglorithms.")]
        private Transform internalTransform;

        public override void OverStart()
        {
            ApplyChanges();
        }

        #region Path And Move Modes
        [Header("Modes")]
        [SerializeField, Tooltip("Indicates what bug will follow as its path.")]
        private PathMode pathMode;
        private PathMode prePathMode;
        [SerializeField, Tooltip("Indicates how bug will move as it follows its destination.")]
        private MovementMode movMode;
        #endregion Path And Move Modes

        #region Path Finding Variables
        [Header("Path Finding Variables")]
        //Variables
        //Path Variables
        [SerializeField, Tooltip("Spline Path .Active if pathMode == SplineFollower.")]
        [ShowIf("pathMode", PathMode.SplineFollower)]
        private Spline splinePath;

        [SerializeField, Tooltip("Waypoints that will be active if pathMode == WaypointFollower.")]
        [ShowIf("pathMode", PathMode.WaypointFollower)]
        private Waypoints waypoints;

        [SerializeField, Tooltip("Random Area that will be active if pathMode == Random.")]
        [ShowIf("pathMode", PathMode.Random)]
        private BoxCollider randomArea;

        [SerializeField, Tooltip("Time that will be idle after fly stops.")]
        private float idleTime;
        [SerializeField, Tooltip("Fly will stop if its position is near to aim by this error. (Only shows in PathMode == Random | WaypointFollower)")]
        [ShowIf("pathMode", PathMode.Random, PathMode.WaypointFollower)]
        private float aimTreshold;
        //Unaccasable variables

        ///<summary>Only for Random & WaypointFollower algorithms. Indicates next point to go.</summary>
        private Vector3 nextPoint = Vector3.zero;
        ///<summary>Only for SplinePath algorithm. Indicates current location on spline.</summary>
        private float currSplinePercent = 0f;
        ///<summary>Only for SplinePath algorithm. Indicates current location on spline.</summary>
        private int waypointIndex = 0;
        ///<summary>Time to use at path finding algorithms.</summary>
        private float pathTimer = 0f;


        #endregion Path Finding Variables      

        #region Path Finder Algorithms
        /// <summary>delegate of path finding funtions.</summary>
        delegate void PathFindingDelegate(float deltaTime);
        /// <summary>Current path finding function.</summary>
        private PathFindingDelegate pathFinder;

        private void RandomPathFinder(float deltaTime)
        {
            if(pathTimer > 0f)  //Moves to next point
            {
                pathTimer -= deltaTime;
            }
            else
            {
                if ((nextPoint - externalTransform.position).magnitude <= aimTreshold)
                {
                    pathTimer = idleTime;
                    nextPoint = randomArea.bounds.center +
                        new Vector3(UnityEngine.Random.Range(-1f, 1f) * randomArea.bounds.extents.x,
                                    UnityEngine.Random.Range(-1f, 1f) * randomArea.bounds.extents.y,
                                                                        externalTransform.position.z);
                }
                else
                {
                    externalTransform.position += deltaTime * speed * (nextPoint - externalTransform.position).normalized;
                }
            }
        }
        private void SplinePathFinder(float deltaTime)
        {
            currSplinePercent += deltaTime * speed / splinePath.GetLength();
            if (currSplinePercent >= 1f) currSplinePercent -= 1f;
            externalTransform.position = (Vector3)splinePath.EvaluatePosition(currSplinePercent) + pathObject.transform.position;
        }
        private void WaypointPathFinder(float deltaTime)
        {
            if (pathTimer > 0f)  //Moves to next point
            {
                pathTimer -= deltaTime;
            }
            else
            {
                if ((nextPoint - externalTransform.position).magnitude <= aimTreshold)
                {
                    pathTimer = idleTime;
                    nextPoint = waypoints[waypointIndex];
                    waypointIndex = (waypointIndex + 1) % waypoints.Length;
                }
                else
                {
                    externalTransform.position += deltaTime * speed * (nextPoint - externalTransform.position).normalized;
                }
            }
        }
        #endregion Path Finder Algorithms

        #region Internal Movement Variables
        [Header("Movement variables")]

        [SerializeField, Tooltip("Internal movement speed of fly.")]
        private float internalMovementSpeed;

        //Movement Variables
        [SerializeField, Tooltip("Fly will periodicly move to these local positions.")]
        [ShowIf("movMode", MovementMode.Shaking)]
        private Vector2[] shakePoints;

        [SerializeField, Tooltip("Fly will circle around itself in this radius.")]
        [ShowIf("movMode", MovementMode.Circuling)]
        private float circleRadius;

        [SerializeField, Tooltip("Fly will stop if its position is near to aim by this error. (Only shows in MovementMode == Shaking)")]
        [ShowIf("movMode", MovementMode.Shaking)]
        private float internalAimTrseshold;

        /// <summary>Next point index to shake.</summary>
        private int shakeIndex = 0;
        /// <summary>Current angle of circle.</summary>
        private float circleAngle = 0f;

        #endregion Internal Movement Variables

        #region Internal Movement Algrotihms
        //Use internalTransform to apply changes.
        private PathFindingDelegate moveFinder;

        //Move finding algorithm for direct.
        private void DirectMoveFinder(float deltaTime)
        {
            internalTransform.localPosition = Vector3.zero;
        }
        //Move finding algorithm for shaking.
        private void ShakeMoveFinder(float deltaTime)
        {
            internalTransform.localPosition += deltaTime * internalMovementSpeed * ((Vector3)shakePoints[shakeIndex] - internalTransform.localPosition).normalized;

            if ((internalTransform.localPosition - (Vector3)shakePoints[shakeIndex]).magnitude < internalAimTrseshold)
                shakeIndex = (shakeIndex + 1) % shakePoints.Length;
        }
        //Move finding algorithm for circuling.
        private void CircleMoveFinder(float deltaTime)
        {
            circleAngle += deltaTime * internalMovementSpeed / circleRadius;
            circleAngle %= 2f * Mathf.PI;
            internalTransform.localPosition = Mathf.Sin(circleAngle) * circleRadius * Vector3.up + Mathf.Cos(circleAngle) * circleRadius * Vector3.right;
        }
        #endregion Internal Movement Algrotihms

        [ContextMenu("Create Path")]
        private void CreatePath()
        {
            if (pathObject == null)
            {
                pathObject = new GameObject("Path");
                pathObject.transform.position = Vector3.zero;
            }
            else
            {
                if (pathObject.TryGetComponent(out BoxCollider col))
                    DestroyImmediate(col);
                else if (pathObject.TryGetComponent(out SplineContainer spline))
                    DestroyImmediate(spline);
                else if (pathObject.TryGetComponent(out Waypoints waypoints))
                    DestroyImmediate(waypoints);

            }
            Debug.Log(pathMode);
            switch (pathMode)
            {
                case PathMode.Random:
                    randomArea = pathObject.AddComponent<BoxCollider>();
                    pathFinder = RandomPathFinder;
                    break;
                case PathMode.SplineFollower:
                    splinePath = pathObject.AddComponent<SplineContainer>().AddSpline();
                    pathFinder = SplinePathFinder;
                    break;
                case PathMode.WaypointFollower:
                    waypoints = pathObject.AddComponent<Waypoints>();
                    pathFinder = WaypointPathFinder;
                    break;
            }
            switch (movMode)
            {
                case MovementMode.Direct:
                    moveFinder = DirectMoveFinder;
                    break;
                case MovementMode.Shaking:
                    moveFinder = ShakeMoveFinder;
                    break;
                case MovementMode.Circuling:
                    moveFinder = CircleMoveFinder;
                    break;
            }

            prePathMode = pathMode;
        }

        [ContextMenu("Apply Changes")]
        private void ApplyChanges()
        {
            if (pathMode != PathMode.Random && pathObject.TryGetComponent(out Collider2D col))
                DestroyImmediate(col);
            else if (pathMode != PathMode.SplineFollower && pathObject.TryGetComponent(out SplineContainer spline))
                DestroyImmediate(spline);
            else if (pathMode != PathMode.WaypointFollower && pathObject.TryGetComponent(out Waypoints waypoints))
                DestroyImmediate(waypoints);

            switch (pathMode)
            {
                case PathMode.Random:
                    if(randomArea == null)
                        randomArea = pathObject.AddComponent<BoxCollider>();
                    pathFinder = RandomPathFinder;
                    break;
                case PathMode.SplineFollower:
                    splinePath ??= pathObject.AddComponent<SplineContainer>().AddSpline();
                    pathFinder = SplinePathFinder;
                    break;
                case PathMode.WaypointFollower:
                    if (waypoints == null)
                        waypoints = pathObject.AddComponent<Waypoints>();
                    pathFinder = WaypointPathFinder;
                    break;
            }
            switch (movMode)
            {
                case MovementMode.Direct:
                    moveFinder = DirectMoveFinder;
                    break;
                case MovementMode.Shaking:
                    moveFinder = ShakeMoveFinder;
                    break;
                case MovementMode.Circuling:
                    moveFinder = CircleMoveFinder;
                    break;
            }
            prePathMode = pathMode;
        }

        private void FixedUpdate()
        {
            //Debug.Log(pathFinder);
            pathFinder?.Invoke(Time.fixedDeltaTime);
            moveFinder?.Invoke(Time.fixedDeltaTime);
        }

        #region Enums
        [Serializable]
        private enum PathMode
        {
            SplineFollower,
            WaypointFollower,
            Random
        }

        [Serializable]
        private enum MovementMode
        {
            /// <summary>Directl goes to its destinations without trying to move in itself.</summary>
            Direct,
            /// <summary>Random shaking while moving.</summary>
            Shaking,
            /// <summary>Circle around while moving.</summary>
            Circuling
        }
        #endregion Enums    
    }

}

namespace Folded.Editor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionFieldName { get; private set; }
        public int[] ExpectedValue { get; private set; }

        public ShowIfAttribute(string conditionFieldName, params object[] expectedValue)
        {
            ConditionFieldName = conditionFieldName;
            // Convert enum to int for generic comparison
            ExpectedValue = new int[expectedValue.Length];
            for(int i = 0; i < expectedValue.Length; ++i)
            {
                ExpectedValue[i] = (int)expectedValue[i];
            }
        }
    }

    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldShow(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // If hidden, collapse the height to 0 so no empty space remains
            if (ShouldShow(property))
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            return 0f;
        }

        private bool ShouldShow(SerializedProperty property)
        {
            ShowIfAttribute attribute = (ShowIfAttribute)this.attribute;

            // Find the property that acts as the condition (the enum)
            // We look relative to the current property to handle nested classes
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(attribute.ConditionFieldName);

            if (conditionProperty == null)
            {
                Debug.LogWarning($"Property {attribute.ConditionFieldName} not found.");
                return true; // Fallback to showing it
            }

            // Check if the enum index matches the expected value
            foreach(int i in attribute.ExpectedValue)
                if(conditionProperty.enumValueIndex == i)
                    return true;
            return false;
        }
    }
}

