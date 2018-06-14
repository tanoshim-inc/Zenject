using System;
using System.Collections;
using System.Collections.Generic;
using ModestTree;
using System.Linq;

#if ZEN_SIGNALS_ADD_UNIRX
using UniRx;
#endif

namespace Zenject
{
    public class SignalBus : ILateDisposable, ITickable
    {
        readonly Dictionary<Type, SignalDeclaration> _localDeclarationMap;
        readonly List<SignalDeclaration> _localDeclarations;
        readonly SignalBus _parentBus;
        readonly Dictionary<SignalSubscriptionId, SignalSubscription> _subscriptionMap = new Dictionary<SignalSubscriptionId, SignalSubscription>();
        readonly ZenjectSettings.SignalSettings _settings;
        readonly Queue _asyncSignalQueue = new Queue();

        public SignalBus(
            [Inject(Source = InjectSources.Local)]
            List<SignalDeclarationBindInfo> signalBindings,
            [Inject(Source = InjectSources.Parent, Optional = true)]
            SignalBus parentBus,
            [InjectOptional]
            ZenjectSettings zenjectSettings)
        {
            if (zenjectSettings != null && zenjectSettings.Signals != null)
            {
                _settings = zenjectSettings.Signals;
            }
            else
            {
                _settings = ZenjectSettings.SignalSettings.Default;
            }

            _localDeclarations = new List<SignalDeclaration>();
            _localDeclarationMap = new Dictionary<Type, SignalDeclaration>();
            _parentBus = parentBus;

            for (int i = 0; i < signalBindings.Count; i++)
            {
                var signalBindInfo = signalBindings[i];

                var declaration = SignalDeclaration.Pool.Spawn(
                    signalBindInfo.SignalType,
                    signalBindInfo.MissingHandlerResponse,
                    signalBindInfo.RunAsync, _settings);

                _localDeclarations.Add(declaration);
                _localDeclarationMap.Add(signalBindInfo.SignalType, declaration);
            }
        }

        public int NumSubscribers
        {
            get { return _subscriptionMap.Count; }
        }

        public void LateDispose()
        {
            if (_settings.AutoUnsubscribeInDispose)
            {
                foreach (var subscription in _subscriptionMap.Values)
                {
                    subscription.Dispose();
                }
            }
            else
            {
                if (!_subscriptionMap.IsEmpty())
                {
                    throw Assert.CreateException(
                        "Found subscriptions for signals '{0}' in SignalBus.LateDispose!  Either add the explicit Unsubscribe or set SignalSettings.AutoUnsubscribeInDispose to true",
                        _subscriptionMap.Values.Select(x => x.SignalType.PrettyName()).Join(", "));
                }
            }

            for (int i = 0; i < _localDeclarations.Count; i++)
            {
                _localDeclarations[i].Dispose();
            }
        }

        public void Tick()
        {
            // Loop until queue is empty
            // This means that we could get infinite loops where signal A triggers signal B which
            // triggers signal A again etc. but it's still better than waiting another frame
            // so that using code can guarantee that their signal will be handled the current frame
            // This might be important if it affects the way things render to the screen for example
            while (_asyncSignalQueue.Count > 0)
            {
                var signal = _asyncSignalQueue.Dequeue();
                GetDeclaration(signal.GetType()).Fire(signal);
            }
        }

        public void Fire<TSignal>()
        {
            // Do this before creating the signal so that it throws if the signal was not declared
            var declaration = GetDeclaration(typeof(TSignal));

            var signal = (TSignal)Activator.CreateInstance(typeof(TSignal));

            if (declaration.IsAsync)
            {
                _asyncSignalQueue.Enqueue(signal);
            }
            else
            {
                declaration.Fire(signal);
            }
        }

