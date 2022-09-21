using UnityEditor;

namespace Orazum.CustomEditor
{ 
    [UnityEditor.CustomEditor(typeof(FigureParamsSO))]
    public class FigureParamsEditor : Editor
    {
        //TODO add property for select method
        private SerializedProperty _generationParams;

        private SerializedProperty _emptyLerpSpeed;
        private SerializedProperty _beforeEmptyTime;
        private SerializedProperty _shouldUsePredefinedEmptyPlaces;
        private SerializedProperty _emptyPlacesCount;
        private SerializedProperty _predefinedEmptyPlaces;

        private SerializedProperty _shuffleStepsAmount;
        private SerializedProperty _shufflePauseTime;
        private SerializedProperty _shuffleLerpSpeed;

        private SerializedProperty _moveLerpSpeed;

        private SerializedProperty _completeLerpSpeed;
        private SerializedProperty _rotationAmplitude;

        public void OnEnable()
        {
            _generationParams = serializedObject.FindProperty("GenParams");

            _emptyLerpSpeed = serializedObject.FindProperty("EmptyLerpSpeed");
            _beforeEmptyTime = serializedObject.FindProperty("BeforeEmptyTime");
            _shouldUsePredefinedEmptyPlaces = serializedObject.FindProperty("ShouldUsePredefinedEmptyPlaces");
            _emptyPlacesCount = serializedObject.FindProperty("_emptyPlacesCount");
            _predefinedEmptyPlaces = serializedObject.FindProperty("PredefinedEmptyPlaces");

            _shuffleStepsAmount = serializedObject.FindProperty("ShuffleStepsAmount");
            _shufflePauseTime = serializedObject.FindProperty("ShufflePauseTime");
            _shuffleLerpSpeed = serializedObject.FindProperty("ShuffleLerpSpeed");
            
            _moveLerpSpeed = serializedObject.FindProperty("MoveLerpSpeed");

            _completeLerpSpeed = serializedObject.FindProperty("CompleteLerpSpeed");
            _rotationAmplitude = serializedObject.FindProperty("RotationAmplitude");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_generationParams);

            EditorGUILayout.PropertyField(_emptyLerpSpeed);
            EditorGUILayout.PropertyField(_beforeEmptyTime);
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

            EditorGUILayout.PropertyField(_completeLerpSpeed);
            EditorGUILayout.PropertyField(_rotationAmplitude);

            serializedObject.ApplyModifiedProperties();
        }
    }
}