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
        public IWindsorContainer Container
        {
            get;
            private set;
        }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns></returns>
        public override IServiceRegistrar RegisterType(string key, Type serviceType, Type implementationType, LifetimeType lifetime)
        {
            Invariant.IsNotNull(serviceType, "serviceType");
            Invariant.IsNotNull(implementationType, "implementationType");

            key = key ?? MakeKey(serviceType, implementationType);

            LifestyleType lifestyle = (lifetime == LifetimeType.PerRequest) ?
                                      LifestyleType.PerWebRequest :
                                      ((lifetime == LifetimeType.Singleton) ?
                                      LifestyleType.Singleton :
                                      LifestyleType.Transient);

            Container.Register(Component.For(serviceType).ImplementedBy(implementationType).Named(key).LifeStyle.Is(lifestyle));

            return this;
        }

        /// <summary>
        /// Registers the instance.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public override IServiceRegistrar RegisterInstance(string key, Type serviceType, object instance)
        {
            Invariant.IsNotNull(serviceType, "serviceType");
            Invariant.IsNotNull(instance, "instance");

            key = key ?? MakeKey(serviceType, instance.GetType());

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
                GetProperties(instance.GetType(), Container).Each(property => property.SetValue(instance, Container.Resolve(property.PropertyType), null));
            }
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected override object DoGetService(Type serviceType, string key)
        {
            return string.IsNullOrEmpty(key) ? Container.Resolve(serviceType) : Container.Resolve(serviceType, key);
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        protected override IEnumerable<object> DoGetServices(Type serviceType)
        {
            return Container.ResolveAll(serviceType).Cast<object>();
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
            return serviceType.FullName + "->" + implementationType.FullName;
        }
    }
}