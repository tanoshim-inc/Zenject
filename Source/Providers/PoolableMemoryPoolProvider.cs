using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public abstract class PoolableMemoryPoolProviderBase<TContract> : IProvider
    {
        public PoolableMemoryPoolProviderBase(
            DiContainer container, Guid poolId)
        {
            Container = container;
            PoolId = poolId;
        }

        public bool IsCached
        {
            get { return false; }
        }

        protected Guid PoolId
        {
            get;
            private set;
        }

        protected DiContainer Container
        {
            get;
            private set;
        }

        public bool TypeVariesBasedOnMemberType
        {
            get { return false; }
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(TContract);
        }

        public abstract List<object> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction);
    }

    // Zero parameters

    public class PoolableMemoryPoolProvider<TContract> : PoolableMemoryPoolProviderBase<TContract>
        where TContract : IPoolable<IMemoryPool>
    {
        public PoolableMemoryPoolProvider(
            DiContainer container, Guid poolId)
            : base(container, poolId)
        {
        }

        public override List<object> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction)
        {
            Assert.That(args.IsEmpty());

            Assert.IsNotNull(context);

            Assert.That(typeof(TContract).DerivesFromOrEqual(context.MemberType));

            // Do this even when validating in case it has its own dependencies
            var pool = Container.ResolveId<PoolableMemoryPool<IMemoryPool, TContract>>(PoolId);

            injectAction = null;

            return new List<object>() { pool.Spawn(pool) };
        }
    }

    // One parameters

    public class PoolableMemoryPoolProvider<TParam1, TContract> : PoolableMemoryPoolProviderBase<TContract>
        where TContract : IPoolable<TParam1, IMemoryPool>
    {
        public PoolableMemoryPoolProvider(
            DiContainer container, Guid poolId)
            : base(container, poolId)
        {
        }

        public override List<object> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction)
        {
            Assert.IsEqual(args.Count, 1);
            Assert.IsNotNull(context);

            Assert.That(typeof(TContract).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());

            // Do this even when validating in case it has its own dependencies
            var pool = Container.ResolveId<PoolableMemoryPool<TParam1, IMemoryPool, TContract>>(PoolId);

            injectAction = null;

            return new List<object>() { pool.Spawn((TParam1)args[0].Value, pool) };
        }
    }

    // Two parameters

    public class PoolableMemoryPoolProvider<TParam1, TParam2, TContract> : PoolableMemoryPoolProviderBase<TContract>
        where TContract : IPoolable<TParam1, TParam2, IMemoryPool>
    {
        public PoolableMemoryPoolProvider(
            DiContainer container, Guid poolId)
            : base(container, poolId)
        {
        }

        public override List<object> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction)
        {
            Assert.IsEqual(args.Count, 2);
            Assert.IsNotNull(context);

            Assert.That(typeof(TContract).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());

            // Do this even when validating in case it has its own dependencies
            var pool = Container.ResolveId<PoolableMemoryPool<TParam1, TParam2, IMemoryPool, TContract>>(PoolId);

            injectAction = null;

            return new List<object>() { pool.Spawn(
                (TParam1)args[0].Value,
                (TParam2)args[1].Value,
                pool) };
        }
    }

    // Three parameters

    public class PoolableMemoryPoolProvider<TParam1, TParam2, TParam3, TContract> : PoolableMemoryPoolProviderBase<TContract>
        where TContract : IPoolable<TParam1, TParam2, TParam3, IMemoryPool>
    {
        public PoolableMemoryPoolProvider(
            DiContainer container, Guid poolId)
            : base(container, poolId)
        {
        }

        public override List<object> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction)
        {
            Assert.IsEqual(args.Count, 3);
            Assert.IsNotNull(context);

            Assert.That(typeof(TContract).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());

            // Do this even when validating in case it has its own dependencies
            var pool = Container.ResolveId<PoolableMemoryPool<TParam1, TParam2, TParam3, IMemoryPool, TContract>>(PoolId);

            injectAction = null;

            return new List<object>() { pool.Spawn(
                (TParam1)args[0].Value,
                (TParam2)args[1].Value,
                (TParam3)args[2].Value,
                pool) };
        }
    }

    // Four parameters

    public class PoolableMemoryPoolProvider<TParam1, TParam2, TParam3, TParam4, TContract> : PoolableMemoryPoolProviderBase<TContract>
        where TContract : IPoolable<TParam1, TParam2, TParam3, TParam4, IMemoryPool>
    {
        public PoolableMemoryPoolProvider(
            DiContainer container, Guid poolId)
            : base(container, poolId)
        {
        }

        public override List<object> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction)
        {
            Assert.IsEqual(args.Count, 4);
            Assert.IsNotNull(context);

            Assert.That(typeof(TContract).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());
            Assert.That(args[3].Type.DerivesFromOrEqual<TParam4>());

            // Do this even when validating in case it has its own dependencies
            var pool = Container.ResolveId<PoolableMemoryPool<TParam1, TParam2, TParam3, TParam4, IMemoryPool, TContract>>(PoolId);

            injectAction = null;

            return new List<object>() { pool.Spawn(
                (TParam1)args[0].Value,
                (TParam2)args[1].Value,
                (TParam3)args[2].Value,
                (TParam4)args[3].Value,
                pool) };
        }
    }

    // Five parameters

    public class PoolableMemoryPoolProvider<TParam1, TParam2, TParam3, TParam4, TParam5, TContract> : PoolableMemoryPoolProviderBase<TContract>
        where TContract : IPoolable<TParam1, TParam2, TParam3, TParam4, TParam5, IMemoryPool>
    {
        public PoolableMemoryPoolProvider(
            DiContainer container, Guid poolId)
            : base(container, poolId)
        {
        }

        public override List<object> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction)
        {
            Assert.IsEqual(args.Count, 5);
            Assert.IsNotNull(context);

            Assert.That(typeof(TContract).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());
            Assert.That(args[3].Type.DerivesFromOrEqual<TParam4>());
            Assert.That(args[4].Type.DerivesFromOrEqual<TParam5>());

            // Do this even when validating in case it has its own dependencies
            var pool = Container.ResolveId<PoolableMemoryPool<TParam1, TParam2, TParam3, TParam4, TParam5, IMemoryPool, TContract>>(PoolId);

            injectAction = null;

            return new List<object>() { pool.Spawn(
                (TParam1)args[0].Value,
                (TParam2)args[1].Value,
                (TParam3)args[2].Value,
                (TParam4)args[3].Value,
                (TParam5)args[4].Value,
                pool) };
        }
    }

    // Six parameters

    public class PoolableMemoryPoolProvider<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TContract> : PoolableMemoryPoolProviderBase<TContract>
        where TContract : IPoolable<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, IMemoryPool>
    {
        public PoolableMemoryPoolProvider(
            DiContainer container, Guid poolId)
            : base(container, poolId)
        {
        }

        public override List<object> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction)
        {
            Assert.IsEqual(args.Count, 6);
            Assert.IsNotNull(context);

            Assert.That(typeof(TContract).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());
            Assert.That(args[3].Type.DerivesFromOrEqual<TParam4>());
            Assert.That(args[4].Type.DerivesFromOrEqual<TParam5>());
            Assert.That(args[5].Type.DerivesFromOrEqual<TParam6>());

            // Do this even when validating in case it has its own dependencies
            var pool = Container.ResolveId<PoolableMemoryPool<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, IMemoryPool, TContract>>(PoolId);

            injectAction = null;

            return new List<object>() { pool.Spawn(
                (TParam1)args[0].Value,
                (TParam2)args[1].Value,
                (TParam3)args[2].Value,
                (TParam4)args[3].Value,
                (TParam5)args[4].Value,
                (TParam6)args[5].Value,
                pool) };
        }
    }
}

