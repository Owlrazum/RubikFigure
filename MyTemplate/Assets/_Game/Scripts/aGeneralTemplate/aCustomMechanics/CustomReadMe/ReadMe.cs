using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ReadMe", menuName = "ScriptableObjects/ReadMe", order = 1)]
public class ReadMe : ScriptableObject
{
	public Texture2D icon;
	public string title;
	public Section[] sections;
	public bool loadedLayout;

	[Serializable]
	public class Section
	{
		public string heading, text, linkText, url;
	}
}
