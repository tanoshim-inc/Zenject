using System;
using System.Collections.Generic;
using Zenject;

namespace ModestTree
{
    public class ListPool<T> : NewableMemoryPool<List<T>>
    {
        static ListPool<T> _instance = new ListPool<T>();

        public ListPool()
        {
            OnSpawnMethod = OnSpawned;
            OnDespawnedMethod = OnDespawned;
        }

        public static ListPool<T> Instance
        {
            get { return _instance; }
        }

        void OnSpawned(List<T> list)
        {
            Assert.That(list.IsEmpty());
        }

        void OnDespawned(List<T> list)
        {
            list.Clear();
        }
    }
}
