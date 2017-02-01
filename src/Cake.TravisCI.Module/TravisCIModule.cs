using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;
using Cake.TravisCI.Module;

[assembly: CakeModule(typeof(TravisCIModule))]

namespace Cake.TravisCI.Module
{
    class TravisCIModule : ICakeModule
    {
        public void Register(ICakeContainerRegistrar registrar)
        {
            if (!string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("TRAVIS")))
            {
                registrar.RegisterType<TravisCIEngine>().As<ICakeEngine>().Singleton();
                //registrar.RegisterType<TFBuildLog>().As<ICakeLog>().Singleton();
                //registrar.RegisterType<TFBuildReportPrinter>().As<ICakeReportPrinter>().Singleton();
            }
        }
    }
}