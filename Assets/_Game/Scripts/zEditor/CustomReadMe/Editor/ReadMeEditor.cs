using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ReadMe))]
public class ReadmeEditor : Editor
{
    private const float SPACE = 16;
    private ReadMe _readMe;
	private bool _isInitialized;

	private GUIStyle _styleTitle;
	private GUIStyle _styleBody;

	private void Init()
	{
        if (_isInitialized)
        { 
        	return;
		}

		_readMe = (ReadMe)target;

		_styleBody = new GUIStyle(EditorStyles.label);
		_styleBody.wordWrap = true;
		_styleBody.fontSize = 18;

		_styleTitle = new GUIStyle(_styleBody);
		_styleTitle.fontSize = 32;

		_isInitialized = true;
	}

    protected override void OnHeaderGUI()
	{
		Init();

		var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 4f - 20f, 128f);

		GUILayout.BeginHorizontal("In BigTitle");
		{
            if (_readMe.HelpText != null)
            { 
            	GUILayout.Label(_readMe.HelpText.name, _styleTitle);
			}
		}
		GUILayout.EndHorizontal();
	}

	public override void OnInspectorGUI()
	{
        if (_readMe != null)
        {
            EditorGUILayout.Space(SPACE);
            GUILayout.Label(_readMe.HelpText.text, _styleBody);
        }

        base.OnInspectorGUI();
	}
}
