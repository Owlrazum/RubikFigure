using System.Collections.Generic;

using UnityEngine;

namespace Orazum.Utilities
{ 
    public class ObjectPool
    {
        private Queue<IPoolable> _pool;

        private GameObject _prefab;
        private int _totalCapasity;

        public ObjectPool(GameObject prefabArg, int initialCapasity)
        {
            _prefab = prefabArg;

            for (int i = 0; i < initialCapasity; i++)
            {
                GameObject gb = UnityEngine.Object.Instantiate(_prefab);
                if (!gb.TryGetComponent<IPoolable>(out IPoolable poolable))
                if (poolable == null)
                {
                    Debug.LogError("To use pool, a prefab should contain IPoolable");
                }

                _pool.Enqueue(poolable);
            }

            _totalCapasity = initialCapasity;
        }

        public void Spawn(Vector3 pos, Transform parentArg = null)
        {
            if (_pool.Count == 0)
            {
                Extend();
            }

            IPoolable poolable = _pool.Dequeue();
            poolable.OnSpawn();

            poolable.GetTransform().position = pos;
            if (parentArg != null)
            {
                poolable.GetTransform().parent = parentArg;
            }
        }

        public void Despawn(IPoolable poolable)
        {
            poolable.OnDespawn();
            _pool.Enqueue(poolable);
        }

        private void Extend()
        {
            for (int i = 0; i < _totalCapasity / 2; i++)
            {
                GameObject gb = UnityEngine.Object.Instantiate(_prefab);
                IPoolable poolable = gb.GetComponent<IPoolable>();
                _pool.Enqueue(poolable);
            }

            _totalCapasity += _totalCapasity / 2;
        }
    }
}
