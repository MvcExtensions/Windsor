#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Windsor
{
    using System;
    using System.Linq;
    using System.Web;

    using Microsoft.Practices.ServiceLocation;

    using Castle.Windsor;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;

    using Component = Castle.MicroKernel.Registration.Component;
    using FactorySupportFacility = Castle.Facilities.FactorySupport.FactorySupportFacility;

    /// <summary>
    /// Defines a <seealso cref="Bootstrapper">Bootstrapper</seealso> which is backed by <seealso cref="WindsorAdapter"/>.
    /// </summary>
    public class WindsorBootstrapper : Bootstrapper
    {
        private static readonly Type moduleType = typeof(IModule);

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorBootstrapper"/> class.
        /// </summary>
        /// <param name="buildManager">The build manager.</param>
        public WindsorBootstrapper(IBuildManager buildManager) : base(buildManager)
        {
        }

        /// <summary>
        /// Creates the service locator.
        /// </summary>
        /// <returns></returns>
        protected override IServiceLocator CreateServiceLocator()
        {
            IWindsorContainer container = new WindsorContainer();

            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel));
            container.AddFacility<FactorySupportFacility>();

            container.Register(Component.For<HttpContextBase>().LifeStyle.Transient.UsingFactoryMethod(() => new HttpContextWrapper(HttpContext.Current)));

            WindsorAdapter adapter = new WindsorAdapter(container);

            return adapter;
        }

        /// <summary>
        /// Loads the container specific modules.
        /// </summary>
        protected override void LoadModules()
        {
            IWindsorContainer container = ((WindsorAdapter)ServiceLocator).Container;

            BuildManager.ConcreteTypes
                        .Where(type => moduleType.IsAssignableFrom(type) && type.HasDefaultConstructor())
                        .Select(type => Activator.CreateInstance(type))
                        .Cast<IModule>()
                        .Each(module => module.Load(container));
        }
    }
}