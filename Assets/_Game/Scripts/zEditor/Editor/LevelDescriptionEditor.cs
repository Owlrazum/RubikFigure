using UnityEditor;
using UnityEngine;

namespace Orazum.CustomEditor
{ 
    [UnityEditor.CustomEditor(typeof(LevelDescriptionSO))]
    public class LevelDescriptionInspector : Editor
    {
        private SerializedProperty _shouldUsePredefinedEmptyPlaces;

        private SerializedProperty _predefinedEmptyPlaces;
        private SerializedProperty _emptyPlacesCount;
        public void OnEnable()
        {
            _shouldUsePredefinedEmptyPlaces = serializedObject.FindProperty("ShouldUsePredefinedEmptyPlaces");
            _predefinedEmptyPlaces = serializedObject.FindProperty("PredefinedEmptyPlaces");
            _emptyPlacesCount = serializedObject.FindProperty("EmptyPlacesCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(_shouldUsePredefinedEmptyPlaces);
            if (_shouldUsePredefinedEmptyPlaces.boolValue)
            {
                EditorGUILayout.PropertyField(_predefinedEmptyPlaces);
            }
            else
            {
                EditorGUILayout.PropertyField(_emptyPlacesCount);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}