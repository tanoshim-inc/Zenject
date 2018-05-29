using System;
using ModestTree;

namespace Zenject
{
    public interface ILazyProvider
    {
        object GetLazy();
    }

    [ZenjectAllowDuringValidationAttribute]
    public class LazyWrapper<T> : IValidatable, ILazyProvider
    {
        readonly DiContainer _container;
        readonly InjectContext _context;

        Lazy<T> _lazy;

        public LazyWrapper(DiContainer container, InjectContext context)
        {
            Assert.DerivesFromOrEqual<T>(context.MemberType);

            _container = container;
            _context = context;
            _lazy = new Lazy<T>(GetValue);
        }

        public object GetLazy()
        {
            return _lazy;
        }

        void IValidatable.Validate()
        {
            // Cannot cast
            _container.Resolve(_context);
        }

        public T GetValue()
        {
            return (T)_container.Resolve(_context);
        }
    }
}
