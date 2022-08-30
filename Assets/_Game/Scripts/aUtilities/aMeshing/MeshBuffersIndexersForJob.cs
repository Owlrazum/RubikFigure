using System;
using Unity.Collections;


namespace Orazum.Meshing
{ 
    public struct MeshBuffersIndexersForJob : IDisposable
    {
        private NativeArray<MeshBuffersIndexers> _data;

        public MeshBuffersIndexersForJob(MeshBuffersIndexers indexersManaged)
        {
            _data = new NativeArray<MeshBuffersIndexers>(1, Allocator.Persistent);
        }

        public MeshBuffersIndexers GetIndexersForChangesInsideJob()
        {
            return _data[0];
        }

        public void ApplyChanges(MeshBuffersIndexers changedIndexers)
        {
            _data[0] = changedIndexers;
        }

        public MeshBuffersIndexers GetIndexersOutsideJob()
        {
            return _data[0];
        }

        public void Reset()
        {
            MeshBuffersIndexers indexers = _data[0];
            indexers.Reset();
            _data[0] = indexers;
        }

        public void DisposeIfNeeded()
        {
            if (_data.IsCreated)
            {
                _data.Dispose();
            }
        }

        public void Dispose()
        {
            _data.Dispose();
        }
    }
}