using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Custom editor for the ClothObstacleHider to make it easier to position end-points.
    /// </summary>
    [CustomEditor(typeof(ClothObstacleHider)), CanEditMultipleObjects]
    public class ClothObstacleHiderEditor : UnityEditor.Editor
    {
        private SerializedProperty firstPositionProperty;
        private SerializedProperty secondPositionProperty;
        
        private void OnEnable()
        {
            firstPositionProperty = serializedObject.FindProperty(nameof(ClothObstacleHider.firstPosition));
            secondPositionProperty = serializedObject.FindProperty(nameof(ClothObstacleHider.secondPosition));
        }
        
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (targets.Length > 1) return;

            ClothObstacleHider hider = target as ClothObstacleHider;
            if (hider == null) return;

            if (GUILayout.Button("Set First Position As Current"))
            {
                firstPositionProperty.vector2Value= hider.transform.localPosition;
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Set Second Position As Current"))
            {
                secondPositionProperty.vector2Value= hider.transform.localPosition;
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Move to first position"))
            {
                hider.transform.localPosition = hider.firstPosition;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnSceneGUI()
        {
            ClothObstacleHider hider = target as ClothObstacleHider;
            Debug.Assert(hider != null);
            Transform transform = hider.transform;

            EditorGUI.BeginChangeCheck();
            Vector2 firstPos =
                Handles.PositionHandle(transform.TransformPoint(hider.firstPosition), Quaternion.identity);
            Vector2 secondPos =
                Handles.PositionHandle(transform.TransformPoint(hider.secondPosition), Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(hider, "Change Hider position");
                hider.firstPosition = transform.InverseTransformPoint(firstPos);
                hider.secondPosition = transform.InverseTransformPoint(secondPos);
            }
        }
    }
}
