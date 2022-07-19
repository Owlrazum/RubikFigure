using UnityEngine;

namespace Orazum.Utilities
{ 
    public interface IPoolable
    {
        public void OnSpawn();
        public void OnDespawn();

        public Transform GetTransform();
    }
}
