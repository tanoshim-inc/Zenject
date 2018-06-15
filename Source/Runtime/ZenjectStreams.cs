#if ZEN_SIGNALS_ADD_UNIRX

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UniRx;

namespace Zenject
{
    public class ZenjectStreams : IInitializable, ITickable, ILateTickable, IFixedTickable, IDisposable, ILateDisposable
    {
        // Execute just after all the unordered objects
        // This makes sense as a default for cases where classes wait to Sample() using the streams below since
        // the original trigger would most likely occur during the unordered objects and therefore the
        // Sample will be triggered during the same frame
        public static readonly int ExecutionOrder = 1;

        readonly Subject<Unit> _tickStream = new Subject<Unit>();
        readonly Subject<Unit> _lateTickStream = new Subject<Unit>();
        readonly Subject<Unit> _fixedTickStream = new Subject<Unit>();
        readonly Subject<Unit> _disposeStream = new Subject<Unit>();
        readonly Subject<Unit> _lateDisposeStream = new Subject<Unit>();
        readonly Subject<Unit> _initializeStream = new Subject<Unit>();

        public ZenjectStreams(
            TickableManager tickManager, InitializableManager initManager, DisposableManager disposeManager)
        {
            tickManager.Add(this, ExecutionOrder);
            tickManager.AddLate(this, ExecutionOrder);
            tickManager.AddFixed(this, ExecutionOrder);
            initManager.Add(this, ExecutionOrder);;
            disposeManager.Add(this, ExecutionOrder);
            disposeManager.AddLate(this, ExecutionOrder);
        }

        public IObservableRx<Unit> TickStream
        {
            get { return _tickStream; }
        }

        public IObservableRx<Unit> LateTickStream
        {
            get { return _lateTickStream; }
        }

        public IObservableRx<Unit> FixedTickStream
        {
            get { return _fixedTickStream; }
        }

        public IObservableRx<Unit> DisposeStream
        {
            get { return _disposeStream; }
        }

        public IObservableRx<Unit> LateDisposeStream
        {
            get { return _lateDisposeStream; }
        }

        public IObservableRx<Unit> InitializeStream
        {
            get { return _initializeStream; }
        }

        public void Initialize()
        {
            _initializeStream.OnNext();
        }

        public void Dispose()
        {
            _disposeStream.OnNext();
        }

        public void LateDispose()
        {
            _lateDisposeStream.OnNext();
        }

        public void Tick()
        {
            _tickStream.OnNext();
        }

        public void LateTick()
        {
            _lateTickStream.OnNext();
        }

        public void FixedTick()
        {
            _fixedTickStream.OnNext();
        }
    }
}

#endif
