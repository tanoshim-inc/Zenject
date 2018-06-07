using System;
using ModestTree;
using NUnit.Framework;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.Other
{
    [TestFixture]
    public class TestDecorators : ZenjectUnitTestFixture
    {
        static int CallCounter = 0;

        public interface ISaveHandler
        {
            void Save();
        }

        public class SaveHandler : ISaveHandler
        {
            public static int CallCount
            {
                get; set;
            }

            public void Save()
            {
                CallCount = CallCounter++;
            }
        }

        public class SaveDecorator1 : ISaveHandler
        {
            readonly ISaveHandler _handler;

            public SaveDecorator1(ISaveHandler handler)
            {
                _handler = handler;
            }

            public static int CallCount
            {
                get; set;
            }

            public void Save()
            {
                CallCount = CallCounter++;
                _handler.Save();
            }
        }

        public class SaveDecorator2 : ISaveHandler
        {
            readonly ISaveHandler _handler;

            public SaveDecorator2(ISaveHandler handler)
            {
                _handler = handler;
            }

            public static int CallCount
            {
                get; set;
            }

            public void Save()
            {
                CallCount = CallCounter++;
                _handler.Save();
            }
        }

        public class Foo
        {
        }

        [Test]
        public void TestSimpleCase()
        {
            Container.Bind<ISaveHandler>().To<SaveHandler>().AsSingle();
            Container.Decorate<ISaveHandler>().With<SaveDecorator1>();

            CallCounter = 1;
            SaveHandler.CallCount = 0;
            SaveDecorator1.CallCount = 0;

            Container.Resolve<ISaveHandler>().Save();

            Assert.IsEqual(SaveDecorator1.CallCount, 1);
            Assert.IsEqual(SaveHandler.CallCount, 2);
        }

        [Test]
        public void TestMultiple()
        {
            Container.Bind<ISaveHandler>().To<SaveHandler>().AsSingle();
            Container.Decorate<ISaveHandler>().With<SaveDecorator1>();
            Container.Decorate<ISaveHandler>().With<SaveDecorator2>();

            CallCounter = 1;
            SaveHandler.CallCount = 0;
            SaveDecorator1.CallCount = 0;
            SaveDecorator2.CallCount = 0;

            Container.Resolve<ISaveHandler>().Save();

            Assert.IsEqual(SaveDecorator2.CallCount, 1);
            Assert.IsEqual(SaveDecorator1.CallCount, 2);
            Assert.IsEqual(SaveHandler.CallCount, 3);
        }
    }
}
