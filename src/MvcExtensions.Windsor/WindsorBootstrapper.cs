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
    using Castle.Facilities.FactorySupport;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Releasers;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;

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
        /// <param name="bootstrapperTasks">The bootstrapper tasks.</param>
        /// <param name="perRequestTasks">The per request tasks.</param>
        public WindsorBootstrapper(IBuildManager buildManager, IBootstrapperTasksRegistry bootstrapperTasks, IPerRequestTasksRegistry perRequestTasks) : base(buildManager, bootstrapperTasks, perRequestTasks)
        {
        }

        /// <summary>
        /// Creates the container adapter.
        /// </summary>
        /// <returns></returns>
        protected override ContainerAdapter CreateAdapter()
        {
            IWindsorContainer container = new WindsorContainer();

            // we don't want to allow windsor to track mvc infrastructure stuff, sorry for this:(
            container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();

            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel));
            container.AddFacility<FactorySupportFacility>();

            var parent = new WindsorContainer();
            parent.Kernel.Resolver.AddSubResolver(new ArrayResolver(parent.Kernel));
            parent.AddFacility<FactorySupportFacility>();
            parent.AddChildContainer(container);

            container.Register(Component.For<HttpContextBase>().LifeStyle.Transient.UsingFactoryMethod(() => new HttpContextWrapper(HttpContext.Current)));

            var adapter = new WindsorAdapter(container, parent);

            return adapter;
        }

        /// <summary>
        /// Loads the container specific modules.
        /// </summary>
        protected override void LoadModules()
        {
            var parent = ((WindsorAdapter)Adapter).Parent;

            IWindsorInstaller[] windsorInstallers = BuildManager.ConcreteTypes
                .Where(type => installerType.IsAssignableFrom(type) && type.HasDefaultConstructor())
                .Select(Activator.CreateInstance)
                .Cast<IWindsorInstaller>()
                .ToArray();

            parent.Install(windsorInstallers);
        }
    }
}