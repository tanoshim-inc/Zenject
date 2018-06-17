using System;
using System.Diagnostics;
using ModestTree;
using NUnit.Framework;
using UnityEngine;
using Assert = ModestTree.Assert;

namespace Zenject.Tests.Other
{
    [TestFixture]
    public class TestMisc : ZenjectUnitTestFixture
    {
        public interface ILoadFilter
        {
        }

        public class CacheLoadFilter : ILoadFilter
        {
        }

        public class DatabaseLoadFilter : ILoadFilter
        {
        }

        public class FileLoadFilter : ILoadFilter
        {
        }

        [Test]
        public void Test1()
        {
            Container.Bind<ILoadFilter>().WithId("cache").To<CacheLoadFilter>().AsSingle();
            Container.Bind<ILoadFilter>().WithId("database").To<DatabaseLoadFilter>().AsSingle();
            Container.Bind<ILoadFilter>().WithId("file").To<FileLoadFilter>().AsSingle();

            Container.ResolveId<ILoadFilter>("cache");
        }
    }
}
