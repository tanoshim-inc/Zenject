using System;
using UnityEngine;
using Zenject;

namespace Zenject.SpaceFighter
{
    // Here we can add some high-level methods to give some info to other
    // parts of the codebase outside of our enemy facade
    public class EnemyFacade : MonoBehaviour, IPoolable<float, float, IMemoryPool>, IDisposable
    {
        [Inject]
        EnemyView _view;

        [Inject]
        EnemyTunables _tunables;

        [Inject]
        EnemyDeathHandler _deathHandler;

        [Inject]
        EnemyStateManager _stateManager;

        [Inject]
        EnemyRegistry _registry;

        IMemoryPool _pool;

        public EnemyStates State
        {
            get { return _stateManager.CurrentState; }
        }

        public float Accuracy
        {
            get { return _tunables.Accuracy; }
        }

        public float Speed
        {
            get { return _tunables.Speed; }
        }

        public Vector3 Position
        {
            get { return _view.Position; }
            set { _view.Position = value; }
        }

        public void Dispose()
        {
            _pool.Despawn(this);
        }

        public void Die()
        {
            _deathHandler.Die();
        }

        public void OnDespawned()
        {
            _registry.RemoveEnemy(this);
            _pool = null;
        }

        public void OnSpawned(float accuracy, float speed, IMemoryPool pool)
        {
            _pool = pool;
            _tunables.Accuracy = accuracy;
            _tunables.Speed = speed;

            _registry.AddEnemy(this);
        }

        public class Factory : PlaceholderFactory<float, float, EnemyFacade>
        {
        }
    }
}
