using System;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;
using Cake.MyGet.Module;

[assembly: CakeModule(typeof(MyGetModule))]

namespace Cake.MyGet.Module
{
    /// <summary>
    /// Implementation of <see cref="ICakeModule"/> for MyGet.
    /// </summary>
    public class MyGetModule : ICakeModule
    {
        /// <inheritdoc cref="ICakeModule.Register"/>
        public void Register(ICakeContainerRegistrar registrar)
        {
            var buildRunner = Environment.GetEnvironmentVariable("BuildRunner");
            if (!string.IsNullOrWhiteSpace(buildRunner) && string.Equals(buildRunner, "MyGet", StringComparison.OrdinalIgnoreCase))
            {
                registrar.RegisterType<MyGetEngine>().As<ICakeEngine>().Singleton();
                registrar.RegisterType<MyGetBuildLog>().As<ICakeLog>().Singleton();
                registrar.RegisterType<MyGetReportPrinter>().As<ICakeReportPrinter>().Singleton();
            }
        }
    }
}
