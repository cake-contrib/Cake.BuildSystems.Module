using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;

[assembly: CakeModule(typeof(Cake.AzurePipelines.Module.AzurePipelinesModule))]

namespace Cake.AzurePipelines.Module
{
    public class AzurePipelinesModule : ICakeModule
    {
        public void Register(ICakeContainerRegistrar registrar)
        {
            if (!string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                registrar.RegisterType<AzurePipelinesEngine>().As<ICakeEngine>().Singleton();
                registrar.RegisterType<AzurePipelinesLog>().As<ICakeLog>().Singleton();
                registrar.RegisterType<AzurePipelinesReportPrinter>().As<ICakeReportPrinter>().Singleton();
            }
        }
    }
}