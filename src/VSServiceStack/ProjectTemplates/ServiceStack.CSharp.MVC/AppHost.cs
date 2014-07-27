﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Funq;
using ServiceStack;
using Microsoft.AspNet.Identity.EntityFramework;
using ServiceStack.CSharp.MVC.ServiceInterface;


namespace ServiceStack.CSharp.MVC
{
    public class AppHost : AppHostBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost()
            : base("$safeprojectname$", typeof(MyServices).Assembly)
        {

        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            //Config examples
            //this.AddPlugin(new PostmanFeature());
            //this.AddPlugin(new CorsFeature());
        }
    }
}