using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rearranges transform based on the inclusion in the collider.
/// </summary>
public class Clustering : MonoBehaviour
{
    [SerializeField]
    private Transform shardsRootParent;

    [SerializeField]
    [Tooltip("Place boxColliders as if they were world axis aligned bounding boxes." +
        "Then try to group shards depending whether they should be together")]
    private string toolTip = "You can read a tooltip in tooltip of this tooltip";

    [SerializeField]
    [Tooltip("An aabb is used to test whether the point is inside")]
    private Transform boxCollidersParent;

    private List<BoxCollider> groupingBoxes;

    private void Start()
    {
        Transform shardsRoot = shardsRootParent.GetChild(0);
        for (int i = 0; i < shardsRoot.childCount; i++)
        {
            shardsRoot.GetChild(i).name = "Shard";
        }

        var array = boxCollidersParent.GetComponentsInChildren<BoxCollider>();
        groupingBoxes = new List<BoxCollider>(array);

        List<List<Transform>> clusters = new List<List<Transform>>();
        for (int i = 0; i < groupingBoxes.Count; i++)
        {
            clusters.Add(new List<Transform>());
            for (int j = 0; j < shardsRoot.childCount; j++)
            {
                Transform shard = shardsRoot.GetChild(j);
                if (groupingBoxes[i].bounds.Contains(shard.position))
                {
                    clusters[i].Add(shard);
                }
            }
        }

        int siblingIndex = 0;
        for (int i = 0; i < clusters.Count; i++)
        {
            int indexInGroup = 0;
            for (int j = 0; j < clusters[i].Count; j++)
            {
                clusters[i][j].SetSiblingIndex(siblingIndex++);
                clusters[i][j].name += " " + ++indexInGroup;
            }
        }
    }
}
