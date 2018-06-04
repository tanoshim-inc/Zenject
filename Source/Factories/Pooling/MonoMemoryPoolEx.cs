using System;
using System.Collections.Generic;
using UnityEngine;
using ModestTree;

namespace Zenject
{
    // Same as MonoMemoryPool except it will automatically re-parent the game object to
    // it's original parent during the despawn.  So if you do things like:
    // Container.BindMemoryPool<Foo, Foo.Pool>()
    // .FromNewComponentOnNewGameObject().UnderTransformGroup("Foos");
    // And if you use MonoMemoryPoolEx, then Foo will always be placed underneath the Foos
    // gameobject again even if it's parent changes after spawn

    public class MonoMemoryPoolEx<TValue> : MonoMemoryPool<TValue>
        where TValue : Component
    {
        Transform _originalParent;

        protected override void OnCreated(TValue item)
        {
            base.OnCreated(item);
            _originalParent = item.transform.parent;
        }

        protected override void OnDespawned(TValue item)
        {
            base.OnDespawned(item);
            item.transform.SetParent(_originalParent, false);
        }
    }

    // One parameter
    // NOTE: For this to work, the given component must be at the root game object of the thing
    // you want to use in a pool
    public class MonoMemoryPoolEx<TParam1, TValue> : MonoMemoryPool<TParam1, TValue>
        where TValue : Component
    {
        Transform _originalParent;

        protected override void OnCreated(TValue item)
        {
            base.OnCreated(item);
            _originalParent = item.transform.parent;
        }

        protected override void OnDespawned(TValue item)
        {
            base.OnDespawned(item);
            item.transform.SetParent(_originalParent, false);
        }
    }

    // Two parameters
    // NOTE: For this to work, the given component must be at the root game object of the thing
    // you want to use in a pool
    public class MonoMemoryPoolEx<TParam1, TParam2, TValue>
        : MonoMemoryPool<TParam1, TParam2, TValue>
        where TValue : Component
    {
        Transform _originalParent;

        protected override void OnCreated(TValue item)
        {
            base.OnCreated(item);
            _originalParent = item.transform.parent;
        }

        protected override void OnDespawned(TValue item)
        {
            base.OnDespawned(item);
            item.transform.SetParent(_originalParent, false);
        }
    }

    // Three parameters
    // NOTE: For this to work, the given component must be at the root game object of the thing
    // you want to use in a pool
    public class MonoMemoryPoolEx<TParam1, TParam2, TParam3, TValue>
        : MonoMemoryPool<TParam1, TParam2, TParam3, TValue>
        where TValue : Component
    {
        Transform _originalParent;

        protected override void OnCreated(TValue item)
        {
            base.OnCreated(item);
            _originalParent = item.transform.parent;
        }

        protected override void OnDespawned(TValue item)
        {
            base.OnDespawned(item);
            item.transform.SetParent(_originalParent, false);
        }
    }

    // Four parameters
    // NOTE: For this to work, the given component must be at the root game object of the thing
    // you want to use in a pool
    public class MonoMemoryPoolEx<TParam1, TParam2, TParam3, TParam4, TValue>
        : MonoMemoryPool<TParam1, TParam2, TParam3, TParam4, TValue>
        where TValue : Component
    {
        Transform _originalParent;

        protected override void OnCreated(TValue item)
        {
            base.OnCreated(item);
            _originalParent = item.transform.parent;
        }

        protected override void OnDespawned(TValue item)
        {
            base.OnDespawned(item);
            item.transform.SetParent(_originalParent, false);
        }
    }

    // Five parameters
    // NOTE: For this to work, the given component must be at the root game object of the thing
    // you want to use in a pool
    public class MonoMemoryPoolEx<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
        : MonoMemoryPool<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
        where TValue : Component
    {
        Transform _originalParent;

        protected override void OnCreated(TValue item)
        {
            base.OnCreated(item);
            _originalParent = item.transform.parent;
        }

        protected override void OnDespawned(TValue item)
        {
            base.OnDespawned(item);
            item.transform.SetParent(_originalParent, false);
        }
    }
}

