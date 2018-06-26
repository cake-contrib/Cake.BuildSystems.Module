using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Common.Build;
using Cake.Common.Build.TFBuild.Data;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;

namespace Cake.TFBuild.Module
{
    /// <summary>
    /// Represents a Cake engine for use with the TF Build engine.
    /// </summary>
    public sealed class TFBuildEngine : CakeEngineBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TFBuildEngine"/> type.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="log">The log.</param>
        public TFBuildEngine(ICakeDataService dataService, ICakeLog log) : base(new CakeEngine(dataService, log))
        {
            _engine.Setup += BuildSetup;
            _engine.TaskSetup += OnTaskSetup;
            _engine.TaskTeardown += OnTaskTeardown;
            _engine.Teardown += OnBuildTeardown;
        }

        private void OnBuildTeardown(object sender, TeardownEventArgs e)
        {
            var b = e.TeardownContext.BuildSystem();
            if (b.IsRunningOnVSTS || b.IsRunningOnTFS)
            {
                b.TFBuild.Commands.UpdateRecord(_parentRecord, new TFBuildRecordData
                {
                    FinishTime = DateTime.Now,
                    Status = TFBuildTaskStatus.Completed,
                    Result = e.TeardownContext.Successful ? TFBuildTaskResult.Succeeded : TFBuildTaskResult.Failed,
                    Progress = GetProgress(TaskRecords.Count, _engine.Tasks.Count),
                });
            }
        }

        private void OnTaskTeardown(object sender, TaskTeardownEventArgs e)
        {
            var b = e.TaskTeardownContext.BuildSystem();
            if (b.IsRunningOnVSTS || b.IsRunningOnTFS)
            {
                var currentTask = _engine.Tasks.First(t => t.Name == e.TaskTeardownContext.Task.Name);
                var currentIndex = _engine.Tasks.ToList().IndexOf(currentTask);
                //b.TFBuild.UpdateProgress(_parentRecord, GetProgress(currentIndex, _engine.Tasks.Count));
                var g = TaskRecords[currentTask.Name];
                b.TFBuild.Commands.UpdateRecord(g,
                    new TFBuildRecordData
                    {
                        FinishTime = DateTime.Now,
                        Progress = 100,
                        Result = GetTaskResult(e.TaskTeardownContext)
                    });
            }
        }

        private TFBuildTaskResult? GetTaskResult(ITaskTeardownContext taskTeardownContext)
        {
            if (taskTeardownContext.Skipped) return TFBuildTaskResult.Skipped;

            // TODO: this logic should be improved but is difficult without task status in the context
            return TFBuildTaskResult.Succeeded;
        }

        private void OnTaskSetup(object sender, TaskSetupEventArgs e)
        {
            var b = e.TaskSetupContext.BuildSystem();
            if (b.IsRunningOnVSTS || b.IsRunningOnTFS)
            {
                var currentTask =
                    _engine.Tasks.First(t => t.Name == e.TaskSetupContext.Task.Name);
                var currentIndex = _engine.Tasks.ToList().IndexOf(currentTask);
                b.TFBuild.UpdateProgress(_parentRecord, GetProgress(currentIndex, _engine.Tasks.Count));
                //b.TFBuild.Commands.SetProgress(GetProgress(currentIndex, _engine.Tasks.Count), e.TaskSetupContext.Task.Name);
                b.TFBuild.Commands.SetProgress(GetProgress(currentIndex, _engine.Tasks.Count), string.Empty);
                var g = e.TaskSetupContext.TFBuild()
                    .Commands.CreateNewRecord(currentTask.Name, "build", TaskRecords.Count + 1,
                        new TFBuildRecordData() {StartTime = DateTime.Now, ParentRecord = _parentRecord, Progress = 0});
                TaskRecords.Add(currentTask.Name, g);
            }
        }

        private int GetProgress(int currentTask, int count)
        {
            var f = (double) currentTask / (double) count * 100;
            return Convert.ToInt32(Math.Truncate(f));
        }

        private void BuildSetup(object sender, SetupEventArgs e)
        {
            var b = e.Context.BuildSystem();
            if (b.IsRunningOnTFS || b.IsRunningOnVSTS)
            {
                //e.Context.TFBuild().Commands.SetProgress(0, "Build Setup");
                e.Context.TFBuild().Commands.SetProgress(0, string.Empty);
                var g = e.Context.TFBuild()
                    .Commands.CreateNewRecord("Cake Build", "build", 0, new TFBuildRecordData {StartTime = DateTime.Now});
                _parentRecord = g;
            }
        }

        private Guid _parentRecord;
        private Dictionary<string, Guid> TaskRecords { get; } = new Dictionary<string, Guid>();
    }
}