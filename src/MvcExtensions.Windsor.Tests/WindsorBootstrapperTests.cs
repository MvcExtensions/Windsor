#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Windsor.Tests
{
    using Castle.MicroKernel;
    using Castle.Windsor;

    using Moq;
    using Xunit;

    public class WindsorBootstrapperTests
    {
        [Fact]
        public void Should_be_able_to_create_adapter()
        {
            var buildManager = new Mock<IBuildManager>();
            buildManager.SetupGet(bm => bm.Assemblies).Returns(new[] { GetType().Assembly });

            var bootstrapper = new WindsorBootstrapper(buildManager.Object);

            Assert.IsType<WindsorAdapter>(bootstrapper.Adapter);
        }

        [Fact]
        public void Should_be_able_to_install_installer()
        {
            var buildManager = new Mock<IBuildManager>();
            buildManager.SetupGet(bm => bm.ConcreteTypes).Returns(new[] { typeof(DummyInstaller) });

            var bootstrapper = new WindsorBootstrapper(buildManager.Object);

            DummyInstaller.Installed = true;

            Assert.IsType<WindsorAdapter>(bootstrapper.Adapter);

            Assert.True(DummyInstaller.Installed);
        }

        private sealed class DummyInstaller : IWindsorInstaller
        {
            public static bool Installed { get; set; }

            public void Install(IWindsorContainer container, IConfigurationStore store)
            {
                Installed = true;
            }
        }
    }
}