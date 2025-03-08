using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Custom editor for the AttachableMovingObject to make it easier to position end-points.
    /// </summary>
    [CustomEditor(typeof(AttachableMovingObject)), CanEditMultipleObjects]
    public class AttachableMovingObjectEditor : UnityEditor.Editor
    {
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (targets.Length > 1) return;

            AttachableMovingObject attachableMovingObject = target as AttachableMovingObject;
            if (attachableMovingObject == null) return;

            if (GUILayout.Button("Set First Position As Current"))
            {
                attachableMovingObject.firstPosition = attachableMovingObject.transform.position;
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Set Second Position As Current"))
            {
                attachableMovingObject.secondPosition = attachableMovingObject.transform.position;
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Move to first position"))
            {
                attachableMovingObject.transform.position = attachableMovingObject.firstPosition;
                serializedObject.ApplyModifiedProperties();
            }

            GUI.enabled = attachableMovingObject.pathRenderer;
            if (GUILayout.Button("Update Path Renderer"))
            {
                attachableMovingObject.pathRenderer.UpdateLocation(attachableMovingObject);
            }
        }

        private void OnSceneGUI()
        {
            AttachableMovingObject attachableMovingObject = target as AttachableMovingObject;
            Debug.Assert(attachableMovingObject != null);

            EditorGUI.BeginChangeCheck();
            Vector2 firstPos = Handles.PositionHandle(attachableMovingObject.firstPosition, Quaternion.identity);
            Vector2 secondPos = Handles.PositionHandle(attachableMovingObject.secondPosition, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(attachableMovingObject, "Change Hider position");
                attachableMovingObject.firstPosition = firstPos;
                attachableMovingObject.secondPosition = secondPos;
            }
        }
    }
}
