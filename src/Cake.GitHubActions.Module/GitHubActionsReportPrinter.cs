using System;
using Cake.Common.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;
using JetBrains.Annotations;

namespace Cake.GitHubActions.Module
{
    /// <summary>
    /// The GitHub Actions report printer.
    /// </summary>
    [UsedImplicitly]
    public class GitHubActionsReportPrinter : CakeReportPrinterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubActionsReportPrinter"/> class.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="context">The context.</param>
        public GitHubActionsReportPrinter(IConsole console, ICakeContext context)
            : base(console, context)
        {
        }

        /// <inheritdoc />
        public override void Write(CakeReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            try
            {
                if (_context.GitHubActions().IsRunningOnGitHubActions)
                {
                    _context.GitHubActions().Commands.SetStepSummary(RenderMarkdownReport(report));
                }

                WriteToConsole(report);
            }
            finally
            {
                _console.ResetColor();
            }
        }

        /// <inheritdoc />
        public override void WriteLifeCycleStep(string name, Verbosity verbosity)
        {
            // Intentionally left blank
        }

        /// <inheritdoc />
        public override void WriteSkippedStep(string name, Verbosity verbosity)
        {
            // Intentionally left blank
        }

        /// <inheritdoc />
        public override void WriteStep(string name, Verbosity verbosity)
        {
            // Intentionally left blank
        }
    }
}
