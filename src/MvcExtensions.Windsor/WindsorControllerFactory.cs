#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Windsor
{
    using System.Web.Mvc;

    /// <summary>
    /// Represents specific controller factory for Castle.Windsor container
    /// </summary>
    public class WindsorControllerFactory : DefaultControllerFactory
    {
        private readonly ContainerAdapter container;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorControllerFactory"/> class.
        /// </summary>
        /// <param name="container"></param>
        public WindsorControllerFactory(ContainerAdapter container)
        {
            this.container = container;
        }

        /// <summary>
        /// Releases the specified controller.
        /// </summary>
        /// <param name="controller">The controller to release.</param>
        public override void ReleaseController(IController controller)
        {
            ((WindsorAdapter)container).Container.Release(controller);
        }
    }
}