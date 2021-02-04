using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;
using Cake.TravisCI.Module;

[assembly: CakeModule(typeof(TravisCIModule))]

namespace Cake.TravisCI.Module
{
    public class TravisCIModule : ICakeModule
    {
        public void Register(ICakeContainerRegistrar registrar)
        {
            if (!string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("TRAVIS")))
            {
                registrar.RegisterType<TravisCIEngine>().As<ICakeEngine>().Singleton();
                registrar.RegisterType<TravisCILog>().As<ICakeLog>().Singleton();
                //registrar.RegisterType<TravisCIReportPrinter>().As<ICakeReportPrinter>().Singleton();
            }
        }
    }
}