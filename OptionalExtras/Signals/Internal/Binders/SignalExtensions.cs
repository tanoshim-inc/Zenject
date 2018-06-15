using System;
using ModestTree;
using System.Linq;

namespace Zenject
{
    public static class SignalExtensions
    {
        public static DeclareSignalRequireHandlerAsyncTickPriorityCopyBinder DeclareSignal<TSignal>(this DiContainer container)
        {
            var signalBindInfo = new SignalDeclarationBindInfo(typeof(TSignal));

            signalBindInfo.RunAsync = container.Settings.Signals.DefaultSyncMode == SignalDefaultSyncModes.Asynchronous;
            signalBindInfo.MissingHandlerResponse = container.Settings.Signals.MissingHandlerDefaultResponse;

            var bindInfo = container.Bind<SignalDeclaration>()
                .WithArguments(typeof(TSignal), signalBindInfo).WhenInjectedInto<SignalBus>().BindInfo;

            return new DeclareSignalRequireHandlerAsyncTickPriorityCopyBinder(signalBindInfo, bindInfo);
        }

        public static BindSignalToBinder<TSignal> BindSignal<TSignal>(this DiContainer container)
        {
            return new BindSignalToBinder<TSignal>(container);
        }
    }
}
