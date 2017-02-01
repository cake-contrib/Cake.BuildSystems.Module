using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cake.Common.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;

namespace Cake.TravisCI.Module
{
    public class TravisCIEngine : CakeEngineBase
    {
        private readonly string _buildMessage;
        public TravisCIEngine(ICakeLog log) : base(new CakeEngine(log))
        {
            _engine.Setup += OnBuildSetup;
            _engine.TaskSetup += OnTaskSetup;
            _engine.TaskTeardown += OnTaskTeardown;
            _engine.Teardown += OnBuildTeardown;
            _buildMessage = "Cake";
            //_buildMessage = $"Cake Build (running {_engine.Tasks.Count} tasks)";
        }

        private void OnBuildTeardown(object sender, TeardownEventArgs e)
        {
            var b = e.TeardownContext.BuildSystem();
            if (b.IsRunningOnTravisCI)
            {
                var tr = b.TravisCI;
                tr.WriteEndFold(_buildMessage.ToFoldMessage());
            }
        }

        private void OnTaskTeardown(object sender, TaskTeardownEventArgs e)
        {
            var b = e.TaskTeardownContext.BuildSystem();
            if (b.IsRunningOnTravisCI)
            {
                var tr = b.TravisCI;
                tr.WriteEndFold(e.TaskTeardownContext.Task.Name.ToFoldMessage());
            }
        }

        private void OnTaskSetup(object sender, TaskSetupEventArgs e)
        {
            var b = e.TaskSetupContext.BuildSystem();
            if (b.IsRunningOnTravisCI)
            {
                var tr = b.TravisCI;
                tr.WriteStartFold(e.TaskSetupContext.Task.Name.ToFoldMessage());
            }
        }

        private void OnBuildSetup(object sender, SetupEventArgs e)
        {
            var b = e.Context.BuildSystem();
            if (b.IsRunningOnTravisCI)
            {
                var tr = b.TravisCI;
                tr.WriteStartFold(_buildMessage.ToFoldMessage());
            }
        }
    }
}