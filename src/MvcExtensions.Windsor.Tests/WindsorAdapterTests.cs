#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Windsor.Tests
{
    using System;
    using System.Web.Mvc;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using Moq;
    using Xunit;
    using Xunit.Extensions;

    public class WindsorAdapterTests
    {
        private readonly Mock<IWindsorContainer> container;
        private readonly WindsorAdapter adapter;

        public WindsorAdapterTests()
        {
            container = new Mock<IWindsorContainer>();
            adapter = new WindsorAdapter(container.Object);
        }

        [Fact]
        public void Dispose_should_also_dispose_container()
        {
            adapter.Dispose();

            container.Verify(c => c.Dispose());
        }

        [Theory]
        [InlineData(LifetimeType.PerRequest)]
        [InlineData(LifetimeType.Singleton)]
        [InlineData(LifetimeType.Transient)]
        public void Should_be_able_to_register_type(LifetimeType lifetime)
        {
            container.Setup(c => c.Register(It.IsAny<IRegistration[]>())).Verifiable();

            adapter.RegisterType(typeof(DummyObject), typeof(DummyObject), lifetime);

            container.Verify();
        }

        [Fact]
        public void Should_be_able_to_register_instance()
        {
            container.Setup(c => c.Register(It.IsAny<IRegistration[]>())).Verifiable();

            adapter.RegisterInstance(typeof(DummyObject), new DummyObject());

            container.Verify();
        }

        [Fact]
        public void Should_be_able_to_inject()
        {
            var dummy = new DummyObject2();

            container.Setup(c => c.Kernel.HasComponent(typeof(DummyObject))).Returns(true).Verifiable();
            container.Setup(c => c.Resolve(typeof(DummyObject))).Returns(new DummyObject()).Verifiable();

            adapter.Inject(dummy);

            container.Verify();
        }

        [Fact]
        public void Should_be_able_to_get_service_by_type()
        {
            container.Setup(x => x.Kernel.HasComponent(typeof(DummyObject))).Returns(true).Verifiable();
            container.Setup(c => c.Resolve(It.IsAny<Type>())).Verifiable();

            adapter.GetService<DummyObject>();

            container.Verify();
        }

        [Fact]
        public void Should_be_able_to_get_all_instances()
        {
            container.Setup(c => c.ResolveAll(It.IsAny<Type>())).Returns(new DummyObject[] { });

            adapter.GetServices(typeof(DummyObject));

            container.VerifyAll();
        }

        private sealed class DummyObject
        {
        }

        private sealed class DummyObject2
        {
            public DummyObject Dummy { get; set; }

            public string StringProperty { get; set; }
        }
    }
}