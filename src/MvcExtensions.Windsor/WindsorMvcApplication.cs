#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Windsor
{
    using System.Web;

    using Castle.MicroKernel.Lifestyle;

    /// <summary>
    /// Defines a <see cref="HttpApplication"/> which uses <seealso cref="WindsorBootstrapper"/>.
    /// </summary>
    public class WindsorMvcApplication : ExtendedMvcApplication
    {
        /// <summary>
        /// Executes custom initialization code after all event handler modules have been added.
        /// </summary>
        public override void Init()
        {
            base.Init();
            new PerWebRequestLifestyleModule().Init(this);
            EndRequest += (sender, args) => ((WindsorAdapter)Adapter).ReleaseAllInjectedServices(); 
        }

        /// <summary>
        /// Creates the bootstrapper.
        /// </summary>
        /// <returns></returns>
        protected override IBootstrapper CreateBootstrapper()
        {
            return new WindsorBootstrapper(BuildManagerWrapper.Current, BootstrapperTasksRegistry.Current, PerRequestTasksRegistry.Current);
        }
    }
}