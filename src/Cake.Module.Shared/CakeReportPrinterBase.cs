using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        /// <summary>
        /// Gets a value indicating whether durations are rendered as humanized timespans.
        /// </summary>
        /// <remarks>
        /// Controlled by the <c>BuildSystems_HumanizedTimespans</c> Cake configuration setting. It is off
        /// by default, so a report keeps the full <see cref="TimeSpan"/> precision Cake itself uses unless
        /// the setting is turned on.
        /// </remarks>
        protected bool UseHumanizedTimespans
        {
            get
            {
                var configuration = _context?.Configuration;
                return configuration != null && configuration.GetConfigFlag("BuildSystems_HumanizedTimespans");
            }
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
        /// <param name="report">The report to write.</param>
        protected void WriteToConsole(CakeReport report)
        {
            var includeSkippedReasonColumn = report.Any(r => !string.IsNullOrEmpty(r.SkippedMessage));

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
            if (includeSkippedReasonColumn)
            {
                _console.WriteLine(lineFormat, "Task", "Duration", "Status", "Skip Reason");
            }
            else
            {
                _console.WriteLine(lineFormat, "Task", "Duration", "Status");
            }
            _console.WriteLine(new string('-', 20 + maxTaskNameLength));

            // Write task status.
            foreach (var item in report)
            {
                if (ShouldWriteTask(item))
                {
                    _console.ForegroundColor = GetItemForegroundColor(item);
                    if (includeSkippedReasonColumn)
                    {
                        _console.WriteLine(lineFormat, item.TaskName, FormatDuration(item), item.ExecutionStatus.ToReportStatus(), item.SkippedMessage);
                    }
                    else
                    {
                        _console.WriteLine(lineFormat, item.TaskName, FormatDuration(item), item.ExecutionStatus.ToReportStatus());
                    }
                }
            }

            // Write footer.
            _console.ForegroundColor = ConsoleColor.Green;
            _console.WriteLine(new string('-', 20 + maxTaskNameLength));
            _console.WriteLine(lineFormat, "Total:", FormatTime(GetTotalTime(report)));
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
        /// Formats a <see cref="TimeSpan"/>, honouring <see cref="UseHumanizedTimespans"/>.
        /// </summary>
        /// <param name="time">The <see cref="TimeSpan"/> to format.</param>
        /// <returns>A formatted string.</returns>
        protected string FormatTime(TimeSpan time)
        {
            return UseHumanizedTimespans
                ? time.Humanize()
                : time.ToString("c", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the formatted time it took to process one <see cref="CakeReportEntry"/>,
        /// honouring <see cref="UseHumanizedTimespans"/>.
        /// </summary>
        /// <param name="item">The <see cref="CakeReportEntry"/>.</param>
        /// <returns>The formatted time.</returns>
        protected string FormatDuration(CakeReportEntry item)
        {
            if (item.ExecutionStatus == CakeTaskExecutionStatus.Skipped)
            {
                // A humanized report leaves the cell of a task that never ran empty. Full precision keeps
                // Cake's dash, so that turning the setting off leaves the report exactly as it was.
                return UseHumanizedTimespans ? string.Empty : "-";
            }

            return FormatTime(item.Duration);
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
        /// Calculates the foreground color required to write out one <see cref="CakeReportEntry"/>.
        /// </summary>
        /// <param name="item">The <see cref="CakeReportEntry"/>.</param>
        /// <returns>The calculated <see cref="ConsoleColor"/>.</returns>
        protected static ConsoleColor GetItemForegroundColor(CakeReportEntry item)
        {
            return item.ExecutionStatus == CakeTaskExecutionStatus.Executed ? ConsoleColor.Green : ConsoleColor.Gray;
        }
    }
}
