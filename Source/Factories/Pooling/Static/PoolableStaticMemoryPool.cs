using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public class PoolableStaticMemoryPool<TValue> : StaticMemoryPool<TValue>
        where TValue : class, IPoolable, new()
    {
        public PoolableStaticMemoryPool()
            : base(OnSpawned, OnDespawned)
        {
        }

        static void OnSpawned(TValue value)
        {
            value.OnSpawned();
        }

        static void OnDespawned(TValue value)
        {
            value.OnDespawned();
        }
    }

    public class PoolableStaticMemoryPool<TParam1, TValue> : StaticMemoryPool<TParam1, TValue>
        where TValue : class, IPoolable<TParam1>, new()
    {
        public PoolableStaticMemoryPool()
            : base(OnSpawned, OnDespawned)
        {
        }

        static void OnSpawned(TParam1 p1, TValue value)
        {
            value.OnSpawned(p1);
        }

        static void OnDespawned(TValue value)
        {
            value.OnDespawned();
        }
    }
}
