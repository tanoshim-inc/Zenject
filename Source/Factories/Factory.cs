using System;
using System.Collections.Generic;

#if !NOT_UNITY3D
using JetBrains.Annotations;
#endif

namespace Zenject
{
    // Zero parameters
    public class Factory<TValue> : PlaceholderFactory<TValue>, IFactory<TValue>
    {
        // If you were hoping to override this method, use BindFactory<>.FromFactory instead
#if !NOT_UNITY3D
        [NotNull]
#endif
        public TValue Create()
        {
            return CreateInternal(new List<TypeValuePair>());
        }

        protected sealed override IEnumerable<Type> ParamTypes
        {
            get { yield break; }
        }
    }

    // One parameter
    public class Factory<TParam1, TValue>
        : PlaceholderFactory<TValue>, IFactory<TParam1, TValue>
    {
        // If you were hoping to override this method, use BindFactory<>.FromFactory instead
#if !NOT_UNITY3D
        [NotNull]
#endif
        public TValue Create(TParam1 param)
        {
            return CreateInternal(
                new List<TypeValuePair>()
                {
                    InjectUtil.CreateTypePair(param),
                });
        }

        protected sealed override IEnumerable<Type> ParamTypes
        {
            get { yield return typeof(TParam1); }
        }
    }

    // Two parameters
    public class Factory<TParam1, TParam2, TValue>
        : PlaceholderFactory<TValue>, IFactory<TParam1, TParam2, TValue>
    {
        // If you were hoping to override this method, use BindFactory<>.FromFactory instead
#if !NOT_UNITY3D
        [NotNull]
#endif
        public TValue Create(TParam1 param1, TParam2 param2)
        {
            return CreateInternal(
                new List<TypeValuePair>()
                {
                    InjectUtil.CreateTypePair(param1),
                    InjectUtil.CreateTypePair(param2),
                });
        }

        protected sealed override IEnumerable<Type> ParamTypes
        {
            get
            {
                yield return typeof(TParam1);
                yield return typeof(TParam2);
            }
        }
    }

    // Three parameters
    public class Factory<TParam1, TParam2, TParam3, TValue>
        : PlaceholderFactory<TValue>, IFactory<TParam1, TParam2, TParam3, TValue>
    {
        // If you were hoping to override this method, use BindFactory<>.FromFactory instead
#if !NOT_UNITY3D
        [NotNull]
#endif
        public TValue Create(TParam1 param1, TParam2 param2, TParam3 param3)
        {
            return CreateInternal(
                new List<TypeValuePair>()
                {
                    InjectUtil.CreateTypePair(param1),
                    InjectUtil.CreateTypePair(param2),
                    InjectUtil.CreateTypePair(param3),
                });
        }

        protected sealed override IEnumerable<Type> ParamTypes
        {
            get
            {
                yield return typeof(TParam1);
                yield return typeof(TParam2);
                yield return typeof(TParam3);
            }
        }
    }

    // Four parameters
    public class Factory<TParam1, TParam2, TParam3, TParam4, TValue>
        : PlaceholderFactory<TValue>, IFactory<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        // If you were hoping to override this method, use BindFactory<>.FromFactory instead
#if !NOT_UNITY3D
        [NotNull]
#endif
        public TValue Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
        {
            return CreateInternal(
                new List<TypeValuePair>()
                {
                    InjectUtil.CreateTypePair(param1),
                    InjectUtil.CreateTypePair(param2),
                    InjectUtil.CreateTypePair(param3),
                    InjectUtil.CreateTypePair(param4),
                });
        }

        protected sealed override IEnumerable<Type> ParamTypes
        {
            get
            {
                yield return typeof(TParam1);
                yield return typeof(TParam2);
                yield return typeof(TParam3);
                yield return typeof(TParam4);
            }
        }
    }

    // Five parameters
    public class Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
        : PlaceholderFactory<TValue>, IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        // If you were hoping to override this method, use BindFactory<>.FromFactory instead
#if !NOT_UNITY3D
        [NotNull]
#endif
        public TValue Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
        {
            return CreateInternal(
                new List<TypeValuePair>()
                {
                    InjectUtil.CreateTypePair(param1),
                    InjectUtil.CreateTypePair(param2),
                    InjectUtil.CreateTypePair(param3),
                    InjectUtil.CreateTypePair(param4),
                    InjectUtil.CreateTypePair(param5),
                });
        }

        protected sealed override IEnumerable<Type> ParamTypes
        {
            get
            {
                yield return typeof(TParam1);
                yield return typeof(TParam2);
                yield return typeof(TParam3);
                yield return typeof(TParam4);
                yield return typeof(TParam5);
            }
        }
    }

    // Ten parameters
    public class Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TValue>
        : PlaceholderFactory<TValue>, IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TValue>
    {
        // If you were hoping to override this method, use BindFactory<>.ToFactory instead
        public TValue Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10)
        {
            return CreateInternal(
                new List<TypeValuePair>()
                {
                    InjectUtil.CreateTypePair(param1),
                    InjectUtil.CreateTypePair(param2),
                    InjectUtil.CreateTypePair(param3),
                    InjectUtil.CreateTypePair(param4),
                    InjectUtil.CreateTypePair(param5),
                    InjectUtil.CreateTypePair(param6),
                    InjectUtil.CreateTypePair(param7),
                    InjectUtil.CreateTypePair(param8),
                    InjectUtil.CreateTypePair(param9),
                    InjectUtil.CreateTypePair(param10),
                });
        }

        protected sealed override IEnumerable<Type> ParamTypes
        {
            get
            {
                yield return typeof(TParam1);
                yield return typeof(TParam2);
                yield return typeof(TParam3);
                yield return typeof(TParam4);
                yield return typeof(TParam5);
                yield return typeof(TParam6);
                yield return typeof(TParam7);
                yield return typeof(TParam8);
                yield return typeof(TParam9);
                yield return typeof(TParam10);
            }
        }
    }
}

