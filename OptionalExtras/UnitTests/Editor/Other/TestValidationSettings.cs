
using System;
using System.Collections.Generic;
using Zenject;
using NUnit.Framework;
using System.Linq;
using ModestTree;
using Assert=ModestTree.Assert;

namespace Zenject.Tests
{
    [TestFixture]
    public class TestValidationSettings
    {
        DiContainer Container
        {
            get; set;
        }

        [SetUp]
        public void Setup()
        {
            Container = new DiContainer(true);
        }

        [Test]
        public void TestValidationErrorLogOnly()
        {
            Container.Settings = new ZenjectSettings()
            {
                ValidationErrorResponse = ZenjectSettings.ValidationErrorResponses.Log,
            };

            Container.Bind<Bar>().AsSingle().NonLazy();

            Container.ResolveRoots();
        }

        [Test]
        public void TestValidationErrorThrows()
        {
            Container.Settings = new ZenjectSettings()
            {
                ValidationErrorResponse = ZenjectSettings.ValidationErrorResponses.Throw,
            };

            Container.Bind<Bar>().AsSingle().NonLazy();

            Assert.Throws(() => Container.ResolveRoots());
        }

        [Test]
        public void TestOutsideObjectGraph1()
        {
            Container.Settings = new ZenjectSettings()
            {
                ValidationErrorResponse = ZenjectSettings.ValidationErrorResponses.Throw,
                ResolveOnlyRootsDuringValidation = true
            };

            Container.Bind<Bar>().AsSingle();

            Container.ResolveRoots();
        }

        [Test]
        public void TestOutsideObjectGraph2()
        {
            Container.Settings = new ZenjectSettings()
            {
                ValidationErrorResponse = ZenjectSettings.ValidationErrorResponses.Throw,
                ResolveOnlyRootsDuringValidation = false
            };

            Container.Bind<Bar>().AsSingle();

            Assert.Throws(() => Container.ResolveRoots());
        }

        public class Bar
        {
            public Bar(Foo foo)
            {
            }
        }

        public class Foo
        {
        }
    }
}


