#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Windsor.Tests
{
    using System;

    using Castle.Core;
    using Castle.Windsor;

    using Moq;
    using Xunit;
    using Xunit.Extensions;

    public class WindsorAdapterTests
    {
        private readonly Mock<IWindsorContainer> container;
        private WindsorAdapter adapter;

        public WindsorAdapterTests()
        {
            container = new Mock<IWindsorContainer>();
            adapter = new WindsorAdapter(container.Object);
        }

        [Fact]
        public void Dispose_should_also_dispose_container()
        {
            container.Setup(c => c.Dispose());

            adapter.Dispose();

            container.VerifyAll();
        }

        [Fact]
        public void Should_finalize()
        {
            adapter = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [Theory]
        [InlineData(LifetimeType.PerRequest, "foo")]
        [InlineData(LifetimeType.Singleton, null)]
        [InlineData(LifetimeType.Transient, "")]
        public void Should_be_able_to_register_type(LifetimeType lifetime, string key)
        {
            LifestyleType lifestyle = (lifetime == LifetimeType.PerRequest) ?
                                      LifestyleType.PerWebRequest :
                                      ((lifetime == LifetimeType.Singleton) ?
                                      LifestyleType.Singleton :
                                      LifestyleType.Transient);

            container.Setup(c => c.AddComponentLifeStyle(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Type>(), lifestyle)).Verifiable();

            adapter.RegisterType(key, typeof(DummyObject), typeof(DummyObject), lifetime);

            container.Verify();
        }

        [Fact]
        public void Should_be_able_to_register_instance()
        {
            container.Setup(c => c.Kernel.AddComponentInstance(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<object>())).Verifiable();

            adapter.RegisterInstance("foo", typeof(DummyObject), new DummyObject());

            container.Verify();
        }

        [Fact]
        public void Should_be_able_to_inject()
        {
            var dummy = new DummyObject2();

            container.Setup(c => c.Kernel.HasComponent(typeof(DummyObject))).Returns(true);
            container.Setup(c => c.Resolve(typeof(DummyObject))).Returns(new DummyObject());

            adapter.Inject(dummy);

            container.VerifyAll();
        }

        [Fact]
        public void Should_be_able_to_get_instance_by_type()
        {
            container.Setup(c => c.Resolve(It.IsAny<Type>()));

            adapter.GetInstance<DummyObject>();

            container.VerifyAll();
        }

        [Fact]
        public void Should_be_able_to_get_instance_by_type_and_key()
        {
            container.Setup(c => c.Resolve(It.IsAny<Type>(), It.IsAny<string>()));

            adapter.GetInstance<DummyObject>("foo");

            container.VerifyAll();
        }

        [Fact]
        public void Should_be_able_to_get_all_instances()
        {
            container.Setup(c => c.ResolveAll(It.IsAny<Type>())).Returns(new DummyObject[] { });

            adapter.GetAllInstances(typeof(DummyObject));

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