using System;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UITextHighlighter : MonoBehaviour
{
    [SerializeField]
    private Color _defaultColor = Color.black;

    [SerializeField]
    private Color _highlightColor = Color.white;

    private UIButton _button;
    private TextMeshProUGUI _text;

    private void Awake()
    {
        TryGetComponent(out _text);
        bool isFound = transform.parent.TryGetComponent(out _button);
#if UNITY_EDITOR
        if (!isFound)
        {
            Debug.LogError("Parent should Contain UIButton!");
        }
#endif

        _button.EventOnEnter += OnEnter;
        _button.EventOnExit += OnExit;
    }

    private void OnEnter()
    {
        _text.color = _highlightColor;
    }

    private void OnExit()
    { 
        _text.color = _defaultColor;
    }

    private void OnDestroy()
    {
        _button.EventOnEnter -= OnEnter;
        _button.EventOnExit -= OnExit;
    }
}