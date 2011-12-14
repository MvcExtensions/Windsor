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

    using Castle.Windsor;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Facilities.FactorySupport;

    /// <summary>
    /// Defines a <seealso cref="Bootstrapper">Bootstrapper</seealso> which is backed by <seealso cref="WindsorAdapter"/>.
    /// </summary>
    public class WindsorBootstrapper : Bootstrapper
    {
        private static readonly Type installerType = typeof(IWindsorInstaller);

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorBootstrapper"/> class.
        /// </summary>
        /// <param name="buildManager">The build manager.</param>
        public WindsorBootstrapper(IBuildManager buildManager) : base(buildManager)
        {
        }

        /// <summary>
        /// Creates the container adapter.
        /// </summary>
        /// <returns></returns>
        protected override ContainerAdapter CreateAdapter()
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
            IWindsorContainer container = ((WindsorAdapter)Adapter).Container;

            BuildManager.ConcreteTypes
                        .Where(type => installerType.IsAssignableFrom(type) && type.HasDefaultConstructor())
                        .Select(type => Activator.CreateInstance(type))
                        .Cast<IWindsorInstaller>()
                        .Each(installer => installer.Install(container, null));
        }
    }
}