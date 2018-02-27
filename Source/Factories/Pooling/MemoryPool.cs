using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    // Zero parameters
    public class MemoryPool<TValue> : MemoryPoolBase<TValue>, IMemoryPool<TValue>
    {
        public TValue Spawn()
        {
            var item = GetInternal();
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start("{0}.Reinitialize", this.GetType()))
#endif
            {
                Reinitialize(item);
            }
            return item;
        }

        protected virtual void Reinitialize(TValue item)
        {
            // Optional
        }
    }

    // One parameter
    public class MemoryPool<TParam1, TValue>
        : MemoryPoolBase<TValue>, IMemoryPool<TParam1, TValue>
    {
        public TValue Spawn(TParam1 param)
        {
            var item = GetInternal();
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start("{0}.Reinitialize", this.GetType()))
#endif
            {
                Reinitialize(param, item);
            }
            return item;
        }

        protected virtual void Reinitialize(TParam1 p1, TValue item)
        {
            // Optional
        }
    }

    // Two parameters
    public class MemoryPool<TParam1, TParam2, TValue>
        : MemoryPoolBase<TValue>, IMemoryPool<TParam1, TParam2, TValue>
    {
        public TValue Spawn(TParam1 param1, TParam2 param2)
        {
            var item = GetInternal();

#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start("{0}.Reinitialize", this.GetType()))
#endif
            {
                Reinitialize(param1, param2, item);
            }
            return item;
        }

        protected virtual void Reinitialize(TParam1 p1, TParam2 p2, TValue item)
        {
            // Optional
        }
    }

    // Three parameters
    public class MemoryPool<TParam1, TParam2, TParam3, TValue>
        : MemoryPoolBase<TValue>, IMemoryPool<TParam1, TParam2, TParam3, TValue>
    {
        public TValue Spawn(TParam1 param1, TParam2 param2, TParam3 param3)
        {
            var item = GetInternal();
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start("{0}.Reinitialize", this.GetType()))
#endif
            {
                Reinitialize(param1, param2, param3, item);
            }
            return item;
        }

        protected virtual void Reinitialize(TParam1 p1, TParam2 p2, TParam3 p3, TValue item)
        {
            // Optional
        }
    }

    // Four parameters
    public class MemoryPool<TParam1, TParam2, TParam3, TParam4, TValue>
        : MemoryPoolBase<TValue>, IMemoryPool<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        public TValue Spawn(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
        {
            var item = GetInternal();
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start("{0}.Reinitialize", this.GetType()))
#endif
            {
                Reinitialize(param1, param2, param3, param4, item);
            }
            return item;
        }

        protected virtual void Reinitialize(TParam1 p1, TParam2 p2, TParam3 p3, TParam4 p4, TValue item)
        {
            // Optional
        }
    }

    // Five parameters
    public class MemoryPool<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
        : MemoryPoolBase<TValue>, IMemoryPool<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        public TValue Spawn(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
        {
            var item = GetInternal();
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start("{0}.Reinitialize", this.GetType()))
#endif
            {
                Reinitialize(param1, param2, param3, param4, param5, item);
            }
            return item;
        }

        protected virtual void Reinitialize(TParam1 p1, TParam2 p2, TParam3 p3, TParam4 p4, TParam5 p5, TValue item)
        {
            // Optional
        }
    }

    // Six parameters
    public class MemoryPool<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>
        : MemoryPoolBase<TValue>, IMemoryPool<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>
    {
        public TValue Spawn(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
        {
            var item = GetInternal();
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start("{0}.Reinitialize", this.GetType()))
#endif
            {
                Reinitialize(param1, param2, param3, param4, param5, param6, item);
            }
            return item;
        }

        protected virtual void Reinitialize(TParam1 p1, TParam2 p2, TParam3 p3, TParam4 p4, TParam5 p5, TParam6 p6, TValue item)
        {
            // Optional
        }
    }
}
