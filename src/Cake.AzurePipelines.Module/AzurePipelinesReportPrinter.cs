using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

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
        private static readonly string TaskColumnHeader = "Task";
        private static readonly string DurationColumnHeader = "Duration";
        private static readonly string StatusColumnHeader = "Status";
        private static readonly string SkipReasonColumnHeader = "Skip Reason";
        private static readonly int TaskColumnMinWidth = 29;
        private static readonly int ColumnGap = 2;
        private static readonly ConsoleColor TableColor = ConsoleColor.Green;

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

                WriteToLog(report);
            }
            finally
            {
                _console.ResetColor();
            }
        }

        private static string FormatLine(string lineFormat, params object[] columns)
        {
            // The trailing column is padded to a fixed width, which would leave every line with a
            // tail of spaces that serves no purpose in a build log.
            return string.Format(CultureInfo.InvariantCulture, lineFormat, columns).TrimEnd();
        }

        private static int MaxLength(IEnumerable<string> values)
        {
            return values.Select(v => v?.Length ?? 0).DefaultIfEmpty(0).Max();
        }

        private static ConsoleColor GetStatusColor(CakeReportEntry item)
        {
            if (item.Category == CakeReportEntryCategory.Setup || item.Category == CakeReportEntryCategory.Teardown)
            {
                return ConsoleColor.Cyan;
            }

            switch (item.ExecutionStatus)
            {
                case CakeTaskExecutionStatus.Failed:
                    return ConsoleColor.Red;
                case CakeTaskExecutionStatus.Executed:
                    return ConsoleColor.Green;
                default:
                    return ConsoleColor.Gray;
            }
        }

        private static string GetStatusIcon(CakeReportEntry item)
        {
            // Check mark, skip-forward and cross mark, spelled out as escapes to keep this file ASCII.
            switch (item.ExecutionStatus)
            {
                case CakeTaskExecutionStatus.Executed:
                case CakeTaskExecutionStatus.Delegated:
                    return "\u2705";
                case CakeTaskExecutionStatus.Skipped:
                    return "\u23ED\uFE0F";
                case CakeTaskExecutionStatus.Failed:
                    return "\u274C";
                default:
                    return string.Empty;
            }
        }

        private static string EscapeMarkdown(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("|", "\\|")
                .Replace("\r\n", " ")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        /// <summary>
        /// Writes the report to the build log as an aligned table, colour coded per task status.
        /// </summary>
        /// <remarks>
        /// Replaces <see cref="CakeReportPrinterBase.WriteToConsole"/>, which neither renders the
        /// status and skip reason columns nor produces colours that survive the Azure Pipelines log
        /// viewer. Kept local to this module until the same treatment can be verified on the other
        /// build systems.
        /// </remarks>
        /// <param name="report">The report to write.</param>
        private void WriteToLog(CakeReport report)
        {
            var entries = report.Where(ShouldWriteTask).ToList();
            var includeSkipReasonColumn = entries.Any(e => !string.IsNullOrEmpty(e.SkippedMessage));
            var totalDuration = FormatTime(GetTotalTime(report));

            // Durations are no longer a fixed 16 characters wide, so every column is sized to what it
            // actually holds. Otherwise a humanized duration would sit in a wide sea of padding.
            var taskColumnWidth = Math.Max(TaskColumnMinWidth, MaxLength(entries.Select(e => e.TaskName))) + ColumnGap;
            var durationColumnWidth = Math.Max(
                DurationColumnHeader.Length,
                MaxLength(entries.Select(e => FormatDuration(e)).Concat(new[] { totalDuration })));
            var statusColumnWidth = Math.Max(
                StatusColumnHeader.Length,
                MaxLength(entries.Select(e => e.ExecutionStatus.ToReportStatus()))) + ColumnGap;

            // The duration is right aligned like the number it is, so its gap has to follow the column
            // instead of being padding inside it.
            var lineFormat = "{0,-" + taskColumnWidth + "}{1," + durationColumnWidth + "}"
                + new string(' ', ColumnGap) + "{2,-" + statusColumnWidth + "}";
            var separatorWidth = taskColumnWidth + durationColumnWidth + ColumnGap + statusColumnWidth;

            if (includeSkipReasonColumn)
            {
                separatorWidth += Math.Max(SkipReasonColumnHeader.Length, MaxLength(entries.Select(e => e.SkippedMessage)));
                lineFormat += "{3}";
            }

            var separator = new string('-', separatorWidth);

            // Write header.
            _console.WriteLine();
            WriteLine(TableColor, FormatLine(lineFormat, TaskColumnHeader, DurationColumnHeader, StatusColumnHeader, SkipReasonColumnHeader));
            WriteLine(TableColor, separator);

            // Write task status.
            foreach (var item in entries)
            {
                WriteLine(GetStatusColor(item), FormatLine(lineFormat, item.TaskName, FormatDuration(item), item.ExecutionStatus.ToReportStatus(), item.SkippedMessage));
            }

            // Write footer.
            WriteLine(TableColor, separator);
            WriteLine(TableColor, FormatLine(lineFormat, "Total:", totalDuration, string.Empty, string.Empty));
        }

        /// <summary>
        /// Renders the report as a Markdown table for the Cake Build Summary widget.
        /// </summary>
        /// <remarks>
        /// Markdown has no notion of colours, so the execution status carries an emoji instead.
        /// Setup and teardown entries are set in italics to tell them apart from regular tasks.
        /// </remarks>
        /// <param name="report">The report to render.</param>
        /// <returns>The report as a Markdown table.</returns>
        private string RenderMarkdown(CakeReport report)
        {
            var entries = report.Where(ShouldWriteTask).ToList();
            var includeSkipReasonColumn = entries.Any(e => !string.IsNullOrEmpty(e.SkippedMessage));

            var sb = new StringBuilder();
            sb.AppendLine(string.Empty);
            sb.AppendLine(includeSkipReasonColumn
                ? "|Task|Duration|Status|Skip Reason|"
                : "|Task|Duration|Status|");
            sb.AppendLine(includeSkipReasonColumn
                ? "|:---|-------:|:-----|:----------|"
                : "|:---|-------:|:-----|");

            foreach (var item in entries)
            {
                var taskName = EscapeMarkdown(item.TaskName);
                if (item.Category != CakeReportEntryCategory.Task)
                {
                    taskName = "_" + taskName + "_";
                }

                var status = GetStatusIcon(item) + " " + item.ExecutionStatus.ToReportStatus();
                sb.AppendLine(includeSkipReasonColumn
                    ? $"|{taskName}|{FormatDuration(item)}|{status}|{EscapeMarkdown(item.SkippedMessage)}|"
                    : $"|{taskName}|{FormatDuration(item)}|{status}|");
            }

            var total = $"|**Total:**|**{FormatTime(GetTotalTime(report))}**|";
            sb.AppendLine(includeSkipReasonColumn ? total + "||" : total + "|");
            sb.AppendLine(string.Empty);

            return sb.ToString();
        }

        private void WriteToMarkdown(CakeReport report)
        {
            // Tasks run in dependency order, so the last one of them is the target that was
            // requested. The setup and teardown entries surrounding them are not tasks and must not
            // be picked here.
            var targetName = report.LastOrDefault(e => e.Category == CakeReportEntryCategory.Task)?.TaskName ?? string.Empty;

            var b = _context.BuildSystem().AzurePipelines;
            FilePath agentWorkPath = b.Environment.Build.ArtifactStagingDirectory + "/tasksummary.md";
            var absFilePath = agentWorkPath.MakeAbsolute(_context.Environment);
            var file = _context.FileSystem.GetFile(absFilePath);
            using (var writer = new StreamWriter(file.OpenWrite()))
            {
                writer.Write(RenderMarkdown(report));
            }

            // The target name is passed as an argument, so that curly braces in it cannot be
            // mistaken for a format placeholder.
            _console.WriteLine(
                "##vso[task.addattachment type=Distributedtask.Core.Summary;name=Cake {0} Build Summary;]{1}",
                targetName,
                absFilePath.FullPath);
        }

        private void WriteLine(ConsoleColor color, string text)
        {
            // The text is always passed as an argument and never as the format string, so that curly
            // braces in a task name or skip reason cannot be mistaken for a format placeholder.
            if (_console.SupportAnsiEscapeCodes)
            {
                _console.WriteLine(AnsiEscapeCodes.GetForeground(color) + "{0}" + AnsiEscapeCodes.Reset, text);
            }
            else
            {
                _console.ForegroundColor = color;
                _console.WriteLine("{0}", text);
            }
        }
    }
}