        public void Fire(object signal)
        {
            var declaration = GetDeclaration(signal.GetType());

            if (declaration.IsAsync)
            {
                _asyncSignalQueue.Enqueue(signal);
            }
            else
            {
                declaration.Fire(signal);
            }
        }

#if ZEN_SIGNALS_ADD_UNIRX
        public IObservable<TSignal> GetStream<TSignal>()
        {
            return GetStream(typeof(TSignal)).Select(x => (TSignal)x);
        }

        public IObservable<object> GetStream(Type signalType)
        {
            return GetDeclaration(signalType).Stream;
        }
#endif

        public void Subscribe<TSignal>(Action callback)
        {
            Action<object> wrapperCallback = (args) => callback();
            SubscribeInternal(typeof(TSignal), callback, wrapperCallback);
        }

        public void Subscribe<TSignal>(Action<TSignal> callback)
        {
            Action<object> wrapperCallback = (args) => callback((TSignal)args);
            SubscribeInternal(typeof(TSignal), callback, wrapperCallback);
        }

        public void Subscribe(Type signalType, Action<object> callback)
        {
            SubscribeInternal(signalType, callback, callback);
        }

        public void Unsubscribe<TSignal>(Action callback)
        {
            Unsubscribe(typeof(TSignal), callback);
        }

        public void Unsubscribe(Type signalType, Action callback)
        {
            UnsubscribeInternal(signalType, callback, true);
        }

        public void Unsubscribe(Type signalType, Action<object> callback)
        {
            UnsubscribeInternal(signalType, callback, true);
        }

        public void Unsubscribe<TSignal>(Action<TSignal> callback)
        {
            UnsubscribeInternal(typeof(TSignal), callback, true);
        }

        public void TryUnsubscribe<TSignal>(Action callback)
        {
            UnsubscribeInternal(typeof(TSignal), callback, false);
        }

        public void TryUnsubscribe(Type signalType, Action callback)
        {
            UnsubscribeInternal(signalType, callback, false);
        }

        public void TryUnsubscribe(Type signalType, Action<object> callback)
        {
            UnsubscribeInternal(signalType, callback, false);
        }

        public void TryUnsubscribe<TSignal>(Action<TSignal> callback)
        {
            UnsubscribeInternal(typeof(TSignal), callback, false);
        }

        void UnsubscribeInternal(Type signalType, object token, bool throwIfMissing)
        {
            UnsubscribeInternal(
                new SignalSubscriptionId(signalType, token), throwIfMissing);
        }

        void UnsubscribeInternal(SignalSubscriptionId id, bool throwIfMissing)
        {
            SignalSubscription subscription;

            if (_subscriptionMap.TryGetValue(id, out subscription))
            {
                _subscriptionMap.RemoveWithConfirm(id);
                subscription.Dispose();
            }
            else
            {
                if (throwIfMissing)
                {
                    throw Assert.CreateException(
                        "Called unsubscribe for signal '{0}' but could not find corresponding subscribe.  If this is intentional, call TryUnsubscribe instead.");
                }
            }
        }

        void SubscribeInternal(Type signalType, object token, Action<object> callback)
        {
            SubscribeInternal(
                new SignalSubscriptionId(signalType, token), callback);
        }

        void SubscribeInternal(SignalSubscriptionId id, Action<object> callback)
        {
            Assert.That(!_subscriptionMap.ContainsKey(id),
                "Tried subscribing to the same signal with the same callback on Zenject.SignalBus");

            var declaration = GetDeclaration(id.SignalType);
            var subscription = SignalSubscription.Pool.Spawn(callback, declaration);

            _subscriptionMap.Add(id, subscription);
        }

        SignalDeclaration GetDeclaration(Type signalType)
        {
            SignalDeclaration handler;

            if (_localDeclarationMap.TryGetValue(signalType, out handler))
            {
                return handler;
            }

            if (_parentBus != null)
            {
                return _parentBus.GetDeclaration(signalType);
            }

            throw Assert.CreateException(
                "Fired undeclared signal with type '{0}'!", signalType);
        }
    }
}
