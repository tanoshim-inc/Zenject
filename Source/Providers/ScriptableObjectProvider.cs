#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Zenject
{
	public class ScriptableObjectProvider : IProvider
	{
		readonly DiContainer _container;
		readonly Type _resourceType;
		readonly UnityEngine.Object _resource;
		readonly List<TypeValuePair> _extraArguments;
		readonly object _concreteIdentifier;
		readonly bool _createNew;

		public bool TypeVariesBasedOnMemberType
		{
			get
			{
				return false;
			}
		}

		public bool IsCached 
		{
			get
			{
				return false;
			}
		}

		public ScriptableObjectProvider(
			UnityEngine.Object resource, Type resourceType,
			DiContainer container, object concreteIdentifier, List<TypeValuePair> extraArguments,
			bool createNew)
		{
			_container = container;
			Assert.DerivesFromOrEqual<ScriptableObject>(resourceType);

			_concreteIdentifier = concreteIdentifier;
			_extraArguments = extraArguments;
			_resourceType = resourceType;
			_resource = resource;
			_createNew = createNew;
		}

		public Type GetInstanceType(InjectContext context)
		{
			return _resourceType;
		}

		public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
			InjectContext context, List<TypeValuePair> args)
		{
			Assert.IsNotNull(context);

			List<object> objects;

			if (_createNew)
			{
				objects = new List<object> (){ ScriptableObject.Instantiate (_resource) };
			}
			else
			{
				objects = new List<object> (){ _resource };
			}

			Assert.That(!objects.IsEmpty(),
				"Could not find object '{0}' with type '{1}'", _resource, _resourceType);

			yield return objects;

			var injectArgs = new InjectArgs()
			{
				ExtraArgs = _extraArguments.Concat(args).ToList(),
				Context = context,
				ConcreteIdentifier = _concreteIdentifier,
			};

			foreach (var obj in objects)
			{
				_container.InjectExplicit(
					obj, _resourceType, injectArgs);
			}
		}

		public List<object> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args, out Action injectAction)
		{
			Assert.IsNotNull(context);

			List<object> objects;

			if (_createNew)
			{
				objects = new List<object>() { UnityEngine.ScriptableObject.Instantiate(_resource) };
			}
			else
			{
				objects = new List<object>() { _resource };
			}

			var injectArgs = new InjectArgs()
			{
				ExtraArgs = _extraArguments.Concat(args).ToList(),
				Context = context,
				ConcreteIdentifier = _concreteIdentifier,
			};

			injectAction = () =>
			{
				foreach (var obj in objects)
				{
					_container.InjectExplicit(
						obj, _resourceType, injectArgs);
				}
			};

			return objects;
		}
	}
}

#endif
