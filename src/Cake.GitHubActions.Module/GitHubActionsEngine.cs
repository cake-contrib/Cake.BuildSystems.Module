using Cake.Common.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;

using JetBrains.Annotations;

namespace Cake.GitHubActions.Module
{
    /// <summary>
    /// <see cref="ICakeEngine"/> implementation for GitHub Actions.
    /// </summary>
    [UsedImplicitly]
    public sealed class GitHubActionsEngine : CakeEngineBase
    {
        private readonly IConsole _console;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubActionsEngine"/> class.
        /// </summary>
        /// <param name="dataService">Implementation of <see cref="ICakeDataService"/>.</param>
        /// <param name="log">Implementation of <see cref="ICakeLog"/>.</param>
        /// <param name="console">Implementation of <see cref="IConsole"/>.</param>
        public GitHubActionsEngine(ICakeDataService dataService, ICakeLog log, IConsole console)
            : base(new CakeEngine(dataService, log))
        {
            _console = console;
            _engine.BeforeSetup += OnBeforeSetup;
            _engine.AfterSetup += OnAfterSetup;
            _engine.BeforeTaskSetup += OnBeforeTaskSetup;
            _engine.BeforeTaskTeardown += OnBeforeTaskTeardown;
            _engine.BeforeTeardown += OnBeforeBuildTeardown;
            _engine.AfterTeardown += OnAfterBuildTeardown;
        }

        private void WriteStartBlock(BuildSystem buildSystem, string name)
        {
            if (!buildSystem.IsRunningOnGitHubActions)
            {
                return;
            }

            _console.WriteLine($"::group::{name}");
        }

        private void WriteEndBlock(BuildSystem buildSystem)
        {
            if (!buildSystem.IsRunningOnGitHubActions)
            {
                return;
            }

            _console.WriteLine("::endgroup::");
        }

        private void OnBeforeSetup(object sender, BeforeSetupEventArgs e)
        {
            WriteStartBlock(e.Context.BuildSystem(), "Setup");
        }

        private void OnAfterSetup(object sender, AfterSetupEventArgs e)
        {
            WriteEndBlock(e.Context.BuildSystem());
        }

        private void OnBeforeTaskSetup(object sender, BeforeTaskSetupEventArgs e)
        {
            WriteStartBlock(e.TaskSetupContext.BuildSystem(), e.TaskSetupContext.Task.Name);
        }

        private void OnBeforeTaskTeardown(object sender, BeforeTaskTeardownEventArgs e)
        {
            WriteEndBlock(e.TaskTeardownContext.BuildSystem());
        }

        private void OnBeforeBuildTeardown(object sender, BeforeTeardownEventArgs e)
        {
            WriteStartBlock(e.TeardownContext.BuildSystem(), "Teardown");
        }

        private void OnAfterBuildTeardown(object sender, AfterTeardownEventArgs e)
        {
            WriteEndBlock(e.TeardownContext.BuildSystem());
        }
    }
}
