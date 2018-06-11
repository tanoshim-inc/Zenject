using System;

namespace Zenject
{
    public class SignalRootInstaller : Installer<SignalRootInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SignalBus>().AsSingle().CopyIntoAllSubContainers();

            // Dispose last to ensure that we don't remove SignalSubscription before the user does
            Container.BindLateDisposableExecutionOrder<SignalBus>(-999);

            // Run async events at the end of the frame
            // We could do this at the beginning of the frame too but end of the frame is probably
            // better so that they are handled before the next render
            Container.BindTickableExecutionOrder<SignalBus>(1000);
        }
    }
}
