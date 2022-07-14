using System.Collections.Generic;
using UnityEngine;
using Orazum.Utilities;

public class PoolsController : MonoBehaviour
{
    private Dictionary<int, ObjectPool> pools;

    private void Awake()
    {
        pools = new Dictionary<int, ObjectPool>();
    }

    public void Spawn(GameObject prefab, Vector3 pos, Transform parent = null)
    {
        int id = prefab.GetInstanceID();
        if (!pools.ContainsKey(id))
        {
            ObjectPool objPool = new ObjectPool(prefab, 5);
            pools.Add(id, objPool);
        }
        
        pools[id].Spawn(pos, parent);
    }

    public void Despawn(int prefabID , IPoolable poolable)
    {
        if (!pools.ContainsKey(prefabID))
        {
            Debug.LogError("Despawn of the poolable is not possible because of invalid prefabID");
        }

        pools[prefabID].Despawn(poolable);
    }
}


