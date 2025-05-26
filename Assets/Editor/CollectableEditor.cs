using Interactables;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Custom editor for Collectables to easily randomize them.
    /// </summary>
    [CustomEditor(typeof(Collectable))]
    public class CollectableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            Collectable collectable = target as Collectable;
            if (GUILayout.Button("Randomize") && collectable is not null)
            {
                collectable.RandomizeRotation();
                collectable.RandomizeSprite();
            }
        }
    }
}
