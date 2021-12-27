using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BeizerSegment))]
public class BeizerShow : Editor
{
    void OnSceneGUI()
    {
        var beizerData = (BeizerSegment)target;

        DrawBeizer(beizerData);
    }

    private void DrawBeizer(BeizerSegment beizerData)
    {
        Transform[] ar = new Transform[4];
        for (int j = 0; j < 4; j++)
        {
            ar[j] = beizerData.transform.GetChild(j);
            ar[j].position = Handles.PositionHandle(
                ar[j].position,
                Quaternion.identity);
        }
        Handles.DrawBezier(
            ar[0].position, ar[3].position,
            ar[1].position, ar[2].position,
            Color.red, null, 5);
    }
}
