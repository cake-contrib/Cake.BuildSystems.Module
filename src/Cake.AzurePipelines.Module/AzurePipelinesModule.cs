using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;

[assembly: CakeModule(typeof(Cake.AzurePipelines.Module.AzurePipelinesModule))]

namespace Cake.AzurePipelines.Module
{
    /// <summary>
    /// <see cref="ICakeModule"/> implementation for Azure Pipelines.
    /// </summary>
    public class AzurePipelinesModule : ICakeModule
    {
        /// <inheritdoc cref="ICakeModule.Register"/>
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
