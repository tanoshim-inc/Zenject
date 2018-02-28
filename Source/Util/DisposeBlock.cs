using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public static class DisposeBlockExtensions
    {
        public static T AttachedTo<T>(this T disposable, DisposeBlock block)
            where T : IDisposable
        {
            block.Add(disposable);
            return disposable;
        }
    }

    public class DisposeBlock : IDisposable
    {
        static readonly NewableMemoryPool<DisposeBlock> _pool =
            new NewableMemoryPool<DisposeBlock>(
                x => x.OnSpawned, x => x.OnDespawned);

        readonly List<IDisposable> _disposables = new List<IDisposable>();

        void OnSpawned()
        {
            Assert.That(_disposables.Count == 0);
        }

        void OnDespawned()
        {
            // Dispose in reverse order since usually that makes the most sense
            for (int i = _disposables.Count - 1; i >= 0; i--)
            {
                _disposables[i].Dispose();
            }
            _disposables.Clear();
        }

        public void AddRange<T>(IList<T> disposables)
            where T : IDisposable
        {
            for (int i = 0; i < disposables.Count; i++)
            {
                _disposables.Add(disposables[i]);
            }
        }

        public void Add(IDisposable disposable)
        {
            Assert.That(!_disposables.Contains(disposable));
            _disposables.Add(disposable);
        }

        public void Remove(IDisposable disposable)
        {
            _disposables.RemoveWithConfirm(disposable);
        }

        public static DisposeBlock Spawn()
        {
            return _pool.Spawn();
        }

        public void Dispose()
        {
            _pool.Despawn(this);
        }
    }
}
