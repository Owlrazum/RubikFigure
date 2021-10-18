using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class ObjectPool
{
    private Queue<GameObject> pool;

    private Queue<IPoolable> poolables;

    private GameObject prefab;

    public ObjectPool(GameObject prefabArg, int initialCapasity)
    {
        prefab = prefabArg;

        pool = new Queue<GameObject>();
        for (int i = 0; i < initialCapasity; i++)
        {
            GameObject gb = UnityEngine.Object.Instantiate(prefab);
            IPoolable poolable = gb.GetComponent<IPoolable>();
            if (poolable == null)
            {
                Debug.LogError("To use pool, a prefab should contain IPoolable");
            }

            pool.Enqueue(gb);
            poolables.Enqueue(poolable);
        }
    }

    public void Spawn(Vector3 pos, Transform parentArg = null)
    {
        if (pool.Count == 0)
        {
            Extend();
        }
        GameObject gb = pool.Dequeue();
        gb.transform.position = pos;
        if (parentArg != null)
        {
            gb.transform.parent = parentArg;
        }

        IPoolable poolable = poolables.Dequeue();
        poolable.OnSpawn();
    }

    public void Despawn(GameObject gb, IPoolable poolable)
    {
        gb.transform.position = Vector3.zero;
        pool.Enqueue(gb);

        poolable.OnDespawn();
        poolables.Enqueue(poolable);
    }

    private void Extend()
    {
        Debug.Log("Object Pool was extended");

        for (int i = 0; i < 5; i++)
        {
            GameObject gb = UnityEngine.Object.Instantiate(prefab);
            IPoolable poolable = gb.GetComponent<IPoolable>();
            pool.Enqueue(gb);
            poolables.Enqueue(poolable);
        }
    }
}
