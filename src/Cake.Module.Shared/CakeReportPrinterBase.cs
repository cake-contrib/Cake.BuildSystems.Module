using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Cake.Core;
using Cake.Core.Diagnostics;

namespace Cake.Module.Shared
{
    /// <summary>
    /// Base-implementation for <see cref="ICakeReportPrinter"/>.
    /// </summary>
    public abstract class CakeReportPrinterBase : ICakeReportPrinter
    {
        /// <summary>
        /// Gets the <see cref="ICakeContext"/>.
        /// </summary>
        // ReSharper disable once SA1401
        protected readonly ICakeContext _context;

        /// <summary>
        /// Gets the <see cref="IConsole"/>.
        /// </summary>
        // ReSharper disable once SA1401
        protected readonly IConsole _console;

        private static readonly string TaskColumnHeader = "Task";
        private static readonly string DurationColumnHeader = "Duration";
        private static readonly string StatusColumnHeader = "Status";
        private static readonly string SkipReasonColumnHeader = "Skip Reason";
        private static readonly int TaskColumnMinWidth = 29;
        private static readonly int DurationColumnWidth = 20;
        private static readonly int StatusColumnWidth = 20;
        private static readonly ConsoleColor TableColor = ConsoleColor.Green;

        /// <summary>
        /// Initializes a new instance of the <see cref="CakeReportPrinterBase"/> class.
        /// </summary>
        /// <param name="console">The <see cref="IConsole"/>.</param>
        /// <param name="context">The <see cref="ICakeContext"/>.</param>
        // ReSharper disable once PublicConstructorInAbstractClass
        public CakeReportPrinterBase(IConsole console, ICakeContext context)
        {
            _context = context;
            _console = console;
        }

        /// <inheritdoc />
        public abstract void Write(CakeReport report);

        /// <inheritdoc/>
        public virtual void WriteLifeCycleStep(string name, Verbosity verbosity)
        {
            new CakeReportPrinter(_console, _context).WriteLifeCycleStep(name, verbosity);
        }

        /// <inheritdoc/>
        public virtual void WriteStep(string name, Verbosity verbosity)
        {
            new CakeReportPrinter(_console, _context).WriteLifeCycleStep(name, verbosity);
        }

        /// <inheritdoc/>
        public virtual void WriteSkippedStep(string name, Verbosity verbosity)
        {
            new CakeReportPrinter(_console, _context).WriteLifeCycleStep(name, verbosity);
        }

        /// <summary>
        /// Writes the report to the <see cref="IConsole"/>.
        /// </summary>
        /// <remarks>
        /// The status of every task is color coded, the same way it is when the build runs in a regular
        /// terminal. Because most build systems show their logs in a web UI where assigning
        /// <see cref="IConsole.ForegroundColor"/> has no effect, the colors are emitted as ANSI escape
        /// sequences whenever the console reports that it understands them.
        /// </remarks>
        /// <param name="report">The report to write.</param>
        protected void WriteToConsole(CakeReport report)
        {
            _console.WriteLine();
            RenderTextReport(
                report,
                (entry, text) => WriteLine(entry == null ? TableColor : GetItemForegroundColor(entry), text));
        }

        /// <summary>
        /// Renders the report as an aligned plain text table and hands it to <paramref name="writeLine"/>
        /// one line at a time.
        /// </summary>
        /// <param name="report">The report to render.</param>
        /// <param name="writeLine">
        /// Called once per line of the table. Receives the <see cref="CakeReportEntry"/> the line describes,
        /// or <c>null</c> for the header, separator and total lines, along with the rendered line itself.
        /// </param>
        protected void RenderTextReport(CakeReport report, Action<CakeReportEntry, string> writeLine)
        {
            var entries = report.Where(ShouldWriteTask).ToList();
            var includeSkipReasonColumn = entries.Any(e => !string.IsNullOrEmpty(e.SkippedMessage));

            var taskColumnWidth = Math.Max(TaskColumnMinWidth, MaxLength(entries.Select(e => e.TaskName))) + 1;
            var separatorWidth = taskColumnWidth + DurationColumnWidth + StatusColumnWidth;
            var lineFormat = "{0,-" + taskColumnWidth + "}{1,-" + DurationColumnWidth + "}{2,-" + StatusColumnWidth + "}";

            if (includeSkipReasonColumn)
            {
                separatorWidth += Math.Max(SkipReasonColumnHeader.Length, MaxLength(entries.Select(e => e.SkippedMessage)));
                lineFormat += "{3}";
            }

            var separator = new string('-', separatorWidth);

            // Write header.
            writeLine(null, FormatLine(lineFormat, TaskColumnHeader, DurationColumnHeader, StatusColumnHeader, SkipReasonColumnHeader));
            writeLine(null, separator);

            // Write task status.
            foreach (var item in entries)
            {
                writeLine(item, FormatLine(lineFormat, item.TaskName, FormatDuration(item), item.ExecutionStatus.ToReportStatus(), item.SkippedMessage));
            }

            // Write footer.
            writeLine(null, separator);
            writeLine(null, FormatLine(lineFormat, "Total:", FormatTime(GetTotalTime(report)), string.Empty, string.Empty));
        }

