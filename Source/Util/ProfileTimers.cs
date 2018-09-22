#if ZEN_INTERNAL_PROFILING

using System;
using System.Diagnostics;
using ModestTree;

namespace Zenject
{
    public enum InternalTimers
    {
        TypeAnalysisTotal,
        TypeAnalysisLookingUpBakedGetter,
        TypeAnalysisCallingBakedGetter,
        TypeAnalysisActualReflection,
        Count
    }

    // Similar to ProfileBlock except used for measuring speed of zenject specifically
    // And does not use unity's profiler
    public static class ProfileTimers
    {
        static Stopwatch[] _timers;

        static ProfileTimers()
        {
            _timers = new Stopwatch[(int)InternalTimers.Count];

            for (int i = 0; i < (int)InternalTimers.Count; i++)
            {
                _timers[i] = new Stopwatch();
            }
        }

        static Stopwatch GetTimer(InternalTimers type)
        {
            return _timers[(int)type];
        }

        public static double GetTime(InternalTimers type)
        {
            return GetTimer(type).Elapsed.TotalSeconds;
        }

        public static IDisposable CreateTimedBlock(InternalTimers type)
        {
            return TimedBlock.Pool.Spawn(GetTimer(type));
        }

        public static IDisposable CreatePauseBlock(InternalTimers type)
        {
            return PausedBlock.Pool.Spawn(GetTimer(type));
        }

        class TimedBlock : IDisposable
        {
            public static StaticMemoryPool<Stopwatch, TimedBlock> Pool =
                new StaticMemoryPool<Stopwatch, TimedBlock>(OnSpawned);

            Stopwatch _stopwatch;

            static void OnSpawned(Stopwatch stopwatch, TimedBlock instance)
            {
                instance._stopwatch = stopwatch;

                Assert.That(!stopwatch.IsRunning);
                stopwatch.Start();
            }

            public void Dispose()
            {
                Assert.That(_stopwatch.IsRunning);
                _stopwatch.Stop();
                Pool.Despawn(this);
            }
        }

        class PausedBlock : IDisposable
        {
            public static StaticMemoryPool<Stopwatch, PausedBlock> Pool =
                new StaticMemoryPool<Stopwatch, PausedBlock>(OnSpawned);

            Stopwatch _stopwatch;

            static void OnSpawned(Stopwatch stopwatch, PausedBlock instance)
            {
                instance._stopwatch = stopwatch;

                Assert.That(stopwatch.IsRunning);
                stopwatch.Stop();
            }

            public void Dispose()
            {
                Assert.That(!_stopwatch.IsRunning);
                _stopwatch.Start();
                Pool.Despawn(this);
            }
        }
    }
}

#endif
