#region Copyright
// Copyright (c) 2009 - 2012, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>, 2011 - 2012 hazzik <hazzik@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using MvcExtensions.Windsor;

[assembly: AssemblyTitle("MvcExtensions.Windsor")]
[assembly: AssemblyProduct("MvcExtensions.Windsor")]
[assembly: CLSCompliant(true)]
[assembly: Guid("ab5c12b5-472c-4fea-9449-13ba81fc3a89")]
[assembly: PreApplicationStartMethod(typeof(WindsorBootstrapper), "Run")]
