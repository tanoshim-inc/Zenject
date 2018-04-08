using System;
using ModestTree;

namespace Zenject
{
    public class ConcreteIdArgNonLazyBinder : ArgNonLazyBinder
    {
        public ConcreteIdArgNonLazyBinder(BindInfo bindInfo)
            : base(bindInfo)
        {
        }

        public ArgNonLazyBinder WithConcreteId(object id)
        {
            throw Assert.CreateException("TODO");
            //BindInfo.ConcreteIdentifier = id;
            return this;
        }
    }
}

