using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Zenject.Internal;

namespace Zenject
{
    public class PostInjectableInfo
    {
        readonly List<InjectableInfo> _injectableInfo;
        readonly Action<object, object[]> _action;
        readonly string _name;

        public PostInjectableInfo(
            Action<object, object[]> action,
            List<InjectableInfo> injectableInfo,
            string name)
        {
            _injectableInfo = injectableInfo;
            _action = action;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public Action<object, object[]> Action
        {
            get { return _action; }
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
        readonly Func<object[], object> _factoryMethod;
        readonly Type _typeAnalyzed;

        public ZenjectTypeInfo(
            Type typeAnalyzed,
            List<PostInjectableInfo> postInjectMethods,
            Func<object[], object> factoryMethod,
            List<InjectableInfo> fieldInjectables,
            List<InjectableInfo> propertyInjectables,
            List<InjectableInfo> constructorInjectables)
        {
            _postInjectMethods = postInjectMethods;
            _fieldInjectables = fieldInjectables;
            _propertyInjectables = propertyInjectables;
            _constructorInjectables = constructorInjectables;
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

        public Func<object[], object> FactoryMethod
        {
            get { return _factoryMethod; }
        }
    }
}
