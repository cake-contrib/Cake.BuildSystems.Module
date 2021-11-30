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
            _engine.BeforeSetup += OnBeforeSetup;
            _engine.AfterSetup += OnAfterSetup;
            _engine.BeforeTaskSetup += OnBeforeTaskSetup;
            _engine.BeforeTaskTeardown += OnBeforeTaskTeardown;
            _engine.BeforeTeardown += OnBeforeBuildTeardown;
            _engine.AfterTeardown += OnAfterBuildTeardown;
        }

        private void OnBeforeSetup(object sender, BeforeSetupEventArgs e)
        {
            var b = e.Context.BuildSystem();
            if (b.IsRunningOnTeamCity)
            {
                var tc = b.TeamCity;
                tc.WriteStartBlock("Cake Build");
                tc.WriteStartBlock("Setup");
                tc.WriteStartProgress("Running Setup");
            }
        }

        private void OnAfterSetup(object sender, AfterSetupEventArgs e)
        {
            var b = e.Context.BuildSystem();
            if (b.IsRunningOnTeamCity)
            {
                var tc = b.TeamCity;

                // we should write out a duration here for statistics, but cake doesn't include it
                tc.WriteEndProgress("Completed running Setup");
                tc.WriteEndBlock("Setup");
            }
        }

        private void OnBeforeTaskSetup(object sender, BeforeTaskSetupEventArgs e)
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

        private void OnBeforeTaskTeardown(object sender, BeforeTaskTeardownEventArgs e)
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

        private void OnBeforeBuildTeardown(object sender, BeforeTeardownEventArgs e)
        {
            var b = e.TeardownContext.BuildSystem();
            if (b.IsRunningOnTeamCity)
            {
                var tc = b.TeamCity;
                tc.WriteStartBlock("Teardown");
                tc.WriteStartProgress("Running Teardown");
            }
        }

        private void OnAfterBuildTeardown(object sender, AfterTeardownEventArgs e)
        {
            var b = e.TeardownContext.BuildSystem();
            if (b.IsRunningOnTeamCity)
            {
                var tc = b.TeamCity;

                // we should write out a duration here for statistics, but cake doesn't include it
                tc.WriteEndProgress("Completed running Teardown");
                tc.WriteEndBlock("Teardown");
                tc.WriteEndBlock("Cake Build");
            }
        }
    }
}
