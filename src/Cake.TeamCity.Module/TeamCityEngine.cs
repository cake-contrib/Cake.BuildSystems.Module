using Cake.Common.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;
using JetBrains.Annotations;

namespace Cake.TeamCity.Module
{
    /// <summary>
    /// <see cref="ICakeEngine"/> implementation for TeamCity.
    /// </summary>
    [UsedImplicitly]
    public sealed class TeamCityEngine : CakeEngineBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCityEngine"/> class.
        /// </summary>
        /// <param name="dataService">Implementation of <see cref="ICakeDataService"/>.</param>
        /// <param name="log">Implementation of <see cref="ICakeLog"/>.</param>
        public TeamCityEngine(ICakeDataService dataService, ICakeLog log)
            : base(new CakeEngine(dataService, log))
        {
            _engine.Setup += OnBuildSetup;
            _engine.TaskSetup += OnTaskSetup;
            _engine.TaskTeardown += OnTaskTeardown;
            _engine.Teardown += OnBuildTeardown;
        }

        private void OnBuildTeardown(object sender, TeardownEventArgs e)
        {
            var b = e.TeardownContext.BuildSystem();
            if (b.IsRunningOnTeamCity)
            {
                var tc = b.TeamCity;
                tc.WriteEndBlock("Cake Build");
            }
        }

        private void OnTaskTeardown(object sender, TaskTeardownEventArgs e)
        {
            var b = e.TaskTeardownContext.BuildSystem();
            if (b.IsRunningOnTeamCity)
            {
                var tc = b.TeamCity;
                var duration = e.TaskTeardownContext.Duration.TotalMilliseconds.ToString("0");

                // we really should add build statistic values to the TeamCity stuff in Cake, but this will do for now.
                e.TaskTeardownContext.Log.Information($"##teamcity[buildStatisticValue key='Block.{e.TaskTeardownContext.Task.Name}.Duration' value='{duration}']");
                tc.WriteEndProgress($"Completed running {e.TaskTeardownContext.Task.Name} task");
                tc.WriteEndBlock(e.TaskTeardownContext.Task.Name);
            }
        }

        private void OnTaskSetup(object sender, TaskSetupEventArgs e)
        {
            var b = e.TaskSetupContext.BuildSystem();
            if (b.IsRunningOnTeamCity)
            {
                var tc = b.TeamCity;
                tc.WriteStartBlock(e.TaskSetupContext.Task.Name);
                tc.WriteStartProgress($"Running task {e.TaskSetupContext.Task.Name}");
                if (!string.IsNullOrWhiteSpace(e.TaskSetupContext.Task.Description))
                {
                    tc.WriteStartProgress(e.TaskSetupContext.Task.Description);
                }
            }
        }

        private void OnBuildSetup(object sender, SetupEventArgs setupEventArgs)
        {
            var b = setupEventArgs.Context.BuildSystem();
            if (b.IsRunningOnTeamCity)
            {
                var tc = b.TeamCity;
                tc.WriteStartBlock("Cake Build");
            }
        }
    }
}
