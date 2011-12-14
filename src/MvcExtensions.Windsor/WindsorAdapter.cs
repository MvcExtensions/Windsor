#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Windsor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Castle.Core;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    /// <summary>
    /// Defines an adapter class which is backed by Windsor <seealso cref="IWindsorContainer">Container</seealso>.
    /// </summary>
    public class WindsorAdapter : ContainerAdapter
    {
        private static readonly Dictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> propertyCache = new Dictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly object propertyCacheLock = new object();
        private readonly ICollection<object> injectedServices = new HashSet<object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorAdapter"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public WindsorAdapter(IWindsorContainer container)
        {
            Invariant.IsNotNull(container, "container");

            Container = container;
        }

        /// <summary>
        /// Gets the container.
        /// </summary>
        /// <value>The container.</value>
        public IWindsorContainer Container { get; private set; }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns></returns>
        public override IServiceRegistrar RegisterType(Type serviceType, Type implementationType, LifetimeType lifetime)
        {
            Invariant.IsNotNull(serviceType, "serviceType");
            Invariant.IsNotNull(implementationType, "implementationType");

            var key = MakeKey(serviceType, implementationType);
            LifestyleType lifestyle;
            switch (lifetime)
            {
                case LifetimeType.PerRequest:
                    lifestyle = LifestyleType.PerWebRequest;
                    break;
                case LifetimeType.Singleton:
                    lifestyle = LifestyleType.Singleton;
                    break;
                default:
                    lifestyle = LifestyleType.Transient;
                    break;
            }

            Container.Register(Component.For(serviceType).ImplementedBy(implementationType).Named(key).LifeStyle.Is(lifestyle));

            return this;
        }

        /// <summary>
        /// Registers the instance.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public override IServiceRegistrar RegisterInstance(Type serviceType, object instance)
        {
            Invariant.IsNotNull(serviceType, "serviceType");
            Invariant.IsNotNull(instance, "instance");

            var key = MakeKey(serviceType, instance.GetType());

            Container.Register(Component.For(serviceType).Named(key).Instance(instance));

            return this;
        }

        /// <summary>
        /// Injects the matching dependences.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public override void Inject(object instance)
        {
            if (instance != null)
            {
                GetProperties(instance.GetType(), Container)
                    .Each(property =>
                              {
                                  var dependency = Container.Resolve(property.PropertyType);
                                  injectedServices.Add(dependency);
                                  property.SetValue(instance, dependency, null);
                              });
            }
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public override object GetService(Type serviceType)
        {
            return Container.Kernel.HasComponent(serviceType) ? Container.Resolve(serviceType) : null;
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public override IEnumerable<object> GetServices(Type serviceType)
        {
            return Container.ResolveAll(serviceType).Cast<object>();
        }

        internal void ReleaseAllInjectedServices()
        {
            foreach (var service in injectedServices.ToList())
            {
                injectedServices.Remove(service);
                Container.Release(service);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        protected override void DisposeCore()
        {
            Container.Dispose();
        }

        private static IEnumerable<PropertyInfo> GetProperties(Type type, IWindsorContainer container)
        {
            RuntimeTypeHandle typeHandle = type.TypeHandle;
            IEnumerable<PropertyInfo> properties;

            if (!propertyCache.TryGetValue(typeHandle, out properties))
            {
                lock (propertyCacheLock)
                {
                    if (!propertyCache.TryGetValue(typeHandle, out properties))
                    {
                        Func<PropertyInfo, bool> filter = property =>
                                                              {
                                                                  if (property.CanWrite)
                                                                  {
                                                                      MethodInfo setMethod = property.GetSetMethod();

                                                                      if ((setMethod != null) && (setMethod.GetParameters().Length == 1))
                                                                      {
                                                                          if (container.Kernel.HasComponent(property.PropertyType))
                                                                          {
                                                                              return true;
                                                                          }
                                                                      }
                                                                  }

                                                                  return false;
                                                              };

                        properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
                            .Where(filter)
                            .ToList();

                        propertyCache.Add(typeHandle, properties);
                    }
                }
            }

            return properties;
        }

        private static string MakeKey(Type serviceType, Type implementationType)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}->{1}", serviceType.FullName, implementationType.FullName);
        }
    }
}