using UnityEngine;
using System;
using UnityEditor;
//using System.Numerics;
namespace Folded.Editor
{
    
    [CustomEditor(typeof(Core.Waypoints))]
    public class WaypointsEditor : UnityEditor.Editor
    {
        #region Fields
        private float _dashSize = 4f;
        private Core.Waypoints _waypoints;

        private int[] _segmentIndices;

        private GUISkin _sceneSkin;
        #endregion Fields

        private void OnEnable()
        {
            _sceneSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
            Tools.hidden = true;

            _waypoints = target as Core.Waypoints;
        }

        private void OnDisable()
        {
            Tools.hidden = false;
        }

        private void OnSceneGUI()
        {
            if (_waypoints == null)
                OnEnable();
            //Recreates segments if undo or redo.
            if (Event.current.type == EventType.ValidateCommand && Event.current.commandName.Equals("UndoRedoPerformed"))
                CreateSegments();


            //Debug.Log("_waypoints: " + _waypoints[0].x);
            if (_waypoints == null)
                Debug.Log("a");
            //Position handles
            for(int i = 0; i < _waypoints.points.Length; ++i)
            {
                Handles.Label(_waypoints[i], i.ToString(), _sceneSkin.textField);
                EditorGUI.BeginChangeCheck();
                Vector3 newPosition = Handles.PositionHandle(_waypoints[i], Quaternion.identity);

                if(EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_waypoints, "Moved waypoint.");
                    _waypoints[i] = newPosition;

                }
            }

        }

        private void CreateSegments()
        {
            if (_waypoints.points.Length < 2)
                return;
            _segmentIndices = new int[_waypoints.Length * 2];
            int index = 0;

            for(int i = 0; i < _segmentIndices.Length - 2; i += 2)
            {
                _segmentIndices[i] = index;
                ++index;
                _segmentIndices[i + 1] = index;
            }
            _segmentIndices[^2] = _waypoints.Length - 1;
            _segmentIndices[^1] = 0;
        }
    }

}

