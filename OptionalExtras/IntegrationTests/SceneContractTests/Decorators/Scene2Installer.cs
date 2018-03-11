using UnityEngine;
using Zenject;

namespace Zenject.Tests.DecoratorTests
{
    public class Bar
    {
    }

    public class Foo
    {
        public Foo(Bar bar)
        {
        }
    }

    public class Scene2Installer : MonoInstaller<Scene2Installer>
    {
        public override void InstallBindings()
        {
            Container.Bind<Foo>().AsSingle();
        }
    }
}
