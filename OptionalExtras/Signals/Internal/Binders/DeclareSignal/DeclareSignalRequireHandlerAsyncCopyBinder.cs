using System;
using ModestTree;

namespace Zenject
{
    public class DeclareSignalRequireHandlerAsyncCopyBinder : DeclareSignalAsyncCopyBinder
    {
        public DeclareSignalRequireHandlerAsyncCopyBinder(
            SignalDeclarationBindInfo signalBindInfo, BindInfo bindInfo)
            : base(signalBindInfo, bindInfo)
        {
        }

        public DeclareSignalAsyncCopyBinder ThrowOnMissingHandler()
        {
            SignalBindInfo.MissingHandlerResponse = SignalMissingHandlerResponses.Throw;
            return this;
        }

        public DeclareSignalAsyncCopyBinder WarnOnMissingHandler()
        {
            SignalBindInfo.MissingHandlerResponse = SignalMissingHandlerResponses.Warn;
            return this;
        }

        public DeclareSignalAsyncCopyBinder IgnoreMissingHandler()
        {
            SignalBindInfo.MissingHandlerResponse = SignalMissingHandlerResponses.Ignore;
            return this;
        }
    }
}

