using System;
using UnityEngine;
using UnityEngine.Splines;
using Folded.Editor;
using UnityEditor;

namespace Folded.Core
{
    public class Fly : BugAbstractor
    {

        /*
         * PATH FINDING VARIABLES
         */
        [SerializeField, Tooltip("Indicates what bug will follow as its path.")]
        private PathMode pathMode;
        [SerializeField, Tooltip("Indicates how bug will move as it follows its destination.")]
        private MovementMode movMode;

        //Variables
        //Spline Path Variables
        [SerializeField, Tooltip("Spline Path that will be active if pathMode == SplineFollower.")]
        [ShowIf("pathMode", PathMode.SplineFollower)]
        private Spline splinePath;

        [SerializeField, Tooltip("Waypoints that will be active if pathMode == WaypointFollower.")]
        [ShowIf("pathMode", PathMode.WaypointFollower)]
        private Waypoints waypoints;

        [SerializeField, Tooltip("Waypoints that will be active if pathMode == WaypointFollower.")]
        [ShowIf("pathMode", PathMode.Random)]
        private Collider2D randomArea;

        [ContextMenu("Create Spline Path")]
        private void CreateSplinePath()
        {

        }


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
    }

}

namespace Folded.Editor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionFieldName { get; private set; }
        public int ExpectedValue { get; private set; }

        public ShowIfAttribute(string conditionFieldName, object expectedValue)
        {
            ConditionFieldName = conditionFieldName;
            // Convert enum to int for generic comparison
            ExpectedValue = (int)expectedValue;
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
            return conditionProperty.enumValueIndex == attribute.ExpectedValue;
        }
    }
}

