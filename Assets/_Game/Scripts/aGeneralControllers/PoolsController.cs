using UnityEngine;
using UnityEngine.Pool;

public class PoolsController : MonoBehaviour
{
    [SerializeField]
    private PoolableObjectPlaceHolder _prefab;

    private ObjectPool<PoolableObjectPlaceHolder> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<PoolableObjectPlaceHolder>(
            Create,
            null,
            null,
            null,
            false,
            10,
            10000
        );

        PoolingDelegatesContainer.FuncSpawn += Spawn;
        PoolingDelegatesContainer.EventDespawn += Despawn;
    }

    private void OnDestroy()
    {
        PoolingDelegatesContainer.FuncSpawn -= Spawn;
        PoolingDelegatesContainer.EventDespawn -= Despawn;
    }

    private PoolableObjectPlaceHolder Create()
    {
        return Instantiate(_prefab);
    }

    private PoolableObjectPlaceHolder Spawn()
    {
        return _pool.Get();
    }

    private void Despawn(PoolableObjectPlaceHolder bs)
    {
        _pool.Release(bs);
    }
}