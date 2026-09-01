using System;
using System.IO;
using System.Linq;
using Cake.Common.Build;
using Cake.Core;
using Cake.Core.IO;
using Cake.Module.Shared;
using JetBrains.Annotations;

namespace Cake.AzurePipelines.Module
{
    /// <summary>
    /// The TF Build/Azure Pipelines report printer.
    /// </summary>
    [UsedImplicitly]
    public class AzurePipelinesReportPrinter : CakeReportPrinterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzurePipelinesReportPrinter"/> class.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="context">The context.</param>
        public AzurePipelinesReportPrinter(IConsole console, ICakeContext context)
            : base(console, context)
        {
        }

        /// <summary>
        /// Writes the specified report to a target.
        /// </summary>
        /// <param name="report">The report to write.</param>
        public override void Write(CakeReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            try
            {
                if (_context.AzurePipelines().IsRunningOnAzurePipelines)
                {
                    WriteToMarkdown(report);
                }

                WriteToConsole(report);
            }
            finally
            {
                _console.ResetColor();
            }
        }

        private void WriteToMarkdown(CakeReport report)
        {
            // Tasks run in dependency order, so the last one of them is the target that was requested.
            // The setup and teardown entries surrounding them are not tasks and must not be picked here.
            var targetName = report.LastOrDefault(e => e.Category == CakeReportEntryCategory.Task)?.TaskName ?? string.Empty;

            var b = _context.BuildSystem().AzurePipelines;
            FilePath agentWorkPath = b.Environment.Build.ArtifactStagingDirectory + "/tasksummary.md";
            var absFilePath = agentWorkPath.MakeAbsolute(_context.Environment);
            var file = _context.FileSystem.GetFile(absFilePath);
            using (var writer = new StreamWriter(file.OpenWrite()))
            {
                writer.Write(RenderMarkdownReport(report));
            }

            // The target name is passed as an argument, so that curly braces in it cannot be mistaken
            // for a format placeholder.
            _console.WriteLine(
                "##vso[task.addattachment type=Distributedtask.Core.Summary;name=Cake {0} Build Summary;]{1}",
                targetName,
                absFilePath.FullPath);
        }
    }
}
