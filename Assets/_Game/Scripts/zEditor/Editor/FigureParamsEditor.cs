using UnityEditor;

namespace Orazum.CustomEditor
{ 
    [UnityEditor.CustomEditor(typeof(FigureParamsSO))]
    public class FigureParamsEditor : Editor
    {
        private SerializedProperty _generationParams;

        private SerializedProperty _shuffleStepsAmount;
        private SerializedProperty _shufflePauseTime;
        private SerializedProperty _shuffleLerpSpeed;
        private SerializedProperty _moveLerpSpeed;

        private SerializedProperty _startPosition;

        private SerializedProperty _shouldUsePredefinedEmptyPlaces;
        private SerializedProperty _predefinedEmptyPlaces;
        private SerializedProperty _emptyPlacesCount;

        public void OnEnable()
        {
            _generationParams = serializedObject.FindProperty("FigureGenParamsSO");

            _shuffleStepsAmount = serializedObject.FindProperty("ShuffleStepsAmount");
            _shufflePauseTime = serializedObject.FindProperty("ShufflePauseTime");
            _shuffleLerpSpeed = serializedObject.FindProperty("ShuffleLerpSpeed");
            _moveLerpSpeed = serializedObject.FindProperty("MoveLerpSpeed");

            _startPosition = serializedObject.FindProperty("StartPositionForSegmentsInCompletionPhase");

            _shouldUsePredefinedEmptyPlaces = serializedObject.FindProperty("ShouldUsePredefinedEmptyPlaces");
            _predefinedEmptyPlaces = serializedObject.FindProperty("PredefinedEmptyPlaces");
            _emptyPlacesCount = serializedObject.FindProperty("EmptyPlacesCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_generationParams);            

            EditorGUILayout.PropertyField(_shuffleStepsAmount);
            EditorGUILayout.PropertyField(_shufflePauseTime);
            EditorGUILayout.PropertyField(_shuffleLerpSpeed);
            EditorGUILayout.PropertyField(_moveLerpSpeed);

            EditorGUILayout.PropertyField(_startPosition);

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