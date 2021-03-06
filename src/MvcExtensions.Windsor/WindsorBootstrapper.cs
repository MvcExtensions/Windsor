#region Copyright
// Copyright (c) 2009 - 2012, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>, 2011 - 2012 hazzik <hazzik@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Windsor
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using Castle.Facilities.FactorySupport;
    using Castle.MicroKernel.Lifestyle;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

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
        public WindsorBootstrapper(IBuildManager buildManager, IBootstrapperTasksRegistry bootstrapperTasks, IPerRequestTasksRegistry perRequestTasks)
            : base(buildManager, bootstrapperTasks, perRequestTasks)
        {
        }

        /// <summary>
        /// Starts this bootstrapper
        /// </summary>
        public static void Run()
        {
            Current = new WindsorBootstrapper(BuildManagerWrapper.Current, BootstrapperTasksRegistry.Current, PerRequestTasksRegistry.Current);
            DynamicModuleUtility.RegisterModule(typeof(BootstrapperModule));
            DynamicModuleUtility.RegisterModule(typeof(PerWebRequestLifestyleModule));
            DynamicModuleUtility.RegisterModule(typeof(ReleaseInjectedServicesModule));
        }

        /// <summary>
        /// Creates the container adapter.
        /// </summary>
        /// <returns></returns>
        protected override ContainerAdapter CreateAdapter()
        {
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel));
            container.AddFacility<FactorySupportFacility>();

            container.Register(Component.For<HttpContextBase>().LifeStyle.Transient.UsingFactoryMethod(() => new HttpContextWrapper(HttpContext.Current)));
            container.Register(Component.For<IControllerFactory>().LifeStyle.Singleton.ImplementedBy<WindsorControllerFactory>());

            var adapter = new WindsorAdapter(container);

            return adapter;
        }

        /// <summary>
        /// Loads the container specific modules.
        /// </summary>
        protected override void LoadModules()
        {
            IWindsorContainer container = ((WindsorAdapter)Adapter).Container;

            IWindsorInstaller[] windsorInstallers = BuildManager.ConcreteTypes
                .Where(type => installerType.IsAssignableFrom(type) && type.HasDefaultConstructor())
                .Select(Activator.CreateInstance)
                .Cast<IWindsorInstaller>()
                .ToArray();

            container.Install(windsorInstallers);
        }
    }
}
