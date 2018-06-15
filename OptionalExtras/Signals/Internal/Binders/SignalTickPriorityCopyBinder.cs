using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public class SignalTickPriorityCopyBinder : SignalCopyBinder
    {
        public SignalTickPriorityCopyBinder(
            SignalDeclarationBindInfo signalBindInfo, BindInfo bindInfo)
            : base(bindInfo)
        {
            SignalBindInfo = signalBindInfo;
            BindInfo = bindInfo;
        }

        protected BindInfo BindInfo
        {
            get; private set;
        }

        protected SignalDeclarationBindInfo SignalBindInfo
        {
            get; private set;
        }

        public SignalCopyBinder WithTickPriority(int priority)
        {
            SignalBindInfo.TickPriority = priority;
            SignalBindInfo.RunAsync = true;
            return this;
        }
    }
}

