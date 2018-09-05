using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Zenject
{
    public class PostInjectableInfo
    {
        readonly MethodInfo _methodInfo;
        readonly List<InjectableInfo> _injectableInfo;
        readonly Action<object[], object> _action;

        public PostInjectableInfo(
            MethodInfo methodInfo,
            Action<object[], object> action,
            List<InjectableInfo> injectableInfo)
        {
            _methodInfo = methodInfo;
            _injectableInfo = injectableInfo;
            _action = action;
        }

        public Action<object[], object> Action
        {
            get { return _action; }
        }

        public MethodInfo MethodInfo
        {
            get { return _methodInfo; }
        }

        public List<InjectableInfo> InjectableInfo
        {
            get { return _injectableInfo; }
        }
    }

    public class ZenjectTypeInfo
    {
        readonly List<PostInjectableInfo> _postInjectMethods;
        readonly List<InjectableInfo> _constructorInjectables;
        readonly List<InjectableInfo> _fieldInjectables;
        readonly List<InjectableInfo> _propertyInjectables;
        readonly ConstructorInfo _injectConstructor;
        readonly Func<object[], object> _factoryMethod;
        readonly Type _typeAnalyzed;

        public ZenjectTypeInfo(
            Type typeAnalyzed,
            List<PostInjectableInfo> postInjectMethods,
            ConstructorInfo injectConstructor,
            Func<object[], object> factoryMethod,
            List<InjectableInfo> fieldInjectables,
            List<InjectableInfo> propertyInjectables,
            List<InjectableInfo> constructorInjectables)
        {
            _postInjectMethods = postInjectMethods;
            _fieldInjectables = fieldInjectables;
            _propertyInjectables = propertyInjectables;
            _constructorInjectables = constructorInjectables;
            _injectConstructor = injectConstructor;
            _typeAnalyzed = typeAnalyzed;
            _factoryMethod = factoryMethod;

            MemberInjectables = new List<InjectableInfo>();
            MemberInjectables.AddRange(fieldInjectables);
            MemberInjectables.AddRange(propertyInjectables);
        }

        public Type Type
        {
            get { return _typeAnalyzed; }
        }

        public List<PostInjectableInfo> PostInjectMethods
        {
            get { return _postInjectMethods; }
        }

        public IEnumerable<InjectableInfo> AllInjectables
        {
            get
            {
                return _constructorInjectables.Concat(_fieldInjectables).Concat(_propertyInjectables)
                    .Concat(_postInjectMethods.SelectMany(x => x.InjectableInfo));
            }
        }

        public List<InjectableInfo> MemberInjectables
        {
            get; private set;
        }

        public List<InjectableInfo> FieldInjectables
        {
            get { return _fieldInjectables; }
        }

        public List<InjectableInfo> PropertyInjectables
        {
            get { return _propertyInjectables; }
        }

        public List<InjectableInfo> ConstructorInjectables
        {
            get { return _constructorInjectables; }
        }

        public ConstructorInfo InjectConstructor
        {
            get { return _injectConstructor; }
        }

        public Func<object[], object> FactoryMethod
        {
            get { return _factoryMethod; }
        }
    }
}
