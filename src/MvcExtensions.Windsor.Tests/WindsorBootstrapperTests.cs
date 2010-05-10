#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Windsor.Tests
{
    using Castle.Windsor;

    using Moq;
    using Xunit;

    public class WindsorBootstrapperTests
    {
        [Fact]
        public void Should_be_able_to_create_service_locator()
        {
            var buildManager = new Mock<IBuildManager>();
            buildManager.SetupGet(bm => bm.Assemblies).Returns(new[] { GetType().Assembly });

            var bootstrapper = new WindsorBootstrapper(buildManager.Object);

            Assert.IsType<WindsorAdapter>(bootstrapper.ServiceLocator);
        }

        [Fact]
        public void Should_be_able_to_load_modules()
        {
            var buildManager = new Mock<IBuildManager>();
            buildManager.SetupGet(bm => bm.ConcreteTypes).Returns(new[] { typeof(DummyModule) });

            var bootstrapper = new WindsorBootstrapper(buildManager.Object);

            DummyModule.Loaded = false;

            Assert.IsType<WindsorAdapter>(bootstrapper.ServiceLocator);

            Assert.True(DummyModule.Loaded);
        }

        private sealed class DummyModule : IModule
        {
            public static bool Loaded { get; set; }

            public void Load(IWindsorContainer container)
            {
                Loaded = true;
            }
        }
    }
}