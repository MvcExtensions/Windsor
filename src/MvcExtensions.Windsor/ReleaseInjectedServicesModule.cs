#region Copyright
// Copyright (c) 2009 - 2012, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>, 2011 - 2012 hazzik <hazzik@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Windsor
{
    using System;
    using System.Web;

    internal class ReleaseInjectedServicesModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.EndRequest += ReleaseAllInjectedServices;
        }

        public void Dispose()
        {
        }

        private static void ReleaseAllInjectedServices(object sender, EventArgs args)
        {
            ((WindsorAdapter)Bootstrapper.Current.Adapter).ReleaseAllInjectedServices();
        }
    }
}
