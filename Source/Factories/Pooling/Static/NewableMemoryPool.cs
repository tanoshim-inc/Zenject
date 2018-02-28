using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public class NewableMemoryPoolBase<TValue> : IMemoryPool
        where TValue : class, new()
    {
        readonly Stack<TValue> _stack = new Stack<TValue>();

        Func<TValue, Action> _onDespawnedMethodGetter;

        public Action<TValue> OnDespawnedMethod
        {
            set { _onDespawnedMethodGetter = x => () => value(x); }
        }

        public Func<TValue, Action> OnDespawnedMethodGetter
        {
            set { _onDespawnedMethodGetter = value; }
        }

        public int NumTotal
        {
            get; private set;
        }

        public int NumActive
        {
            get { return NumTotal - NumInactive; }
        }

        public int NumInactive
        {
            get { return _stack.Count; }
        }

        public Type ItemType
        {
            get { return typeof(TValue); }
        }

        public void ClearPool()
        {
            _stack.Clear();
            NumTotal = 0;
        }

        public void ExpandPoolBy(int additionalSize)
        {
            for (int i = 0; i < additionalSize; i++)
            {
                _stack.Push(Alloc());
            }
        }

        TValue Alloc()
        {
            var value = new TValue();
            NumTotal++;
            return value;
        }

        protected TValue SpawnGetter()
        {
            TValue element;

            if (_stack.Count == 0)
            {
                element = Alloc();
            }
            else
            {
                element = _stack.Pop();
            }

            return element;
        }

        protected DisposeWrapper<TValue> SpawnWrapper(TValue instance)
        {
            return DisposeWrapper<TValue>.Pool.Spawn(instance, this.Despawn);
        }

        public void Despawn(TValue element)
        {
            if (_onDespawnedMethodGetter != null)
            {
                _onDespawnedMethodGetter(element)();
            }

            if (_stack.Count > 0 && ReferenceEquals(_stack.Peek(), element))
            {
                ModestTree.Log.Error("Getter error. Trying to destroy object that is already released to pool.");
            }

            Assert.That(!_stack.Contains(element), "Attempted to despawn element twice!");

            _stack.Push(element);
        }
    }

    public class NewableMemoryPool<TValue> : NewableMemoryPoolBase<TValue>, IMemoryPool<TValue>
        where TValue : class, new()
    {
        Func<TValue, Action> _onSpawnMethodGetter;

        public NewableMemoryPool()
        {
        }

        public NewableMemoryPool(
            Func<TValue, Action> onSpawnMethodGetter, Func<TValue, Action> onDespawnedMethodGetter = null)
        {
            OnDespawnedMethodGetter = onDespawnedMethodGetter;
            _onSpawnMethodGetter = onSpawnMethodGetter;
        }

        public Func<TValue, Action> OnSpawnMethodGetter
        {
            set { _onSpawnMethodGetter = value; }
        }

        public Action<TValue> OnSpawnMethod
        {
            set { _onSpawnMethodGetter = x => () => value(x); }
        }

        public DisposeWrapper<TValue> SpawnWrapper()
        {
            return base.SpawnWrapper(Spawn());
        }

        public TValue Spawn()
        {
            var item = SpawnGetter();

            if (_onSpawnMethodGetter != null)
            {
                _onSpawnMethodGetter(item)();
            }

            return item;
        }
    }

    public class NewableMemoryPool<TParam1, TValue> : NewableMemoryPoolBase<TValue>, IMemoryPool<TParam1, TValue>
        where TValue : class, new()
    {
        Func<TValue, Action<TParam1>> _onSpawnMethodGetter;

        public NewableMemoryPool()
        {
        }

        public NewableMemoryPool(
            Func<TValue, Action<TParam1>> onSpawnMethodGetter, Func<TValue, Action> onDespawnedMethodGetter = null)
        {
            // What's the point of having a param otherwise?
            Assert.IsNotNull(onSpawnMethodGetter);

            OnDespawnedMethodGetter = onDespawnedMethodGetter;
            _onSpawnMethodGetter = onSpawnMethodGetter;
        }

        public Func<TValue, Action<TParam1>> OnSpawnMethodGetter
        {
            set { _onSpawnMethodGetter = value; }
        }

        public Action<TParam1, TValue> OnSpawnMethod
        {
            set { _onSpawnMethodGetter = x => (p1) => value(p1, x); }
        }

        public DisposeWrapper<TValue> SpawnWrapper(TParam1 param)
        {
            return base.SpawnWrapper(Spawn(param));
        }

        public TValue Spawn(TParam1 param)
        {
            var item = SpawnGetter();

            if (_onSpawnMethodGetter != null)
            {
                _onSpawnMethodGetter(item)(param);
            }

            return item;
        }
    }

    public class NewableMemoryPool<TParam1, TParam2, TValue> : NewableMemoryPoolBase<TValue>, IMemoryPool<TParam1, TParam2, TValue>
        where TValue : class, new()
    {
        Func<TValue, Action<TParam1, TParam2>> _onSpawnMethodGetter;

        public NewableMemoryPool()
        {
        }

        public NewableMemoryPool(
            Func<TValue, Action<TParam1, TParam2>> onSpawnMethodGetter,
            Func<TValue, Action> onDespawnedMethodGetter)
        {
            // What's the point of having a param otherwise?
            Assert.IsNotNull(onSpawnMethodGetter);
            _onSpawnMethodGetter = onSpawnMethodGetter;
            OnDespawnedMethodGetter = onDespawnedMethodGetter;
        }

        public Func<TValue, Action<TParam1, TParam2>> OnSpawnMethodGetter
        {
            set { _onSpawnMethodGetter = value; }
        }

        public Action<TParam1, TParam2, TValue> OnSpawnMethod
        {
            set { _onSpawnMethodGetter = (x) => (p1, p2) => value(p1, p2, x); }
        }

        public DisposeWrapper<TValue> SpawnWrapper(TParam1 p1, TParam2 p2)
        {
            return base.SpawnWrapper(Spawn(p1, p2));
        }

        public TValue Spawn(TParam1 p1, TParam2 p2)
        {
            var item = SpawnGetter();

            if (_onSpawnMethodGetter != null)
            {
                _onSpawnMethodGetter(item)(p1, p2);
            }

            return item;
        }
    }

    public class NewableMemoryPool<TParam1, TParam2, TParam3, TValue> : NewableMemoryPoolBase<TValue>, IMemoryPool<TParam1, TParam2, TParam3, TValue>
        where TValue : class, new()
    {
        Func<TValue, Action<TParam1, TParam2, TParam3>> _onSpawnMethodGetter;

        public NewableMemoryPool()
        {
        }

        public NewableMemoryPool(
            Func<TValue, Action<TParam1, TParam2, TParam3>> onSpawnMethodGetter,
            Func<TValue, Action> onDespawnedMethodGetter)
        {
            // What's the point of having a param otherwise?
            Assert.IsNotNull(onSpawnMethodGetter);

            _onSpawnMethodGetter = onSpawnMethodGetter;
            OnDespawnedMethodGetter = onDespawnedMethodGetter;
        }

        public Func<TValue, Action<TParam1, TParam2, TParam3>> OnSpawnMethodGetter
        {
            set { _onSpawnMethodGetter = value; }
        }

        public Action<TParam1, TParam2, TParam3, TValue> OnSpawnMethod
        {
            set { _onSpawnMethodGetter = (x) => (p1, p2, p3) => value(p1, p2, p3, x); }
        }

        public DisposeWrapper<TValue> SpawnWrapper(TParam1 p1, TParam2 p2, TParam3 p3)
        {
            return base.SpawnWrapper(Spawn(p1, p2, p3));
        }

        public TValue Spawn(TParam1 p1, TParam2 p2, TParam3 p3)
        {
            var item = SpawnGetter();

            if (_onSpawnMethodGetter != null)
            {
                _onSpawnMethodGetter(item)(p1, p2, p3);
            }

            return item;
        }
    }
}
