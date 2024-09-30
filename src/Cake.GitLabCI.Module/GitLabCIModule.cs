using System;

using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;

[assembly: CakeModule(typeof(Cake.GitLabCI.Module.GitLabCIModule))]

namespace Cake.GitLabCI.Module
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
                registrar.RegisterType<GitLabCIEngine>().As<ICakeEngine>().Singleton();
            }
        }
    }
}
