using System;
using ModestTree;

namespace Zenject
{
    public class SignalSubscription : IDisposable
    {
        public static readonly NewableMemoryPool<Action<object>, SignalDeclaration, SignalSubscription> Pool =
            new NewableMemoryPool<Action<object>, SignalDeclaration, SignalSubscription>(
                x => x.OnSpawned, x => x.OnDespawned);

        Action<object> _callback;
        SignalDeclaration _declaration;
        Type _signalType;

        public SignalSubscription()
        {
            SetDefaults();
        }

        public Type SignalType
        {
            get { return _signalType; }
        }

        void SetDefaults()
        {
            _callback = null;
            _declaration = null;
            _signalType = null;
        }

        public void Dispose()
        {
            Pool.Despawn(this);
        }

        void OnSpawned(Action<object> callback, SignalDeclaration declaration)
        {
            Assert.IsNull(_callback);
            _callback = callback;
            _declaration = declaration;
            // Cache this in case OnDeclarationDespawned is called
            _signalType = declaration.SignalType;

            declaration.Add(this);
        }

        // See comment in SignalDeclaration for why this exists
        public void OnDeclarationDespawned()
        {
            _declaration = null;
        }

        void OnDespawned()
        {
            if (_declaration != null)
            {
                _declaration.Remove(this);
            }
            SetDefaults();
        }

        public void Invoke(ISignal signal)
        {
            _callback(signal);
        }
    }
}
