using System;
using System.Collections.Generic;
using ModestTree;
using Zenject;

namespace Zenject
{
    public class DisposeWrapper<TValue> : IDisposable
        where TValue : class, new()
    {
        public static readonly NewableMemoryPool<TValue, IDespawnableMemoryPool<TValue>, DisposeWrapper<TValue>> Pool =
            new NewableMemoryPool<TValue, IDespawnableMemoryPool<TValue>, DisposeWrapper<TValue>>(
                x => x.OnSpawned, x => x.OnDespawned);

        IDespawnableMemoryPool<TValue> _wrappedPool;
        TValue _value;

        public TValue Value
        {
            get { return _value; }
        }

        public void Dispose()
        {
            Pool.Despawn(this);
        }

        void OnSpawned(TValue value, IDespawnableMemoryPool<TValue> wrappedPool)
        {
            Assert.IsNotNull(wrappedPool);
            Assert.IsNotNull(value);

            _wrappedPool = wrappedPool;
            _value = value;
        }

        void OnDespawned()
        {
            _wrappedPool.Despawn(_value);

            _value = null;
            _wrappedPool = null;
        }
    }
}
