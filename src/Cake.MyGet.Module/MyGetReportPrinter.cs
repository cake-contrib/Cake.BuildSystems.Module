using System;
using Cake.Common.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;
using JetBrains.Annotations;

namespace Cake.MyGet.Module
{
    /// <summary>
    /// Implementation of <see cref="ICakeReportPrinter"/> for MyGet.
    /// </summary>
    [UsedImplicitly]
    public class MyGetReportPrinter : CakeReportPrinterBase
    {
        private ICakeLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyGetReportPrinter"/> class.
        /// </summary>
        /// <param name="console">Implementation of <see cref="IConsole"/>.</param>
        /// <param name="log">Implementation of <see cref="ICakeLog"/>.</param>
        /// <param name="context">Implementation of <see cref="ICakeContext"/>.</param>
        public MyGetReportPrinter(IConsole console, ICakeLog log, ICakeContext context)
            : base(console, context)
        {
            _log = log;
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
                if (_context.MyGet().IsRunningOnMyGet)
                {
                    WriteToBuildLog(report);
                }

                WriteToConsole(report);
            }
            finally
            {
                _console.ResetColor();
            }
        }

        private static string Escape(string text)
        {
            return text.Replace("|", "||")
                .Replace("'", "|'")
                .Replace("\n", "|n")
                .Replace("\r", "|r")
                .Replace("[", "|[")
                .Replace("]", "|]");
        }

        private void WriteToBuildLog(CakeReport report)
        {
            RenderTextReport(report, (entry, text) =>
            {
                // MyGet has no colors in its build log, but it does highlight messages by status, which is
                // the closest equivalent for calling out a task that failed.
                var status = entry?.ExecutionStatus == CakeTaskExecutionStatus.Failed ? "ERROR" : "NORMAL";
                _log.Write(
                    Verbosity.Quiet,
                    LogLevel.Information,
                    "##myget[message text='{0}' status='{1}']",
                    Escape(text),
                    status);
            });
        }
    }
}
