using System;
using System.Collections.Generic;
using ModestTree;
using Zenject;

namespace Zenject
{
    public class DisposeWrapper<TValue> : IDisposable
        where TValue : class, new()
    {
        public static readonly NewableMemoryPool<TValue, Action<TValue>, DisposeWrapper<TValue>> Pool =
            new NewableMemoryPool<TValue, Action<TValue>, DisposeWrapper<TValue>>(
                x => x.OnSpawned, x => x.OnDespawned);

        Action<TValue> _despawnMethod;
        TValue _value;

        public TValue Value
        {
            get { return _value; }
        }

        public void Dispose()
        {
            Pool.Despawn(this);
        }

        void OnSpawned(TValue value, Action<TValue> despawnMethod)
        {
            Assert.IsNotNull(despawnMethod);
            Assert.IsNotNull(value);

            _despawnMethod = despawnMethod;
            _value = value;
        }

        void OnDespawned()
        {
            _despawnMethod(_value);
            _value = null;
            _despawnMethod = null;
        }
    }
}
