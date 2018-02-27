using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject.SpaceFighter
{
    public class EnemyRegistry
    {
        public event Action<EnemyFacade> EnemyAdded = delegate {};
        public event Action<EnemyFacade> EnemyRemoved = delegate {};

        readonly List<EnemyFacade> _enemies = new List<EnemyFacade>();

        public IReadOnlyList<EnemyFacade> Enemies
        {
            get { return _enemies; }
        }

        public void Add(EnemyFacade enemy)
        {
            _enemies.Add(enemy);
            EnemyAdded(enemy);
        }

        public void Remove(EnemyFacade enemy)
        {
            bool removed = _enemies.Remove(enemy);
            Assert.That(removed);
            EnemyRemoved(enemy);
        }
    }
}
