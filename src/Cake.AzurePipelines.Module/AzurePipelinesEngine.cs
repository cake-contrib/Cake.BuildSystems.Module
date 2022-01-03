using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Common.Build;
using Cake.Common.Build.AzurePipelines.Data;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;
using JetBrains.Annotations;

namespace Cake.AzurePipelines.Module
{
    /// <summary>
    /// <see cref="ICakeEngine"/> implementation for Azure Pipelines.
    /// </summary>
    [UsedImplicitly]
    public sealed class AzurePipelinesEngine : CakeEngineBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzurePipelinesEngine"/> class.
        /// </summary>
        /// <param name="dataService">Implementation of <see cref="ICakeDataService"/>.</param>
        /// <param name="log">Implementation of <see cref="ICakeLog"/>.</param>
        public AzurePipelinesEngine(ICakeDataService dataService, ICakeLog log)
            : base(new CakeEngine(dataService, log))
        {
            _log = log;
            _engine.BeforeSetup += OnBeforeSetup;
            _engine.AfterSetup += OnAfterSetup;

            _engine.BeforeTaskSetup += OnBeforeTaskSetup;

            _engine.BeforeTaskTeardown += OnBeforeTaskTeardown;

            _engine.AfterTaskTeardown += OnAfterTaskTeardown;

            _engine.BeforeTeardown += OnBeforeTeardown;
            _engine.AfterTeardown += OnAfterTeardown;
        }

        private void OnAfterTaskTeardown(object sender, AfterTaskTeardownEventArgs e)
        {
            var b = e.TaskTeardownContext.BuildSystem();
            if (b.IsRunningOnPipelines())
            {
                WriteGroupEndCommand();
            }
        }

        private void OnAfterTeardown(object sender, AfterTeardownEventArgs e)
        {
            var b = e.TeardownContext.BuildSystem();
            if (b.IsRunningOnPipelines())
            {
                WriteGroupEndCommand();
            }
        }

        private void OnAfterSetup(object sender, AfterSetupEventArgs e)
        {
            var b = e.Context.BuildSystem();
            if (b.IsRunningOnPipelines())
            {
                WriteGroupEndCommand();
            }
        }

        private void OnBeforeTeardown(object sender, BeforeTeardownEventArgs e)
        {
            var b = e.TeardownContext.BuildSystem();
            if (b.IsRunningOnPipelines())
            {
                WriteGroupCommand("Teardown");

                b.AzurePipelines.Commands.UpdateRecord(_parentRecord, new AzurePipelinesRecordData
                {
                    FinishTime = DateTime.Now,
                    Status = AzurePipelinesTaskStatus.Completed,
                    Result = e.TeardownContext.Successful ? AzurePipelinesTaskResult.Succeeded : AzurePipelinesTaskResult.Failed,
                    Progress = GetProgress(TaskRecords.Count, _engine.Tasks.Count),
                });
            }
        }

        private void OnBeforeTaskTeardown(object sender, BeforeTaskTeardownEventArgs e)
        {
            var b = e.TaskTeardownContext.BuildSystem();
            if (b.IsRunningOnPipelines())
            {
                var currentTask = _engine.Tasks.First(t => t.Name == e.TaskTeardownContext.Task.Name);
                var currentIndex = _engine.Tasks.ToList().IndexOf(currentTask);
                var g = TaskRecords[currentTask.Name];
                b.AzurePipelines.Commands.UpdateRecord(g,
                    new AzurePipelinesRecordData
                    {
                        FinishTime = DateTime.Now,
                        Progress = 100,
                        Result = GetTaskResult(e.TaskTeardownContext),
                    });
            }
        }

        private AzurePipelinesTaskResult? GetTaskResult(ITaskTeardownContext taskTeardownContext)
        {
            if (taskTeardownContext.Skipped)
            {
                return AzurePipelinesTaskResult.Skipped;
            }

            // TODO: this logic should be improved but is difficult without task status in the context
            return AzurePipelinesTaskResult.Succeeded;
        }

        private void OnBeforeTaskSetup(object sender, BeforeTaskSetupEventArgs e)
        {
            var b = e.TaskSetupContext.BuildSystem();
            if (b.IsRunningOnPipelines())
            {
                WriteGroupCommand(e.TaskSetupContext.Task.Name);

                var currentTask =
                    _engine.Tasks.First(t => t.Name == e.TaskSetupContext.Task.Name);
                var currentIndex = _engine.Tasks.ToList().IndexOf(currentTask);
                b.AzurePipelines.UpdateProgress(_parentRecord, GetProgress(currentIndex, _engine.Tasks.Count));
                b.AzurePipelines.Commands.SetProgress(GetProgress(currentIndex, _engine.Tasks.Count), string.Empty);
                var g = e.TaskSetupContext.AzurePipelines()
                    .Commands.CreateNewRecord(currentTask.Name, "build", TaskRecords.Count + 1,
                        new AzurePipelinesRecordData { StartTime = DateTime.Now, ParentRecord = _parentRecord, Progress = 0 });
                TaskRecords.Add(currentTask.Name, g);
            }
        }

        private int GetProgress(int currentTask, int count)
        {
            var f = currentTask / (double)count * 100;
            return Convert.ToInt32(Math.Truncate(f));
        }

        private void OnBeforeSetup(object sender, BeforeSetupEventArgs e)
        {
            var b = e.Context.BuildSystem();
            if (b.IsRunningOnPipelines())
            {
                WriteGroupCommand("Setup");

                e.Context.AzurePipelines().Commands.SetProgress(0, string.Empty);
                var g = e.Context.AzurePipelines()
                    .Commands.CreateNewRecord("Cake Build", "build", 0, new AzurePipelinesRecordData { StartTime = DateTime.Now });
                _parentRecord = g;
            }
        }

        private void WriteGroupCommand(string groupName)
        {
            _log.Verbose(string.Empty);
            _log.Information("##[group]{0}", groupName);
        }

        private void WriteGroupEndCommand()
        {
            _log.Information("##[endgroup]");
        }

        private Guid _parentRecord;
        private ICakeLog _log;

        private Dictionary<string, Guid> TaskRecords { get; } = new Dictionary<string, Guid>();
    }
}
