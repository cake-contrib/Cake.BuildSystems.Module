using System;

using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;

[assembly: CakeModule(typeof(Cake.AzurePipelines.Module.GitLabCIModule))]

namespace Cake.AzurePipelines.Module
{
    /// <summary>
    /// <see cref="ICakeModule"/> implementation for GitLab CI.
    /// </summary>
    public class GitLabCIModule : ICakeModule
    {
        /// <inheritdoc cref="ICakeModule.Register"/>
        public void Register(ICakeContainerRegistrar registrar)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(Environment.GetEnvironmentVariable("CI_SERVER"), "yes"))
            {
                registrar.RegisterType<GitLabCILog>().As<ICakeLog>().Singleton();
            }
        }
    }
}
