using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;

[assembly: CakeModule(typeof(Cake.GitHubActions.Module.GitHubActionsModule))]

namespace Cake.GitHubActions.Module
{
    /// <summary>
    /// <see cref="ICakeModule"/> implementation for GitHub Actions.
    /// </summary>
    public class GitHubActionsModule : ICakeModule
    {
        /// <inheritdoc cref="ICakeModule.Register"/>
        public void Register(ICakeContainerRegistrar registrar)
        {
            if (string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("GITHUB_ACTIONS")))
            {
                return;
            }

            registrar.RegisterType<GitHubActionsEngine>().As<ICakeEngine>().Singleton();
            registrar.RegisterType<GitHubActionsLog>().As<ICakeLog>().Singleton();
        }
    }
}
