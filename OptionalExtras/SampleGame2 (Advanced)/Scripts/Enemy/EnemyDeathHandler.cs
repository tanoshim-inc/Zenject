using System;
using UnityEngine;
using Zenject;

namespace Zenject.SpaceFighter
{
    public class EnemyDeathHandler
    {
        readonly SignalBus _signalBus;
        readonly EnemyFacade.Pool _selfFactory;
        readonly Settings _settings;
        readonly Explosion.Pool _explosionPool;
        readonly AudioPlayer _audioPlayer;
        readonly Enemy _enemy;
        readonly EnemyFacade _facade;

        public EnemyDeathHandler(
            Enemy enemy,
            AudioPlayer audioPlayer,
            Explosion.Pool explosionPool,
            Settings settings,
            EnemyFacade.Pool selfFactory,
            EnemyFacade facade,
            SignalBus signalBus)
        {
            _signalBus = signalBus;
            _facade = facade;
            _selfFactory = selfFactory;
            _settings = settings;
            _explosionPool = explosionPool;
            _audioPlayer = audioPlayer;
            _enemy = enemy;
        }

        public void Die()
        {
            var explosion = _explosionPool.Spawn();
            explosion.transform.position = _enemy.Position;

            _audioPlayer.Play(_settings.DeathSound, _settings.DeathSoundVolume);

            _signalBus.Fire<EnemyKilledSignal>();

            _selfFactory.Despawn(_facade);
        }

        [Serializable]
        public class Settings
        {
            public AudioClip DeathSound;
            public float DeathSoundVolume = 1.0f;
        }
    }
}
