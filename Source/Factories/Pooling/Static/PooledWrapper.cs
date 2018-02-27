using System;
using System.Collections.Generic;
using ModestTree;
using Zenject;

namespace Zenject
{
    public class PooledWrapper<TValue> : IDisposable
        where TValue : class, new()
    {
        Action<TValue> _despawnMethod;
        TValue _value;

        public TValue Value
        {
            get { return _value; }
        }

        // For use with using statement
        void IDisposable.Dispose()
        {
            Despawn();
        }

        public void Despawn()
        {
            Pool.Instance.Despawn(this);
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

        public class Pool : NewableMemoryPool<TValue, Action<TValue>, PooledWrapper<TValue>>
        {
            static Pool _instance = new Pool();

            public Pool()
                : base(x => x.OnSpawned, x => x.OnDespawned)
            {
            }

            public static Pool Instance
            {
                get { return _instance; }
            }
        }
    }
}
