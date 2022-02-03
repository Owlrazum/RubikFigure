using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolsController : MonoBehaviour
{
    [Serializable]
    private class PoolData
    {
        public int poolCapasity;
        public GameObject gameObject;
    }

    [SerializeField]
    private List<PoolData> prefabsToPool;

    private Dictionary<string, ObjectPool> pools;

    private void Start()
    {
        pools = new Dictionary<string, ObjectPool>();
        for (int i = 0; i < prefabsToPool.Count; i++)
        {
            GameObject gb = prefabsToPool[i].gameObject;
            int initialCapasity = prefabsToPool[i].poolCapasity;

            ObjectPool pool = new ObjectPool(gb, initialCapasity);

            string name = prefabsToPool[i].gameObject.name;

            pools.Add(name, pool);
        }
    }

    public void Spawn(string name, Vector3 pos, Transform parent = null)
    {
        pools[name].Spawn(pos, parent);
    }
}


