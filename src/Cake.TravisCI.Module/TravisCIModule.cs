using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;
using Cake.TravisCI.Module;

[assembly: CakeModule(typeof(TravisCIModule))]

namespace Cake.TravisCI.Module
{
    /// <summary>
    /// <see cref="ICakeModule"/> implementation for Travis CI.
    /// </summary>
    public class TravisCIModule : ICakeModule
    {
        /// <inheritdoc cref="ICakeModule.Register"/>
        public void Register(ICakeContainerRegistrar registrar)
        {
            if (!string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("TRAVIS")))
            {
                registrar.RegisterType<TravisCIEngine>().As<ICakeEngine>().Singleton();
                registrar.RegisterType<TravisCILog>().As<ICakeLog>().Singleton();
            }
        }
    }
}
