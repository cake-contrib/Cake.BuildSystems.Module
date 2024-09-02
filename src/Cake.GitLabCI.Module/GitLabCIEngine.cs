using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;

using JetBrains.Annotations;

namespace Cake.GitLabCI.Module
{
    /// <summary>
    /// <see cref="ICakeEngine"/> implementation for GitLab CI.
    /// </summary>
    /// <remarks>
    /// This engine emits additional console output to make GitLab CI render the output of the indiviudal Cake tasks as collapsible sections
    /// (see <see href="https://docs.gitlab.com/ee/ci/yaml/script.html#custom-collapsible-sections">Custom collapsible sections (GitLab Docs)</see>).
    /// </remarks>
    [UsedImplicitly]
    public sealed class GitLabCIEngine : CakeEngineBase
    {
        private readonly IConsole _console;
        private readonly object _sectionNameLock = new object();
        private readonly Dictionary<string, string> _taskSectionNames = new Dictionary<string, string>();
        private readonly HashSet<string> _sectionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabCIEngine"/> class.
        /// </summary>
        /// <param name="dataService">Implementation of <see cref="ICakeDataService"/>.</param>
        /// <param name="log">Implementation of <see cref="ICakeLog"/>.</param>
        /// <param name="console">Implementation of <see cref="IConsole"/>.</param>
        public GitLabCIEngine(ICakeDataService dataService, ICakeLog log, IConsole console)
            : base(new CakeEngine(dataService, log))
        {
            _console = console;
            _engine.BeforeSetup += OnBeforeSetup;
            _engine.AfterSetup += OnAfterSetup;
            _engine.BeforeTaskSetup += OnBeforeTaskSetup;
            _engine.AfterTaskTeardown += OnAfterTaskTeardown;
            _engine.BeforeTeardown += OnBeforeTeardown;
            _engine.AfterTeardown += OnAfterTeardown;
        }

        private void OnBeforeSetup(object sender, BeforeSetupEventArgs e)
        {
            WriteSectionStart("Setup");
        }

        private void OnAfterSetup(object sender, AfterSetupEventArgs e)
        {
            WriteSectionEnd("Setup");
        }

        private void OnBeforeTaskSetup(object sender, BeforeTaskSetupEventArgs e)
        {
            WriteSectionStart(GetSectionNameForTask(e.TaskSetupContext.Task.Name), e.TaskSetupContext.Task.Name);
        }

        private void OnAfterTaskTeardown(object sender, AfterTaskTeardownEventArgs e)
        {
            WriteSectionEnd(GetSectionNameForTask(e.TaskTeardownContext.Task.Name));
        }

        private void OnBeforeTeardown(object sender, BeforeTeardownEventArgs e)
        {
            WriteSectionStart("Teardown");
        }

        private void OnAfterTeardown(object sender, AfterTeardownEventArgs e)
        {
            WriteSectionEnd("Teardown");
        }

        private void WriteSectionStart(string sectionName, string sectionHeader = null)
        {
            _console.WriteLine("{0}", $"{AnsiEscapeCodes.SectionMarker}section_start:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:{sectionName}\r{AnsiEscapeCodes.SectionMarker}{AnsiEscapeCodes.ForegroundBlue}{sectionHeader ?? sectionName}{AnsiEscapeCodes.Reset}");
        }

        private void WriteSectionEnd(string sectionName)
        {
            _console.WriteLine("{0}", $"{AnsiEscapeCodes.SectionMarker}section_end:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:{sectionName}\r{AnsiEscapeCodes.SectionMarker}");
        }

        /// <summary>
        /// Computes a unique GitLab CI section name for a task name.
        /// </summary>
        /// <remarks>
        /// GitLab CI requires a section name in both the "start" and "end" markers of a section.
        /// The name can only be composed of letters, numbers, and the _, ., or - characters.
        /// In Cake, each task corresponds to one section.
        /// Since the task name may contain characters not allowed in the section name, unsupprted characters are removed from the task name.
        /// Additionally, this method ensures that the section name is unique and the same task name will be mapped to the same section name for each call.
        /// </remarks>
        private string GetSectionNameForTask(string taskName)
        {
            lock (_sectionNameLock)
            {
                // If there is already a section name for the task, reuse the same name
                if (_taskSectionNames.TryGetValue(taskName, out var sectionName))
                {
                    return sectionName;
                }

                // Remove unsuported characters from the task name (everything except letters, numbers or the _, ., and - characters
                var normalizedTaskName = Regex.Replace(taskName, "[^A-Z|a-z|0-9|_|\\-|\\.]*", string.Empty).ToLowerInvariant();

                // Normalizing the task name can cause multiple tasks to be mapped to the same section name
                // To avoid name conflicts, append a number to the end to make the section name unique.
                sectionName = normalizedTaskName;
                var sectionCounter = 0;
                while (!_sectionNames.Add(sectionName))
                {
                    sectionName = string.Concat(sectionName, "_", sectionCounter++);
                }

                // Save task name -> section name mapping for subsequent calls of GetSectionNameForTask()
                _taskSectionNames.Add(taskName, sectionName);
                return sectionName;
            }
        }
    }
}
