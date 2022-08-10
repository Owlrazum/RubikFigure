using UnityEditor;

namespace Orazum.CustomEditor
{ 
    [UnityEditor.CustomEditor(typeof(LevelDescriptionSO))]
    public class LevelDescriptionInspector : Editor
    {
        private SerializedProperty _generationParams;
        private SerializedProperty _segmentPrefab;
        private SerializedProperty _segmentPointPrefab;

        private SerializedProperty _shuffleStepsAmount;
        private SerializedProperty _shufflePauseTime;
        private SerializedProperty _shuffleLerpSpeed;
        private SerializedProperty _moveLerpSpeed;

        private SerializedProperty _shouldUsePredefinedEmptyPlaces;
        private SerializedProperty _predefinedEmptyPlaces;
        private SerializedProperty _emptyPlacesCount;

        public void OnEnable()
        {
            _generationParams = serializedObject.FindProperty("GenerationParams");
            _segmentPrefab = serializedObject.FindProperty("SegmentPrefab");
            _segmentPointPrefab = serializedObject.FindProperty("SegmentPointPrefab");

            _shuffleStepsAmount = serializedObject.FindProperty("ShuffleStepsAmount");
            _shufflePauseTime = serializedObject.FindProperty("ShufflePauseTime");
            _shuffleLerpSpeed = serializedObject.FindProperty("ShuffleLerpSpeed");
            _moveLerpSpeed = serializedObject.FindProperty("MoveLerpSpeed");

            _shouldUsePredefinedEmptyPlaces = serializedObject.FindProperty("ShouldUsePredefinedEmptyPlaces");
            _predefinedEmptyPlaces = serializedObject.FindProperty("PredefinedEmptyPlaces");
            _emptyPlacesCount = serializedObject.FindProperty("EmptyPlacesCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_generationParams);            
            EditorGUILayout.PropertyField(_segmentPrefab);
            EditorGUILayout.PropertyField(_segmentPointPrefab);

            EditorGUILayout.PropertyField(_shuffleStepsAmount);
            EditorGUILayout.PropertyField(_shufflePauseTime);
            EditorGUILayout.PropertyField(_shuffleLerpSpeed);
            EditorGUILayout.PropertyField(_moveLerpSpeed);

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