        /// <summary>
        /// Renders the report as a Markdown table, for build systems that show a build summary in their web UI.
        /// </summary>
        /// <remarks>
        /// Markdown has no notion of colors, so the execution status carries an emoji instead. Setup and
        /// teardown entries are set in italics to tell them apart from regular tasks.
        /// </remarks>
        /// <param name="report">The report to render.</param>
        /// <returns>The report as a Markdown table.</returns>
        protected string RenderMarkdownReport(CakeReport report)
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

        /// <summary>
        /// Check if the <see cref="CakeReportEntry"/> should be written.
        /// </summary>
        /// <param name="item">The <see cref="CakeReportEntry"/> to check.</param>
        /// <returns><c>true</c>, if the <see cref="CakeReportEntry"/> should be written.</returns>
        protected bool ShouldWriteTask(CakeReportEntry item)
        {
            if (item.ExecutionStatus == CakeTaskExecutionStatus.Delegated)
            {
                return _context.Log.Verbosity >= Verbosity.Verbose;
            }

            return true;
        }

        /// <summary>
        /// Formats a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="time">The <see cref="TimeSpan"/> to format.</param>
        /// <returns>A formatted string.</returns>
        protected static string FormatTime(TimeSpan time)
        {
            return time.ToString("c", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Calculates the total time it took to process all <see cref="CakeReportEntry"/> elements.
        /// </summary>
        /// <param name="entries">The entries to calculate.</param>
        /// <returns>The sum of time it took to perform all entries.</returns>
        protected static TimeSpan GetTotalTime(IEnumerable<CakeReportEntry> entries)
        {
            return entries.Select(i => i.Duration)
                .Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
        }

        /// <summary>
        /// Returns the formatted time it took to process one <see cref="CakeReportEntry"/>.
        /// </summary>
        /// <param name="item">The <see cref="CakeReportEntry"/>.</param>
        /// <returns>The formatted time.</returns>
        protected static string FormatDuration(CakeReportEntry item)
        {
            if (item.ExecutionStatus == CakeTaskExecutionStatus.Skipped)
            {
                return "-";
            }

            return FormatTime(item.Duration);
        }

        /// <summary>
        /// Calculates the foreground color required to write out one <see cref="CakeReportEntry"/>.
        /// </summary>
        /// <param name="item">The <see cref="CakeReportEntry"/>.</param>
        /// <returns>The calculated <see cref="ConsoleColor"/>.</returns>
        protected static ConsoleColor GetItemForegroundColor(CakeReportEntry item)
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

        /// <summary>
        /// Returns the emoji that visualizes the execution status of one <see cref="CakeReportEntry"/>.
        /// </summary>
        /// <param name="item">The <see cref="CakeReportEntry"/>.</param>
        /// <returns>The emoji for the execution status, or an empty string for an unknown status.</returns>
        protected static string GetStatusIcon(CakeReportEntry item)
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

        /// <summary>
        /// Escapes the characters that would otherwise break out of a Markdown table cell.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>The escaped value.</returns>
        protected static string EscapeMarkdown(string value)
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

        private static string FormatLine(string lineFormat, params object[] columns)
        {
            // The trailing column is padded to a fixed width, which would leave every line with a tail of
            // spaces that serves no purpose in a build log.
            return string.Format(CultureInfo.InvariantCulture, lineFormat, columns).TrimEnd();
        }

        private static int MaxLength(IEnumerable<string> values)
        {
            return values.Select(v => v?.Length ?? 0).DefaultIfEmpty(0).Max();
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
