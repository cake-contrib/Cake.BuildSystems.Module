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

        private void WriteToBuildLog(CakeReport report)
        {
            var b = _context.MyGet();

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

            foreach (var entry in report)
            {
                if (ShouldWriteTask(entry))
                {
                    _log.Write(Verbosity.Quiet, LogLevel.Information,
                            "##myget[message text='{0}' status='NORMAL']", string.Format(lineFormat, entry.TaskName, FormatDuration(entry)));
                }
            }

            // Write footer.
            _console.WriteLine(lineFormat, "Total:", FormatTime(GetTotalTime(report)));
        }
    }
}
