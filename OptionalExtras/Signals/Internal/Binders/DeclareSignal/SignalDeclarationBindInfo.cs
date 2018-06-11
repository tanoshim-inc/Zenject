using System;
using ModestTree;

namespace Zenject
{
    public class SignalDeclarationBindInfo
    {
        public SignalDeclarationBindInfo(Type signalType)
        {
            Assert.That(signalType.DerivesFromOrEqual<ISignal>());
            SignalType = signalType;
        }

        public Type SignalType
        {
            get; private set;
        }

        public bool RunAsync
        {
            get; set;
        }

        public SignalMissingHandlerResponses MissingHandlerResponse
        {
            get; set;
        }
    }
}
