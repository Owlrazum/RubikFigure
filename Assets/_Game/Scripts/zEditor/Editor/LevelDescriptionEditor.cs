using UnityEditor;

namespace Orazum.CustomEditor
{ 
    [UnityEditor.CustomEditor(typeof(LevelDescriptionSO))]
    public class LevelDescriptionInspector : Editor
    {
        private SerializedProperty _shouldUsePredefinedEmptyPlaces;

        private SerializedProperty _predefinedEmptyPlaces;
        private SerializedProperty _emptyPlacesCount;

        private SerializedProperty _shuffleStepsAmount;
        private SerializedProperty _shufflePauseTime;
        private SerializedProperty _shuffleLerpSpeed;
        private SerializedProperty _moveLerpSpeed;

        public void OnEnable()
        {
            _shouldUsePredefinedEmptyPlaces = serializedObject.FindProperty("ShouldUsePredefinedEmptyPlaces");
            _predefinedEmptyPlaces = serializedObject.FindProperty("PredefinedEmptyPlaces");
            _emptyPlacesCount = serializedObject.FindProperty("EmptyPlacesCount");

            _shuffleStepsAmount = serializedObject.FindProperty("ShuffleStepsAmount");
            _shufflePauseTime = serializedObject.FindProperty("ShufflePauseTime");
            _shuffleLerpSpeed = serializedObject.FindProperty("ShuffleLerpSpeed");
            _moveLerpSpeed = serializedObject.FindProperty("MoveLerpSpeed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_shouldUsePredefinedEmptyPlaces);
            if (_shouldUsePredefinedEmptyPlaces.boolValue)
            {
                EditorGUILayout.PropertyField(_predefinedEmptyPlaces);
            }
            else
            {
                EditorGUILayout.PropertyField(_emptyPlacesCount);
            }

            EditorGUILayout.PropertyField(_shuffleStepsAmount);
            EditorGUILayout.PropertyField(_shufflePauseTime);
            EditorGUILayout.PropertyField(_shuffleLerpSpeed);
            EditorGUILayout.PropertyField(_moveLerpSpeed);

            serializedObject.ApplyModifiedProperties();
        }
    }
}