using System;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;
using Cake.MyGet.Module;

[assembly: CakeModule(typeof(MyGetModule))]

namespace Cake.MyGet.Module
{
    class MyGetModule : ICakeModule
    {
        public void Register(ICakeContainerRegistrar registrar)
        {
            var buildRunner = System.Environment.GetEnvironmentVariable("BuildRunner");
            if (!string.IsNullOrWhiteSpace(buildRunner) && String.Equals(buildRunner, "MyGet", StringComparison.OrdinalIgnoreCase))
            {
                registrar.RegisterType<MyGetEngine>().As<ICakeEngine>().Singleton();
                registrar.RegisterType<MyGetBuildLog>().As<ICakeLog>().Singleton();
                registrar.RegisterType<MyGetReportPrinter>().As<ICakeReportPrinter>().Singleton();
            }
        }
    }
}