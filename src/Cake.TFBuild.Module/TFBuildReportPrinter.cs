using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Cake.Common.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Cake.TFBuild.Module
{
    public class TFBuildReportPrinter : ICakeReportPrinter
    {
        private readonly ICakeContext _context;
        private readonly IConsole _console;

        public TFBuildReportPrinter(IConsole console, ICakeContext context)
        {
            _context = context;
            _console = console;
        }

        public void Write(CakeReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            try
            {

                if (_context.TFBuild().IsRunningOnTFS || _context.TFBuild().IsRunningOnVSTS) {
                    WriteToMarkdown(report);
                }
                WriteToConsole(report);
            }
            catch
            {
            }
        }

        private void WriteToMarkdown(CakeReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("|Task|Status|Duration|");
            sb.AppendLine("|:--:|:----:|:------:|");
            foreach (var item in report)
            {
                if (ShouldWriteTask(item))
                {
                    sb.AppendLine(string.Format("|{0}|{1}|{2}|", item.TaskName, item.ExecutionStatus, FormatDuration(item)));
                }
            }
            sb.AppendLine("");
            var b = _context.BuildSystem().TFBuild;
            FilePath agentWorkPath = b.Environment.Agent.WorkingDirectory + "/tasksummary.md";
            var absFilePath = agentWorkPath.MakeAbsolute(_context.Environment);
            var file = _context.FileSystem.GetFile(absFilePath);
            using (var writer = new StreamWriter(file.OpenWrite())) {
                writer.Write(sb.ToString());
            }
            b.Commands.UploadTaskSummary(absFilePath);
        }

        private void WriteToConsole(CakeReport report)
        {
            var maxTaskNameLength = 29;
            foreach (var item in report)
            {
                if (item.TaskName.Length > maxTaskNameLength)
                {
                    maxTaskNameLength = item.TaskName.Length;
                }
            }

            maxTaskNameLength++;
            string lineFormat = "{0,-" + maxTaskNameLength + "}{1,-20}";
            _console.ForegroundColor = ConsoleColor.Green;

            // Write header.
            _console.WriteLine();
            _console.WriteLine(lineFormat, "Task", "Duration");
            _console.WriteLine(new string('-', 20 + maxTaskNameLength));

            // Write task status.
            foreach (var item in report)
            {
                if (ShouldWriteTask(item))
                {
                    _console.ForegroundColor = GetItemForegroundColor(item);
                    _console.WriteLine(lineFormat, item.TaskName, FormatDuration(item));
                }
            }

            // Write footer.
            _console.ForegroundColor = ConsoleColor.Green;
            _console.WriteLine(new string('-', 20 + maxTaskNameLength));
            _console.WriteLine(lineFormat, "Total:", FormatTime(GetTotalTime(report)));
        }

        private bool ShouldWriteTask(CakeReportEntry item)
        {
            if (item.ExecutionStatus == CakeTaskExecutionStatus.Delegated)
            {
                return _context.Log.Verbosity >= Verbosity.Verbose;
            }

            return true;
        }

        private static string FormatTime(TimeSpan time)
        {
            return time.ToString("c", CultureInfo.InvariantCulture);
        }

        private static TimeSpan GetTotalTime(IEnumerable<CakeReportEntry> entries)
        {
            return entries.Select(i => i.Duration)
                .Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
        }

        private static string FormatDuration(CakeReportEntry item)
        {
            if (item.ExecutionStatus == CakeTaskExecutionStatus.Skipped)
            {
                return "Skipped";
            }

            return FormatTime(item.Duration);
        }

        private static ConsoleColor GetItemForegroundColor(CakeReportEntry item)
        {
            return item.ExecutionStatus == CakeTaskExecutionStatus.Executed ? ConsoleColor.Green : ConsoleColor.Gray;
        }
    }
}