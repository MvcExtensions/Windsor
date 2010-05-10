#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Windsor
{
    using Castle.Windsor;

    /// <summary>
    /// Represents an interface that is used to register a set of related services.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Loads the module.
        /// </summary>
        /// <param name="container">The container.</param>
        void Load(IWindsorContainer container);
    }
}